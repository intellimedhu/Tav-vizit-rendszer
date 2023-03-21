using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.Login.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterTenant.Extensions
{
    public static class CenterProfileAuthorizationExtensions
    {
        public static async Task<bool> AuthorizedToViewCenterProfile(this ISharedUserService sharedUserService, ContentItem contentItem)
        {
            var dokiNetMember = await sharedUserService.GetCurrentUsersDokiNetMemberAsync();

            return contentItem.As<CenterProfilePart>().MemberRightId == dokiNetMember.MemberRightId;
        }
    }
}
