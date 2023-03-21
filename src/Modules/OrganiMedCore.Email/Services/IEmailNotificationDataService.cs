using OrganiMedCore.Email.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Services
{
    public interface IEmailNotificationDataService
    {
        Task<Guid> QueueAsync(EmailNotification email);

        Task<IEnumerable<EmailNotification>> DequeueAsync(int limit);

        Task MarkAsSentAsync(Guid notificationId);
    }
}
