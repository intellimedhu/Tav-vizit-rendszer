using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates
{
    public class CenterProfileSubmissionViewModel
    {
        public string CenterLeaderName { get; set; }

        public string CenterName { get; set; }

        public string ReviewerMemberFullName { get; set; }

        public Guid Nonce { get; set; }
    }
}
