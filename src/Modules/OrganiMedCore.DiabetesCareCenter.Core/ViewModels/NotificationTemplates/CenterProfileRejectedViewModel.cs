using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates
{
    public class CenterProfileRejectedViewModel
    {
        public string PersonName { get; set; }

        public string CenterName { get; set; }

        public string RejectReason { get; set; }

        public string CurrentRole { get; set; }

        public Guid Nonce { get; set; }
    }
}
