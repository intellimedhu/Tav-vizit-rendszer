using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface ITerritoryService
    {
        Task ChangeTerritorialRapporteurAsync(int territoryId, int newUserId);

        Task ChangeConsultantAsync(int territoryId, string name);

        /// <exception cref="SettlementNotFoundException"></exception>
        /// <exception cref="SettlementHasNoTerritoryException"></exception>
        /// <exception cref="TerritoryHasNoRapporteurException"></exception>
        Task<User> GetRapporteurToSettlementAsync(int zipCode, string settlementName);

        Task<IEnumerable<Territory>> GetTerritoriesAsync();

        Task RemoveTerritoriesFromUserAsync(int userId);

        Task<IEnumerable<IUser>> GetReviewersAsync(int zipCode, string settlementName);

        Task<Territory> GetTerritoryAsync(int id);

        Task<IDictionary<Territory, IEnumerable<int>>> GetUsedZipCodesByTerritoriesAsync();

        Task<bool> CacheEnabledAsync();

        void ClearTerritoryCache();
    }
}
