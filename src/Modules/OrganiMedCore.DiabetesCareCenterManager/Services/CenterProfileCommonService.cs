using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class CenterProfileCommonService : ICenterProfileCommonService
    {
        private readonly ICenterUserService _centerUserService;
        private readonly ISession _session;
        private readonly ISiteService _siteService;


        public CenterProfileCommonService(
            ICenterUserService centerUserService,
            ISession session,
            ISiteService siteService)
        {
            _centerUserService = centerUserService;
            _session = session;
            _siteService = siteService;
        }


        public async Task<IEnumerable<CenterProfileEditorTerritorySearchViewModel>> SearchTerritoryByZipCodeAsync(int zipCode, string settlementName = null)
        {
            var query = _session
                .Query<Settlement, SettlementIndex>(x => x.ZipCode == zipCode.ToString());

            if (!string.IsNullOrEmpty(settlementName))
            {
                query = query.Where(x => x.Name == settlementName);
            }

            var settlements = (await query.ListAsync())
                .GroupBy(x => new
                {
                    x.ZipCode,
                    x.Name,
                    x.TerritoryId
                })
                .Select(g => g.Key);

            if (!settlements.Any())
            {
                // TODO: throw SettlementNotFoundException
                return new CenterProfileEditorTerritorySearchViewModel[] { };
            }

            var territories = await _session.GetAsync<Territory>(
                settlements
                .Where(x => x.TerritoryId.HasValue)
                .Select(x => x.TerritoryId.Value)
                .Distinct()
                .ToArray());

            var localUsers = (await _session
                .GetAsync<User>(
                    territories
                        .Where(x => x.TerritorialRapporteurId.HasValue)
                        .Select(x => x.TerritorialRapporteurId.Value)
                        .Distinct()
                        .ToArray()))
                .Where(x => x.RoleNames.Contains(CenterPosts.TerritorialRapporteur));

            var territorialRapporteurs = await _centerUserService.GetUsersByLocalUsersAsync(localUsers);

            return settlements.Select(settlement =>
            {
                var viewModel = new CenterProfileEditorTerritorySearchViewModel()
                {
                    Settlement = settlement.Name,
                    ZipCode = settlement.ZipCode
                };

                var territory = territories.FirstOrDefault(x => x.Id == settlement.TerritoryId);
                if (territory != null)
                {
                    var territorialRapporteur = territorialRapporteurs.FirstOrDefault(x => x.LocalUser.Id == territory.TerritorialRapporteurId);
                    viewModel.TerritorialRapporteur = territorialRapporteur?.DokiNetMember?.FullName;
                }

                return viewModel;
            });
        }

        public async Task<CenterManagerSettings> GetCenterManagerSettingsAsync()
        {
            var settings = await _siteService.GetSiteSettingsAsync();

            return settings.As<CenterManagerSettings>();
        }
    }
}
