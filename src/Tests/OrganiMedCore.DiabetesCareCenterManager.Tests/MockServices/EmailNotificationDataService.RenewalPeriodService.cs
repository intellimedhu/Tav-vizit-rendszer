using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests.MockServices
{
    public class EmailNotificationDataService_RenewalPeriodService : IEmailNotificationDataService
    {
        private readonly ISession _session;


        public EmailNotificationDataService_RenewalPeriodService(ISession session)
        {
            _session = session;
        }


        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<EmailNotification>> DequeueAsync(int limit)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task MarkAsSentAsync(Guid emailId)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> QueueAsync(EmailNotification email)
        {
            // Assuming the email is not null:
            email.NotificationId = Guid.NewGuid();
            _session.Save(email);

            return Task.FromResult(email.NotificationId);
        }
    }
}
