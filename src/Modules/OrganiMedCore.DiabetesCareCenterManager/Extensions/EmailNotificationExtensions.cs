using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Identity;
using OrchardCore.ContentManagement;
using OrchardCore.Users;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Login.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Extensions
{
    public static class EmailNotificationExtensions
    {
        public static async Task NotifyAcceptedCenterProfileAsync(
            ContentItem contentItem,
            string currentRole,
            string tenant,
            IDokiNetService dokiNetService,
            ISharedUserService sharedUserService,
            ITerritoryService territoryService,
            UserManager<IUser> userManager,
            Func<EmailNotification, Task> queueAsync)
        {
            var partMain = contentItem.As<CenterProfilePart>();
            var partManager = contentItem.As<CenterProfileManagerExtensionsPart>();

            var notifiedUsers = new List<IUser>();

            if (currentRole == CenterPosts.TerritorialRapporteur ||
                currentRole == CenterPosts.MDTSecretary ||
                currentRole == CenterPosts.MDTManagement)

            {
                notifiedUsers.AddRange(await userManager.GetUsersInRoleAsync(CenterPosts.OMKB));
            }

            if (currentRole == CenterPosts.OMKB || currentRole == CenterPosts.MDTManagement)
            {
                var reviewers = await territoryService.GetReviewersAsync(partMain.CenterZipCode, partMain.CenterSettlementName);
                if (reviewers.Any())
                {
                    notifiedUsers.AddRange(reviewers);
                }
            }

            if (currentRole == CenterPosts.OMKB)
            {
                notifiedUsers.AddRange(await userManager.GetUsersInRoleAsync(CenterPosts.MDTManagement));
            }

            var leaderMember = await dokiNetService.GetDokiNetMemberById<DokiNetMember>(partMain.MemberRightId);
            var notifiedDokiNetMembers = new List<DokiNetMember>() { leaderMember };
            notifiedDokiNetMembers.AddRange(await sharedUserService.GetDokiNetMembersFromManagersScopeByLocalUserIdsAsync(GetDistinctUserNames(notifiedUsers)));

            notifiedDokiNetMembers = GetDistinctDokiNetMembers(notifiedDokiNetMembers);

            foreach (var dokiNetMember in notifiedDokiNetMembers)
            {
                //var emailToLeader = dokiNetMember.MemberRightId == leaderMember.MemberRightId;
                await queueAsync(new EmailNotification()
                {
                    TemplateId = EmailTemplateIds.CenterProfileAccepted,
                    Data = new CenterProfileAcceptedViewModel()
                    {
                        PersonName = dokiNetMember.FullName,
                        CenterName = partMain.CenterName,
                        CurrentRole = currentRole,
                        AccreditationStatus = partManager.RenewalAccreditationStatus ?? partMain.AccreditationStatus
                    },
                    To = new HashSet<string>(new[] { dokiNetMember.Emails.FirstOrDefault() })
                });
            }
        }

        public static List<DokiNetMember> GetDistinctDokiNetMembers(List<DokiNetMember> notifiedDokiNetMembers)
            => notifiedDokiNetMembers
                .Where(x => x.Emails.Any())
                .GroupBy(x => x.MemberRightId)
                .Select(x => x.First())
                .ToList();

        public static IEnumerable<string> GetDistinctUserNames(List<IUser> notifiedUsers)
            => notifiedUsers
                .GroupBy(x => x.UserName)
                .Select(x => x.Key);
    }
}
