using System;
using System.Collections.Generic;

namespace OrganiMedCore.Email.Models
{
    public class EmailNotification
    {
        public Guid NotificationId { get; set; }

        public DateTime? ScheduledSendDate { get; set; }

        public DateTime? SentOn { get; set; }

        public HashSet<string> To { get; set; } = new HashSet<string>();

        public HashSet<string> Cc { get; set; } = new HashSet<string>();

        public HashSet<string> Bcc { get; set; } = new HashSet<string>();

        public string TemplateId { get; set; }

        public object Data { get; set; }
    }
}
