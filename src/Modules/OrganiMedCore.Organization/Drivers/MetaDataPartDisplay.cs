using IntelliMed.Core.Services;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Drivers
{
    public class MetaDataPartDisplay : ContentPartDisplayDriver<MetaDataPart>
    {
        private readonly IOrganizationUserProfileService _organizationUserProfileService;
        private readonly IBetterUserService _betterUserService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public IStringLocalizer T { get; set; }


        public MetaDataPartDisplay(
            IOrganizationUserProfileService organizationUserProfileService,
            IBetterUserService betterUserService,
            IStringLocalizer<MetaDataPartDisplay> stringLocalizer,
            ISharedDataAccessorService sharedDataAccessorService)
        {
            _organizationUserProfileService = organizationUserProfileService;
            _betterUserService = betterUserService;
            _sharedDataAccessorService = sharedDataAccessorService;

            T = stringLocalizer;
        }


        public override async Task<IDisplayResult> UpdateAsync(MetaDataPart model, IUpdateModel updater)
        {
            if (string.IsNullOrEmpty(model.OrganizationUnitId))
            {
                using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
                {
                    var profile = await _organizationUserProfileService.GetOrganizationUserProfileAsync(scope, await _betterUserService.GetCurrentUserAsync());
                    if (profile != null)
                    {
                        var profilePart = profile.As<OrganizationUserProfilePart>();
                        if (string.IsNullOrEmpty(profilePart.SignedInOrganizationUnitId))
                        {
                            updater.ModelState.AddModelError(nameof(model.OrganizationUnitId), T["Can't find signed in organization unit."]);
                        }
                        else
                        {
                            model.OrganizationUnitId = profilePart.SignedInOrganizationUnitId;
                        }
                    }
                }
            }

            return Edit(model);
        }
    }
}
