using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface ICenterProfileTenantService
    {
        Task<IEnumerable<ContentItem>> GetCenterProfilesForTenantAsync(string tenantName);

        Task<ContentItem> GetCenterProfileAssignedToTenantAsync(string tenantName);

        Task SetCenterProfileAssignmentAsync(string contentItemId, string tenantName);

        Task<ContentItem> UpdateCenterProfileAsync(string tenantName, ICenterProfileViewModel viewModel);

        Task SaveCenterProfileAsync(ContentItem contentItem, bool submitted);

        Task CalculateAccreditationStatusAsync(ContentItem contentItem);

        Task<Colleague> ExecuteColleagueActionAsync(ContentItem contentItem, CenterProfileColleagueActionViewModel viewModel);

        Task<Colleague> InviteColleagueAsync(ContentItem contentItem, CenterProfileColleagueViewModel viewModel);

        Task<ContentItem> RequireCenterProfileContentItemInNewRenewalProcessAsync(ContentItem contentItem, bool shouldEmptyCache);
    }
}
