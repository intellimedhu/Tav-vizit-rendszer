using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface ICenterProfileCommonService
    {
        Task<IEnumerable<CenterProfileEditorTerritorySearchViewModel>> SearchTerritoryByZipCodeAsync(int zipCode, string settlementName = null);

        Task<CenterManagerSettings> GetCenterManagerSettingsAsync();
    }
}
