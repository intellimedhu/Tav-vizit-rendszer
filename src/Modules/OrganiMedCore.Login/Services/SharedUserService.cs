using IntelliMed.Core.Constants;
using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using IntelliMed.DokiNetIntegration.Exceptions;
using IntelliMed.DokiNetIntegration.Extensions;
using IntelliMed.DokiNetIntegration.Indexes;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.Core.Indexes;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Models.Enums;
using OrganiMedCore.Core.Services;
using OrganiMedCore.Core.Settings;
using OrganiMedCore.Login.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.Login.Services
{
    public class SharedUserService : ISharedUserService
    {
        private readonly IBetterUserService _betterUserService;
        private readonly IDokiNetService _dokiNetService;
        private readonly IEVisitOrganizationUserProfileService _eVisitOrganizationUserProfileService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISharedLoginService _sharedLoginService;
        private readonly ISiteService _siteService;
        private readonly ShellSettings _shellSettings;
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;


        private IStringLocalizer T { get; set; }


        public SharedUserService(
            IBetterUserService betterUserService,
            IDokiNetService dokiNetService,
            IEVisitOrganizationUserProfileService eVisitOrganizationUserProfileService,
            IPasswordGeneratorService passwordGeneratorService,
            IServiceProvider serviceProvider,
            ISession session,
            ISharedDataAccessorService sharedDataAccessorService,
            ISharedLoginService sharedLoginService,
            ISiteService siteService,
            IStringLocalizer<SharedUserService> stringLocalizer,
            ShellSettings shellSettings,
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager)
        {
            _betterUserService = betterUserService;
            _dokiNetService = dokiNetService;
            _eVisitOrganizationUserProfileService = eVisitOrganizationUserProfileService;
            _passwordGeneratorService = passwordGeneratorService;
            _serviceProvider = serviceProvider;
            _session = session;
            _sharedDataAccessorService = sharedDataAccessorService;
            _sharedLoginService = sharedLoginService;
            _siteService = siteService;
            _shellSettings = shellSettings;
            _signInManager = signInManager;
            _userManager = userManager;

            T = stringLocalizer;
        }


        public async Task<IUser> CreateOrUpdateSharedUserUsingDokiNetMemberAsync(
            IServiceScope managersServiceScope,
            DokiNetMember dokiNetMember,
            IList<string> tenantRoles,
            IUpdateModel updater)
        {
            managersServiceScope.ThrowIfNull();
            dokiNetMember.ThrowIfNull();
            updater.ThrowIfNull();

            if (!dokiNetMember.Emails.Any())
            {
                throw new DokiNetMemberRegistrationException(
                    T["Az Ön tagi adatai között nem található email cím."].Value);
            }

            var stampNumberExists = !string.IsNullOrEmpty(dokiNetMember.StampNumber) && !string.IsNullOrWhiteSpace(dokiNetMember.StampNumber);
            if (stampNumberExists && !Regex.IsMatch(dokiNetMember.StampNumber, RegexPatterns.DoctorStampNumber))
            {
                throw new DokiNetMemberRegistrationException(
                    T["Az Ön pecsétszáma érvénytelen a tagi adatbázisban. Kérjük javítsa, majd próbálja meg ismét."].Value);
            }

            var tenantSettings = (await _siteService.GetSiteSettingsAsync()).As<TenantSettings>();
            var tenantIsOrgnization = tenantSettings.IsOrganization;

            var managersSession = managersServiceScope.ServiceProvider.GetRequiredService<ISession>();

            try
            {
                ContentItem existingOrganizationUserProfileByStampNumber = null;
                if (stampNumberExists)
                {
                    existingOrganizationUserProfileByStampNumber = await _eVisitOrganizationUserProfileService.GetByIdentifierAsync(
                        managersServiceScope,
                        dokiNetMember.StampNumber,
                        OrganizationUserIdentifierTypes.StampNumber);
                }

                IUser localUser = null;
                var sharedIUser = await _sharedLoginService.FindSharedUserByUserNameAsync(managersServiceScope, dokiNetMember.Emails.First());
                if (sharedIUser == null)
                {
                    // This may happen when the user in the doki.Net changes his/her primary email address.
                    sharedIUser = await GetSharedUserByDokiNetMemberAsync(managersServiceScope, dokiNetMember);
                }
                else if ((sharedIUser as User).As<DokiNetMember>()?.MemberId != dokiNetMember.MemberId)
                {
                    // Email is not unique in the doki.Net, so if the member IDs are not equal the user must not allow to login in.
                    throw new DokiNetMemberRegistrationException(
                        T["A felhasználóhoz a tagi adatbázisban rögzített email címe már szerepel egy másik személynél is. Kérjük, változtassa meg elsődleges email címét!"].Value);
                }

                if (sharedIUser == null)
                {
                    if (stampNumberExists && existingOrganizationUserProfileByStampNumber != null)
                    {
                        throw new DokiNetMemberRegistrationException(T["Ilyen pecsétszámmal már létezik egy másik felhasználó a rendszerben."].Value);
                    }

                    // Creating eVisit profile
                    var profile = await _eVisitOrganizationUserProfileService.InitializeAsync(managersServiceScope);
                    await _sharedDataAccessorService.UpdateManagerEditorAsync(managersServiceScope, profile, updater, true);
                    var profilePart = profile.As<EVisitOrganizationUserProfilePart>();

                    // TODO: find roles here or provide them somehow.
                    localUser = await _sharedLoginService.SharedRegisterAsync(
                        managersServiceScope,
                        dokiNetMember.Emails.First(),
                        true,
                        _passwordGeneratorService.GenerateRandomPassword(16),
                        tenantRoles,
                        sharedUser => UpdateDokiNetMemberProperty(sharedUser, dokiNetMember),
                        (key, message) => throw new DokiNetMemberRegistrationException(message));

                    if (localUser == null)
                    {
                        throw new DokiNetMemberRegistrationException(T["Hiba történt a bejelentkezés során. #1"].Value);
                    }

                    if (!int.TryParse(localUser.UserName, out int sharedUserId))
                    {
                        throw new DokiNetMemberRegistrationException(T["Hiba történt a bejelentkezés során. #2"].Value);
                    }

                    profile.Alter<EVisitOrganizationUserProfilePart>(x =>
                    {
                        x.SharedUserId = sharedUserId;
                        x.Name = dokiNetMember.FullName;
                        x.Email = dokiNetMember.Emails.First();
                        x.StampNumber = stampNumberExists ? dokiNetMember.StampNumber : null;
                        x.OrganizationUserProfileType = stampNumberExists
                            ? OrganizationUserProfileTypes.Doctor
                            : OrganizationUserProfileTypes.Assistant;
                    });

                    // Saving EVisit profile.
                    await managersServiceScope.ServiceProvider.GetRequiredService<IContentManager>()
                        .CreateAsync(profile);

                    // Assigning EVisit user to the organization.
                    await _eVisitOrganizationUserProfileService.AssignToTenantAsync(
                        managersServiceScope,
                        sharedUserId,
                        _shellSettings.Name,
                        // TODO: add roles.
                        new List<string>(),
                        tenantIsOrgnization,
                        (key, message) => throw new DokiNetMemberRegistrationException(message));
                }
                else
                {
                    if (stampNumberExists &&
                        existingOrganizationUserProfileByStampNumber != null &&
                        existingOrganizationUserProfileByStampNumber.As<EVisitOrganizationUserProfilePart>().SharedUserId != (sharedIUser as User).Id)
                    {
                        throw new DokiNetMemberRegistrationException(T["Ilyen pecsétszámmal már létezik egy másik felhasználó a rendszerben."].Value);
                    }

                    //if (!_sharedLoginService.AuthorizedToSignInOnTenant(sharedUser))
                    //{
                    //    // TODO
                    //}

                    var sharedUser = sharedIUser as User;

                    // eg.: Changed his/her email in doki.net
                    await SetLoginPropertiesAsync(
                        managersServiceScope.ServiceProvider.GetRequiredService<UserManager<IUser>>(),
                        sharedUser,
                        dokiNetMember.Emails.First(),
                        true);

                    localUser = await _userManager.FindByNameAsync(sharedUser.Id.ToString());
                    if (localUser == null)
                    {
                        await _eVisitOrganizationUserProfileService.AssignToTenantAsync(
                            managersServiceScope,
                            sharedUser.Id,
                            _shellSettings.Name,
                            tenantRoles,
                            tenantIsOrgnization,
                            (key, message) => throw new DokiNetMemberRegistrationException(message));

                        localUser = await _userManager.FindByNameAsync(sharedUser.Id.ToString());
                        if (localUser == null)
                        {
                            throw new DokiNetMemberRegistrationException(T["Hiba történt a bejelentkezés során."].Value);
                        }
                    }
                    else
                    {
                        // eg.: Changed his/her email in doki.net
                        await SetLoginPropertiesAsync(_userManager, localUser as User, dokiNetMember.Emails.First());

                        foreach (var role in tenantRoles)
                        {
                            if (!await _userManager.IsInRoleAsync(localUser, role))
                            {
                                await _userManager.AddToRoleAsync(localUser, role);
                            }
                        }
                    }

                    // Updating stored doki.net member data in user.
                    UpdateDokiNetMemberProperty(sharedUser, dokiNetMember);

                    // Updating profile data.
                    // TODO: place this into the serivce.
                    var contentItemProfile = await managersSession
                        .Query<ContentItem, EVisitOrganizationUserProfilePartIndex>(x => x.SharedUserId == sharedUser.Id)
                        .FirstOrDefaultAsync();

                    if (contentItemProfile != null)
                    {
                        // Updating basic info coming from doki.net when signing in.
                        contentItemProfile?.Alter<EVisitOrganizationUserProfilePart>(x =>
                        {
                            x.StampNumber = stampNumberExists ? dokiNetMember.StampNumber : null;
                            x.Name = dokiNetMember.FullName;
                            x.Email = dokiNetMember.Emails.First();
                            x.OrganizationUserProfileType = stampNumberExists
                                ? OrganizationUserProfileTypes.Doctor
                                : OrganizationUserProfileTypes.Assistant;
                        });

                        await managersServiceScope.ServiceProvider.GetRequiredService<IContentManager>().UpdateAsync(contentItemProfile);
                    }

                    managersSession.Save(sharedIUser);
                    //_session.Save(localUser);
                }

                return localUser;
            }
            catch (DokiNetMemberRegistrationException)
            {
                managersSession.Cancel();

                throw;
            }
        }

        public async Task<IdentityResult> DeleteLocalUserIfHasNoRolesAsync(IServiceScope scope, User localUser)
        {
            if (localUser.RoleNames.Any())
            {
                return IdentityResult.Success;
            }

            // TODO: meg kell nézni, hogy a local user benne van-e bármilyen lokális intézményi egységben vagy sem?

            var result = await _userManager.DeleteAsync(localUser);
            if (result != IdentityResult.Success)
            {
                return result;
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var session = scope.ServiceProvider.GetRequiredService<ISession>();

            var sharedUser = await userManager.FindByIdAsync(localUser.UserName) as User;

            // Remove from tenant.
            sharedUser.Alter<EVisitUser>(nameof(EVisitUser), x =>
            {
                x.PermittedTenans.Remove(_shellSettings.Name);
            });

            if (sharedUser.As<EVisitUser>().PermittedTenans.Any())
            {
                session.Save(sharedUser);

                return IdentityResult.Success;
            }

            var profiles = await session
                .Query<ContentItem, EVisitOrganizationUserProfilePartIndex>(x => x.SharedUserId == sharedUser.Id)
                .ListAsync();
            foreach (var profile in profiles)
            {
                session.Delete(profile);
            }

            result = await userManager.DeleteAsync(sharedUser);
            if (result != IdentityResult.Success)
            {
                _session.Cancel();
                session.Cancel();
            }

            return result;
        }

        public async Task<IUser> GetSharedUserByDokiNetMemberAsync(IServiceScope scope, DokiNetMember dokiNetMember)
        {
            IUser user = null;
            if (dokiNetMember.Emails.Any())
            {
                user = await _sharedLoginService.FindSharedUserByUserNameAsync(scope, dokiNetMember.Emails.First());
                if (user != null)
                {
                    return user;
                }
            }

            if (!string.IsNullOrEmpty(dokiNetMember.StampNumber) &&
                !string.IsNullOrWhiteSpace(dokiNetMember.StampNumber) &&
                Regex.IsMatch(dokiNetMember.StampNumber, RegexPatterns.DoctorStampNumber))
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                user = await session
                    .Query<User, DokiNetMemberIndex>(x =>
                        x.MemberRightId == dokiNetMember.MemberRightId ||
                        x.UserName == dokiNetMember.UserName ||
                        x.StampNumber == dokiNetMember.StampNumber)
                    .FirstOrDefaultAsync();
            }

            return user;
        }

        public async Task<DokiNetMember> GetCurrentUsersDokiNetMemberAsync()
            => await GetUsersDokiNetMemberAsync((await _betterUserService.GetCurrentUserAsync()).UserName);

        public Task<DokiNetMember> GetUsersDokiNetMemberAsync(User localUser)
            => GetUsersDokiNetMemberAsync(localUser.UserName);

        public async Task<DokiNetMember> UpdateAndGetCurrentUsersDokiNetMemberDataAsync()
        {
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var sharedUserId = (await _betterUserService.GetCurrentUserAsync()).UserName;
                var sharedIUser = await scope.ServiceProvider.GetRequiredService<UserManager<IUser>>().FindByIdAsync(sharedUserId);

                var sharedUser = sharedIUser as User;
                var dokiNetMember = sharedUser?.As<DokiNetMember>();
                if (dokiNetMember == null || dokiNetMember.MemberRightId == 0)
                {
                    throw new UserHasNoMemberRightIdException();
                }

                var webId = dokiNetMember.WebId;

                var upToDateDokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(dokiNetMember.MemberRightId);
                sharedUser.Alter<DokiNetMember>(nameof(DokiNetMember), member => member.UpdateDokiNetMemberData(upToDateDokiNetMember));
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                await userManager.UpdateAsync(sharedUser);

                if (string.IsNullOrEmpty(upToDateDokiNetMember.WebId) && !string.IsNullOrEmpty(webId))
                {
                    upToDateDokiNetMember.WebId = webId;
                }

                return upToDateDokiNetMember;
            }
        }

        public async Task<IEnumerable<DokiNetMember>> GetDokiNetMembersFromManagersScopeByLocalUserIdsAsync(IEnumerable<string> sharedUserIds)
        {
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var result = new List<DokiNetMember>();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                foreach (var sharedUserId in sharedUserIds)
                {
                    var iUser = await userManager.FindByIdAsync(sharedUserId);
                    if (iUser == null)
                    {
                        continue;
                    }

                    result.Add((iUser as User).As<DokiNetMember>());
                }

                return result;
            }
        }

        public async Task<bool> SignInDokiNetMemberAsync(
            DokiNetMember dokiNetMember,
            Action<string> reportError,
            IUpdateModel updater,
            bool rememberMe)
        {
            var dokiNetMemberValidator = _serviceProvider.GetService<IDokiNetMemberValidator>();
            if (dokiNetMemberValidator != null)
            {
                if (!await dokiNetMemberValidator.ValidateLoginToTenantAsync(dokiNetMember, reportError))
                {
                    return false;
                }
            }

            IUser localUser;
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                try
                {
                    localUser = await CreateOrUpdateSharedUserUsingDokiNetMemberAsync(
                        scope,
                        dokiNetMember,
                        new List<string>(),
                        updater);
                    if (localUser == null)
                    {
                        updater.ModelState.AddModelError(string.Empty, T["Sikertelen bejelentkezés."].Value);

                        return false;
                    }
                }
                catch (DokiNetMemberRegistrationException ex)
                {
                    _session.Cancel();

                    updater.ModelState.AddModelError(string.Empty, ex.Message);

                    return false;
                }
            }

            var dokiNetUserLoginHandler = _serviceProvider.GetService<IDokiNetUserLoginHandler>();
            if (dokiNetUserLoginHandler != null)
            {
                await dokiNetUserLoginHandler.HandleUserBeforeLogin(localUser, dokiNetMember);
            }

            await _signInManager.SignInAsync(localUser, isPersistent: rememberMe);

            if (dokiNetUserLoginHandler != null)
            {
                await dokiNetUserLoginHandler?.HandleUserAfterLogin(localUser, dokiNetMember);
            }

            return true;
        }


        private void UpdateDokiNetMemberProperty(User sharedUser, DokiNetMember dokiNetMember)
            => sharedUser.Alter<DokiNetMember>(
                nameof(DokiNetMember),
                member => member.UpdateDokiNetMemberData(dokiNetMember));

        private async Task SetLoginPropertiesAsync(
            UserManager<IUser> userManager,
            User user,
            string newEmail,
            bool setUserName = false)
        {
            if (user.Email != newEmail)
            {
                await userManager.SetEmailAsync(user, newEmail);
            }

            if (setUserName && user.UserName != newEmail)
            {
                await userManager.SetUserNameAsync(user, newEmail);
            }
        }

        private async Task<DokiNetMember> GetUsersDokiNetMemberAsync(string sharedUserId)
        {
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var sharedIUser = await scope.ServiceProvider.GetRequiredService<UserManager<IUser>>().FindByIdAsync(sharedUserId);

                var dokiNetMember = (sharedIUser as User)?.As<DokiNetMember>();
                if (dokiNetMember == null || dokiNetMember.MemberRightId == 0)
                {
                    throw new UserHasNoMemberRightIdException();
                }

                return dokiNetMember;
            }
        }
    }
}
