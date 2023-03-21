using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenterTenant.ViewModels
{
    public class CenterSettingsViewModel
    {
        public string CenterProfileContentItemId { get; set; }

        public IEnumerable<CenterProfilePart> CenterProfiles { get; set; } = new List<CenterProfilePart>();

        public bool TenantUnavailable { get; set; }
    }
}
