using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface IAccreditationStatusCalculator
    {
        Task<AccreditationStatusResult> CalculateAccreditationStatusAsync(CenterProfilePart part);
    }
}
