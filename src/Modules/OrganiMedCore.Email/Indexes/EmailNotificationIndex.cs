using System;
using YesSql.Indexes;

namespace OrganiMedCore.Email.Indexes
{
    public class EmailNotificationIndex : MapIndex
    {
        public Guid NotificationId { get; set; }

        /// <summary>
        /// UTC
        /// </summary>
        public DateTime? ScheduledSendDate { get; set; }

        public DateTime? SentOn { get; set; }

        public string To { get; set; }

        public string Cc { get; set; }

        public string Bcc { get; set; }

        public string TemplateId { get; set; }
    }
}
