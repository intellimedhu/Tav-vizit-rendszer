using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates
{
    public class CenterProfileLeaderChanged
    {
        public string CenterName { get; set; }

        public string NewLeaderName { get; set; }

        public Guid Nonce { get; set; }
    }
}
