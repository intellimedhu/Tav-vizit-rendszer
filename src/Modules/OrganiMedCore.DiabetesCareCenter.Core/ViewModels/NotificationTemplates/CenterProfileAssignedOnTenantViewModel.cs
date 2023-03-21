using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates
{
    public class CenterProfileAssignedOnTenantViewModel
    {
        public string LeaderName { get; set; }

        public string CenterName { get; set; }

        public Guid Nonce { get; set; }
    }
}
