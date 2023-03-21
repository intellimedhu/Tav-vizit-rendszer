using OrganiMedCore.Email.Models;
using YesSql.Indexes;

namespace OrganiMedCore.Email.Indexes
{
    public class EmailNotificationIndexProvider : IndexProvider<EmailNotification>
    {
        public override void Describe(DescribeContext<EmailNotification> context)
        {
            context.For<EmailNotificationIndex>()
                .Map(emailNotification => new EmailNotificationIndex()
                {
                    Bcc = string.Join(";", emailNotification.Bcc),
                    Cc = string.Join(";", emailNotification.Cc),
                    To = string.Join(";", emailNotification.To),
                    ScheduledSendDate = emailNotification.ScheduledSendDate,
                    SentOn = emailNotification.SentOn,
                    NotificationId = emailNotification.NotificationId,
                    TemplateId = emailNotification.TemplateId
                });
        }
    }
}
