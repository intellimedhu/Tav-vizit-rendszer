using OrchardCore.ContentManagement;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface ICenterProfileReviewService
    {
        Task<CenterProfileReviewAuthorizationResult> GetAuthorizationResultAsync(User user, ContentItem contentItem);

        Task<ReviewerStatistics> GetReviewerStatisticsAsync(User user, IEnumerable<ContentItem> contentItems);
    }
}
