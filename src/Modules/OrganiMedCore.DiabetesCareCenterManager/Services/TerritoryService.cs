using Dapper;
using IntelliMed.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class TerritoryService : ITerritoryService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;
        private readonly ISiteService _siteService;
        private readonly UserManager<IUser> _userManager;


        private const string TerritoriesCacheKey = "TerritoryService.Territories";


        public TerritoryService(
            IMemoryCache memoryCache,
            ISession session,
            ISignal signal,
            ISiteService siteService,
            UserManager<IUser> userManager)
        {
            _memoryCache = memoryCache;
            _session = session;
            _signal = signal;
            _siteService = siteService;
            _userManager = userManager;
        }


        public async Task ChangeTerritorialRapporteurAsync(int territoryId, int newUserId)
        {
            var territory = await GetTerritoryAsync(territoryId);
            if (territory == null)
            {
                throw new NotFoundException("A választott terület nem létezik.");
            }

            if (territory.TerritorialRapporteurId == newUserId)
            {
                throw new TerritoryException("Az új személy azonos a kiválasztott személlyel!");
            }

            territory.TerritorialRapporteurId = newUserId;
            _session.Save(territory);

            if (await CacheEnabledAsync())
            {
                ClearTerritoryCache();
            }
        }

        public async Task ChangeConsultantAsync(int territoryId, string name)
        {
            var territory = await GetTerritoryAsync(territoryId);
            if (territory == null)
            {
                throw new NotFoundException("A választott terület nem létezik.");
            }

            if (territory.Consultant == name)
            {
                throw new TerritoryException("Az új személy azonos a megadott személlyel!");
            }

            territory.Consultant = name;
            _session.Save(territory);

            if (await CacheEnabledAsync())
            {
                ClearTerritoryCache();
            }
        }

        public async Task<User> GetRapporteurToSettlementAsync(int centerZipCode, string centerSettlementName)
        {
            var zipCode = centerZipCode.ToString();
            var settlement = await _session
                .Query<Settlement, SettlementIndex>(x =>
                    x.ZipCode == zipCode &&
                    x.Name == centerSettlementName)
                .FirstOrDefaultAsync();

            if (settlement == null)
            {
                throw new SettlementNotFoundException();
            }

            if (!settlement.TerritoryId.HasValue)
            {
                throw new SettlementHasNoTerritoryException();
            }

            var territory = await GetTerritoryAsync(settlement.TerritoryId.Value);
            if (!territory.TerritorialRapporteurId.HasValue)
            {
                throw new TerritoryHasNoRapporteurException();
            }

            return await _session.GetAsync<User>(territory.TerritorialRapporteurId.Value);
        }

        public async Task<IEnumerable<Territory>> GetTerritoriesAsync()
        {
            if (await CacheEnabledAsync())
            {
                return await LoadTerritoriesFromCacheAsync();
            }

            return await _session.Query<Territory>().ListAsync();
        }

        public async Task RemoveTerritoriesFromUserAsync(int userId)
        {
            var territories = await GetTerritoriesForRapporteurAsync(userId);
            if (!territories.Any())
            {
                return;
            }

            foreach (var territory in territories)
            {
                territory.TerritorialRapporteurId = null;

                _session.Save(territory);
            }

            if (await CacheEnabledAsync())
            {
                ClearTerritoryCache();
            }
        }

        public async Task<IEnumerable<IUser>> GetReviewersAsync(int zipCode, string settlementName)
        {
            try
            {
                var territorialRapporteur = await GetRapporteurToSettlementAsync(zipCode, settlementName);
                if (territorialRapporteur != null)
                {
                    return new[] { territorialRapporteur };
                }
            }
            catch (Exception ex) when
                (ex is SettlementNotFoundException ||
                ex is SettlementHasNoTerritoryException ||
                ex is TerritoryHasNoRapporteurException)
            {
                //var errorMessages = new[]
                //{
                //    "Település vagy terület hiba keletkezett a szakellátóhely adatlap bejelentése során",
                //    $"{part.CenterAddress} {part.CenterSettlementName}",
                //    "Hibalehetőségek: Nincs ilyen település, a település nem tartozik területhez vagy a területnek nincs területi referense"
                //};

                //_logger.LogError(ex, ex.Message, errorMessages);
                //await StoreDebugEmailAsync(managerSettings.DebugEmailAddresses, errorMessages);
            }

            var secretaries = await _userManager.GetUsersInRoleAsync(CenterPosts.MDTSecretary);
            if (secretaries != null && secretaries.Any())
            {
                return secretaries;
            }

            //_logger.LogDebug("Nincs MDT titkár felvéve");
            //await StoreDebugEmailAsync(
            //    managerSettings.DebugEmailAddresses,
            //    $"Szakellátóhely bejelentés történt: {part.CenterName}, {part.CenterSettlementName}",
            //    "MDT titkár nincs beállítva",
            //    $"UTC időpont: {_clock.UtcNow}");

            return Enumerable.Empty<User>();
        }

        public async Task<Territory> GetTerritoryAsync(int id)
        {
            if (await CacheEnabledAsync())
            {
                return (await LoadTerritoriesFromCacheAsync())
                    .FirstOrDefault(x => x.Id == id);
            }

            return await _session.GetAsync<Territory>(id);
        }

        public async Task<IDictionary<Territory, IEnumerable<int>>> GetUsedZipCodesByTerritoriesAsync()
        {
            // TODO: Currently there is no method to join index tables without the Document table.
            // Do this better if yessql makes it possible.
            var sql = $"SELECT DISTINCT ti.{nameof(SettlementIndex.Name)} as 'TerritoryName', si.{nameof(SettlementIndex.ZipCode)} as 'ZipCode' " +
                      $"FROM {nameof(TerritoryIndex)} ti " +
                      $"LEFT JOIN {nameof(SettlementIndex)} si on ti.{nameof(ContentItemIndex.DocumentId)} = si.{nameof(SettlementIndex.TerritoryId)} " +
                      $"JOIN {nameof(CenterProfilePartIndex)} cpi on cpi.{nameof(CenterProfilePartIndex.CenterZipCode)} = si.{nameof(SettlementIndex.ZipCode)} " +
                      $"JOIN {nameof(ContentItemIndex)} ci on ci.{nameof(ContentItemIndex.DocumentId)} = cpi.{nameof(ContentItemIndex.DocumentId)} " +
                      $"WHERE ci.{nameof(ContentItemIndex.Latest)} = 1  AND ci.{nameof(ContentItemIndex.Published)} = 1 " +
                      "ORDER BY 2, 1";

            var transaction = await _session.DemandAsync();
            var queryResult = await transaction.Connection.QueryAsync<CustomTerritoryQueryHelper>(sql, null, transaction);

            return queryResult
                .Where(x => x.ZipCode.HasValue)
                .GroupBy(x => x.TerritoryName)
                .ToDictionary(
                    group => new Territory() { Name = group.Key },
                    group => group.Select(x => x.ZipCode.Value));
        }

        public async Task<bool> CacheEnabledAsync()
            => (await _siteService.GetSiteSettingsAsync()).As<CenterManagerSettings>().TerritoryCacheEnabled;

        public void ClearTerritoryCache()
            => _signal.SignalToken(TerritoriesCacheKey);


        private Task<IEnumerable<Territory>> LoadTerritoriesFromCacheAsync()
            => _memoryCache.GetOrCreateAsync("TerritoryService:AllTerritories", entry =>
            {
                entry.AddExpirationToken(_signal.GetToken(TerritoriesCacheKey));

                return _session.Query<Territory>().ListAsync();
            });

        private async Task<IEnumerable<Territory>> GetTerritoriesForRapporteurAsync(int userId)
        {
            if (await CacheEnabledAsync())
            {
                return (await LoadTerritoriesFromCacheAsync())
                    .Where(x => x.TerritorialRapporteurId == userId);
            }

            return await _session
                .Query<Territory, TerritoryIndex>(x => x.TerritorialRapporteurId == userId)
                .ListAsync();
        }


        private class CustomTerritoryQueryHelper
        {
            public string TerritoryName { get; set; }

            public int? ZipCode { get; set; }
        }
    }
}
