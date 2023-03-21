using IntelliMed.DokiNetIntegration.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface IDiabetesCareCenterService
    {
        Task ChangeCenterProfileLeaderAsync(DokiNetMember nextLeader, IUpdateModel updater);
    }
}
