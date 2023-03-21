using IntelliMed.Core.Services;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Identity;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenterTenant.Constants;
using OrganiMedCore.Login.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterTenant.Services
{
    public class DiabetesCareCenterService : IDiabetesCareCenterService
    {
        private readonly ICenterProfileManagerService _centerProfileManagerService;
        private readonly IDokiNetService _dokiNetService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISharedUserService _sharedUserService;
        private readonly UserManager<IUser> _userManager;


        public DiabetesCareCenterService(
            ICenterProfileManagerService centerProfileManagerService,
            IDokiNetService dokiNetService,
            ISharedDataAccessorService sharedDataAccessorService,
            ISharedUserService sharedUserService,
            UserManager<IUser> userManager)
        {
            _centerProfileManagerService = centerProfileManagerService;
            _dokiNetService = dokiNetService;
            _sharedDataAccessorService = sharedDataAccessorService;
            _sharedUserService = sharedUserService;
            _userManager = userManager;
        }


        public async Task ChangeCenterProfileLeaderAsync(DokiNetMember nextLeader, IUpdateModel updater)
        {
            DokiNetMember currentLeader = null;
            try
            {
                var contentItem = await _centerProfileManagerService.GetCenterProfileEditorForCurrentCenterAsync();
                currentLeader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(
                    contentItem.As<CenterProfilePart>().MemberRightId);

                if (currentLeader.MemberRightId == nextLeader.MemberRightId)
                {
                    return;
                }
            }
            catch (CenterProfileNotAssignedException)
            {
                // No leader yet. This is possible when connecting tenant and center profile for the first time.
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                if (currentLeader != null)
                {
                    // Remove the current leader.
                    // The shared user can be null if he/she has never logged in (or never has been referenced).
                    var sharedUserLeader = await _sharedUserService.GetSharedUserByDokiNetMemberAsync(scope, currentLeader);
                    if (sharedUserLeader != null)
                    {
                        var localUserLeader = await _userManager.FindByNameAsync((sharedUserLeader as User).Id.ToString());
                        await _userManager.RemoveFromRoleAsync(localUserLeader, DiabetesCareCenterRoleNames.DiabetesCareCenterLeader);
                    }
                }

                // Create (or update) a new user in the manager's scope, assign to tenant, create organization user profile and add roles.
                await _sharedUserService.CreateOrUpdateSharedUserUsingDokiNetMemberAsync(
                    scope,
                    nextLeader,
                    new List<string>()
                    {
                        DiabetesCareCenterRoleNames.DiabetesCareCenterLeader
                    },
                    updater);
            }
        }
    }
}
