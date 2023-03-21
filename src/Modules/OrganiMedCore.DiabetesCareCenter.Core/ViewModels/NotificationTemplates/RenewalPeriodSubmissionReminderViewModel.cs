using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates
{
    public class RenewalPeriodSubmissionReminderViewModel
    {
        public string CenterName { get; set; }

        public string LeaderName { get; set; }

        public DateTime Deadline { get; set; }

        public Guid Nonce { get; set; }
    }
}
