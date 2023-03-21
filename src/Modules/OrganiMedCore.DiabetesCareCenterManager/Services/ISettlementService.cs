using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public interface ISettlementService
    {
        Task<Settlement> GetSettlementAsync(int id);

        Task<IEnumerable<Settlement>> GetSettlementsAsync(int territoryId, int page, string q = null);

        Task<int> GetSettlementsCountAsync(int territoryId, string q);

        Task SaveSettlementAsync(Settlement settlement);

        Task DeleteSettlementAsync(int id);
    }
}
