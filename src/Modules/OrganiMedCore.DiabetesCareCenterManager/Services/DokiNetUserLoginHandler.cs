using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Identity;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class DokiNetUserLoginHandler : IDokiNetUserLoginHandler
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly UserManager<IUser> _userManager;


        public DokiNetUserLoginHandler(
            IContentManager contentManager,
            ISession session,
            UserManager<IUser> userManager)
        {
            _contentManager = contentManager;
            _session = session;
            _userManager = userManager;
        }


        public async Task HandleUserBeforeLogin(IUser localUser, DokiNetMember dokiNetMember)
        {
            localUser.ThrowIfNull();
            dokiNetMember.ThrowIfNull();

            if (!string.IsNullOrEmpty(dokiNetMember.StampNumber))
            {
                if (!await _userManager.IsInRoleAsync(localUser, CenterPosts.Doctor))
                {
                    await _userManager.AddToRoleAsync(localUser, CenterPosts.Doctor);
                }
            }
            else if (await _userManager.IsInRoleAsync(localUser, CenterPosts.Doctor))
            {
                await _userManager.RemoveFromRoleAsync(localUser, CenterPosts.Doctor);
            }

            (localUser as User).Alter<DokiNetMember>(nameof(DokiNetMember), x => x.WebId = dokiNetMember.WebId);

            // TODO: Ki kell rakni egy service hívásba és a nonce alapján történő bejelentkezésnél is meghívni.
            var contentItems = await _session
                .Query<ContentItem, CenterProfileColleagueIndex>(index => index.Email.IsIn(dokiNetMember.Emails))
                .LatestAndPublished()
                .ListAsync();

            foreach (var contentItem in contentItems)
            {
                contentItem.Alter<CenterProfilePart>(part =>
                {
                    var colleague = part.Colleagues.FirstOrDefault(x =>
                        (x.MemberRightId == null || x.MemberRightId == dokiNetMember.MemberRightId) &&
                        dokiNetMember.Emails.Contains(x.Email));

                    if (colleague == null)
                    {
                        return;
                    }

                    colleague.Prefix = dokiNetMember.Prefix;
                    colleague.FirstName = dokiNetMember.FirstName;
                    colleague.LastName = dokiNetMember.LastName;
                    if (!colleague.MemberRightId.HasValue)
                    {
                        colleague.MemberRightId = dokiNetMember.MemberRightId;
                    }
                });

                await _contentManager.UpdateAsync(contentItem);
            }
        }

        public Task HandleUserAfterLogin(IUser localUser, DokiNetMember dokiNetMember)
            => Task.CompletedTask;
    }
}
