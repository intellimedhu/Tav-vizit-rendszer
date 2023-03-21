using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Identity;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using OrganiMedCore.Core.Models.Enums;
using OrganiMedCore.Core.Services;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.Services;
using OrganiMedCore.Organization.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Drivers
{
    public class OrganizationUserProfilePartDisplayDriver : ContentPartDisplayDriver<OrganizationUserProfilePart>
    {
        private readonly IOrganizationService _organizationService;
        private readonly UserManager<IUser> _userManager;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly IOrganizationUserProfileService _organizationUserProfileService;
        private readonly IEnumerable<IRoleNameProvider> _roleNameProviders;


        public OrganizationUserProfilePartDisplayDriver(
            IOrganizationService organizationService,
            UserManager<IUser> userManager,
            ISharedDataAccessorService sharedDataAccessorService,
            IOrganizationUserProfileService organizationUserProfileService,
            IEnumerable<IRoleNameProvider> roleNameProviders)
        {
            _organizationService = organizationService;
            _userManager = userManager;
            _sharedDataAccessorService = sharedDataAccessorService;
            _organizationUserProfileService = organizationUserProfileService;
            _roleNameProviders = roleNameProviders;
        }


        public override async Task<IDisplayResult> EditAsync(OrganizationUserProfilePart part, BuildPartEditorContext context)
        {
            var user = await GetLocalUserAsync(part.ContentItem);
            return Initialize<OrganizationUserProfilePartViewModel>("OrganizationUserProfilePart_Edit", model =>
            {
                model.OrganizationUserProfileType = part.OrganizationUserProfileType;
                model.Phone = part.Phone;
                model.Email = part.Email;
                model.OrganizationRank = part.OrganizationRank;
                
                // Set roles.
                if (user != null)
                {
                    var roleNames = _roleNameProviders.SelectMany(service => service.GetRoleNames());
                    var userRoleNames = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
                    model.Roles = roleNames
                        .Select(x => new RoleViewModel
                        {
                            Role = x,
                            IsSelected = userRoleNames.Contains(x, StringComparer.OrdinalIgnoreCase)
                        })
                        .ToArray();
                }

                // Conditionally set profile type related part properties.
                if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor)
                {
                    var allOrganizationUnits = _organizationService.ListOrganizationUnitsAsync().GetAwaiter().GetResult();
                    model.OrganizationUnits = allOrganizationUnits
                    .Select(x => new PermittedOrganizationUnitViewModel
                    {
                        OrganizationUnitName = x.Name,
                        OrganizationUnitId = x.ContentItem.ContentItemId,
                        IsSelected = part.PermittedOrganizationUnits.Contains(x.ContentItem.ContentItemId)
                    }).ToArray();
                }

                if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor ||
                    part.OrganizationUserProfileType == OrganizationUserProfileTypes.SpecialAssistant)
                {
                    model.AntszLicenseNumber = part.AntszLicenseNumber;
                }

                if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor ||
                    part.OrganizationUserProfileType == OrganizationUserProfileTypes.SpecialAssistant ||
                    part.OrganizationUserProfileType == OrganizationUserProfileTypes.Assistant ||
                    part.OrganizationUserProfileType == OrganizationUserProfileTypes.Receptionist)
                {
                    model.ConsultationHours = part.ConsultationHours;
                    model.CheckInMode = part.CheckInMode;
                }
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(OrganizationUserProfilePart part, UpdatePartEditorContext context)
        {
            var previousOrganizationUserProfileType = part.OrganizationUserProfileType;

            var model = new OrganizationUserProfilePartViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            if (!previousOrganizationUserProfileType.HasValue)
            {
                part.OrganizationUserProfileType = model.OrganizationUserProfileType;
            }

            part.Phone = model.Phone;
            part.Email = model.Email;
            part.OrganizationRank = model.OrganizationRank;
            // Updating roles.
            if (context.Updater.ModelState.IsValid)
            {
                var user = await GetLocalUserAsync(part.ContentItem);
                if (user != null)
                {
                    var allowedRoleNames = _roleNameProviders.SelectMany(service => service.GetRoleNames());
                    var roleNames = model.Roles.Where(x => x.IsSelected && allowedRoleNames.Contains(x.Role)).Select(x => x.Role);
                    // Remove roles in two steps to prevent an iteration on a modified collection
                    var rolesToRemove = new List<string>();
                    var roles = await _userManager.GetRolesAsync(user);
                    foreach (var role in roles)
                    {
                        if (!roleNames.Contains(role))
                        {
                            rolesToRemove.Add(role);
                        }
                    }

                    foreach (var role in rolesToRemove)
                    {
                        await _userManager.RemoveFromRoleAsync(user, role);
                    }

                    // Add new roles
                    foreach (var role in roleNames)
                    {
                        if (!await _userManager.IsInRoleAsync(user, role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }

                    var result = await _userManager.UpdateAsync(user);
                }
            }

            // Conditionally update profile type related part properties.
            if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor)
            {
                var allOrganizationUnits = await _organizationService.ListOrganizationUnitsAsync();
                var organizationUnitIds = allOrganizationUnits.Select(x => x.ContentItem.ContentItemId);

                var selectedOrganizationUnits = model.OrganizationUnits
                    .Where(x => x.IsSelected)
                    .Select(x => x.OrganizationUnitId);
                part.PermittedOrganizationUnits = new HashSet<string>(organizationUnitIds.Where(x => selectedOrganizationUnits.Contains(x)));
            }

            if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor ||
                part.OrganizationUserProfileType == OrganizationUserProfileTypes.SpecialAssistant)
            {
                part.AntszLicenseNumber = model.AntszLicenseNumber;
            }

            if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor ||
                part.OrganizationUserProfileType == OrganizationUserProfileTypes.SpecialAssistant ||
                part.OrganizationUserProfileType == OrganizationUserProfileTypes.Assistant ||
                part.OrganizationUserProfileType == OrganizationUserProfileTypes.Receptionist)
            {
                part.ConsultationHours = model.ConsultationHours;
                part.CheckInMode = model.CheckInMode;
            }

            return await EditAsync(part, context);
        }


        private async Task<User> GetLocalUserAsync(ContentItem contentItem)
        {
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                return await _organizationUserProfileService.GetLocalUserAsync(scope, contentItem);
            };
        }
    }
}
