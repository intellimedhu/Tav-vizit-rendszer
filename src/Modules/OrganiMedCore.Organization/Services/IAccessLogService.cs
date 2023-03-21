using IntelliMed.Core;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Users.Models;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Services
{
    /// <summary>
    /// Logs the activities of the user.
    /// </summary>
    public interface IAccessLogService
    {
        /// <summary>
        /// Logs a custom activity.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="user">The user who did the activity.</param>
        /// <param name="message">The custom message of the activity.</param>
        Task LogActivityAsync(IServiceScope managersServiceScope, User user, string message);

        /// <summary>
        /// Logs a content activity, the log message will be generated automatically.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="user">The user who did the activity.</param>
        /// <param name="crud">The operation type.</param>
        /// <param name="contentItem">The related content item.</param>
        Task LogContentActivityAsync(IServiceScope managersServiceScope, User user, Crud crud, ContentItem contentItem);
    }
}
