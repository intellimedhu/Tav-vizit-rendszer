using IntelliMed.Core.Extensions;
using OrchardCore.Modules;
using OrganiMedCore.Email.Indexes;
using OrganiMedCore.Email.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.Email.Services
{
    public class EmailNotificationDataService : IEmailNotificationDataService
    {
        private readonly IClock _clock;
        private readonly ISession _session;


        public EmailNotificationDataService(
            IClock clock,
            ISession session)
        {
            _clock = clock;
            _session = session;
        }


        public async Task<IEnumerable<EmailNotification>> DequeueAsync(int limit)
        {
            var scheduledEmails = await _session
                .Query<EmailNotification, EmailNotificationIndex>(index =>
                    index.SentOn == null &&
                    index.ScheduledSendDate != null &&
                    index.ScheduledSendDate < _clock.UtcNow)
                .OrderBy(index => index.ScheduledSendDate)
                .Take(limit)
                .ListAsync();

            limit -= scheduledEmails.Count();
            if (limit <= 0)
            {
                return scheduledEmails;
            }

            var otherEmails = await _session
                .Query<EmailNotification, EmailNotificationIndex>(index =>
                    index.SentOn == null &&
                    index.ScheduledSendDate == null)
                .OrderBy(index => index.Id)
                .Take(limit)
                .ListAsync();

            return scheduledEmails.Concat(otherEmails);
        }

        public Task<Guid> QueueAsync(EmailNotification email)
        {
            email.ThrowIfNull();

            email.NotificationId = Guid.NewGuid();
            _session.Save(email);

            return Task.FromResult(email.NotificationId);
        }

        public async Task MarkAsSentAsync(Guid notificationId)
        {
            var email = await _session
                .Query<EmailNotification, EmailNotificationIndex>(index => index.NotificationId == notificationId)
                .FirstOrDefaultAsync();

            email.SentOn = _clock.UtcNow;

            _session.Save(email);
        }
    }
}
