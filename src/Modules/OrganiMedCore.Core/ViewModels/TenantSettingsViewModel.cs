using IntelliMed.Core.Extensions;
using OrganiMedCore.Core.Settings;

namespace OrganiMedCore.Core.ViewModels
{
    public class TenantSettingsViewModel
    {
        public bool IsOrganization { get; set; }


        public void Map(TenantSettings model)
        {
            model.ThrowIfNull();

            IsOrganization = model.IsOrganization;
        }
    }
}
