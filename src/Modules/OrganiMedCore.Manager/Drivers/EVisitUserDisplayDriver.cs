using IntelliMed.Core.Constants;
using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Manager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Manager.Drivers
{
    public class EVisitUserDisplayDriver : DisplayDriver<User>
    {
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public EVisitUserDisplayDriver(ISharedDataAccessorService sharedDataAccessorService)
        {
            _sharedDataAccessorService = sharedDataAccessorService;
        }


        public override IDisplayResult Display(User user)
        {
            return Initialize<SummaryAdminUserViewModel>("EVisitUserButtons", model => model.User = user)
                .Location("SummaryAdmin", "Actions:2");
        }

        public override async Task<IDisplayResult> EditAsync(User user, BuildEditorContext context)
        {
            var tenantNames = await GetShellContextNamesExpectManagersAsync();

            return Initialize<EditEVisitUserViewModel>("EVisitUser_Edit", model =>
            {
                var eVisitUser = user.As<EVisitUser>();

                var tenants = tenantNames.Select(x => new PermittedTenantViewModel
                {
                    Tenant = x,
                    IsSelected = eVisitUser.PermittedTenans.Contains(x, StringComparer.OrdinalIgnoreCase)
                })
                .ToArray();

                model.Id = user.Id;
                model.IsEVisitUser = eVisitUser.IsEVisitUser;
                model.EVisitLoginEnabled = eVisitUser.EVisitLoginEnabled;
                model.Tenants = tenants;
            }).Location("Content:1").OnGroup(Constants.GroupIds.EVisitUserEditor);
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            var previouslySelectedTenantNames = user.As<EVisitUser>().PermittedTenans;
            var model = new EditEVisitUserViewModel();

            // https://github.com/OrchardCMS/OrchardCore/issues/1834 because of this a workaround is needed.
            // Basically we proceed no matter what, this must be solved.
            //if (!await context.Updater.TryUpdateModelAsync(model, Prefix))
            //{
            //    return Edit(user);
            //}
            await context.Updater.TryUpdateModelAsync(model, Prefix);
            // end of the workaround.

            var tenantNames = await GetShellContextNamesExpectManagersAsync();

            var selectedTenantNames = new HashSet<string>(model
                .Tenants
                .Where(x => x.IsSelected)
                .Select(x => x.Tenant)
                .Where(x => tenantNames.Contains(x)));

            var addedTenantNames = selectedTenantNames.Where(x => !previouslySelectedTenantNames.Contains(x));
            var removedTenantNames = previouslySelectedTenantNames.Where(x => !selectedTenantNames.Contains(x));

            foreach (var tenant in addedTenantNames)
            {
                try
                {
                    using (var scope = await _sharedDataAccessorService.GetTenantServiceScopeAsync(tenant))
                    {
                        var tenantsUserService = scope.ServiceProvider.GetRequiredService<IUserService>();
                        var tenantsPasswordGeneratorService = scope.ServiceProvider.GetRequiredService<IPasswordGeneratorService>();

                        // Shared user ID is stored in the tenant's username.
                        var tenantUser = await tenantsUserService.CreateUserAsync(
                            new User()
                            {
                                UserName = user.Id.ToString(),
                                Email = user.Email,
                                RoleNames = new List<string>()
                            },
                            tenantsPasswordGeneratorService.GenerateRandomPassword(16),
                            (key, message) => context.Updater.ModelState.AddModelError(key, message));
                    }
                }
                catch (Exception ex) when (ex is InvalidOperationException)
                {
                    if (ex.Message.StartsWith("No service for type"))
                    {
                        context.Updater.ModelState.AddModelError("User.PermittedTenans", $"A(z) {tenant} azonosítójú intézmény nem elérhető, ezért nem módosítható rajta az OrganiMed belépés.");
                        continue;
                    }

                    throw;
                }
            }

            foreach (var tenant in removedTenantNames)
            {
                try
                {
                    using (var scope = await _sharedDataAccessorService.GetTenantServiceScopeAsync(tenant))
                    {
                        var tenantsUserManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

                        // Shared user ID is stored in the tenant's username.
                        var tenantUser = await tenantsUserManager.FindByNameAsync(user.Id.ToString());
                        await tenantsUserManager.DeleteAsync(tenantUser);
                    }
                }
                catch (Exception ex) when (ex is InvalidOperationException)
                {
                    if (ex.Message.StartsWith("No service for type"))
                    {
                        context.Updater.ModelState.AddModelError("User.PermittedTenans", $"A(z) {tenant} azonosítójú intézmény nem elérhető, ezért nem módosítható rajta az OrganiMed belépés.");
                        continue;
                    }

                    throw;
                }
            }

            user.Alter<EVisitUser>(nameof(EVisitUser), x =>
            {
                x.EVisitLoginEnabled = model.EVisitLoginEnabled;
                x.IsEVisitUser = model.IsEVisitUser;
                x.PermittedTenans = selectedTenantNames;
            });

            return Edit(user);
        }


        private async Task<IEnumerable<string>> GetShellContextNamesExpectManagersAsync()
            => (await _sharedDataAccessorService.ListShellContextsAsync())
                .Where(context => context.Settings.Name != WellKnownNames.ManagerTenantName)
                .Select(context => context.Settings.Name);
    }
}
