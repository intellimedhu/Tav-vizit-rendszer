using IntelliMed.DokiNetIntegration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Services
{
    public interface ISharedUserService
    {
        /// <summary>
        /// Creates or updates the shared user stored in eVisitManager tenant based on a <see cref="DokiNetMember"/>.
        /// </summary>
        /// <exception cref="DokiNetMemberRegistrationException"></exception>
        /// <returns>The created or updated local user.</returns>
        Task<IUser> CreateOrUpdateSharedUserUsingDokiNetMemberAsync(
            IServiceScope managersServiceScope,
            DokiNetMember dokiNetMember,
            IList<string> tenantRoles,
            IUpdateModel updater);

        /// <summary>
        /// Removes the local user if it has no role and
        /// removes the shared user if it has no other tenants like the current.
        /// </summary>
        /// <returns><see cref="IdentityResult"/></returns>
        Task<IdentityResult> DeleteLocalUserIfHasNoRolesAsync(IServiceScope scope, User localUser);

        Task<IUser> GetSharedUserByDokiNetMemberAsync(IServiceScope scope, DokiNetMember dokiNetMember);

        /// <summary>
        /// Returns the stored data.
        /// </summary>
        /// <exception cref="UserHasNoMemberRightIdException"></exception>
        Task<DokiNetMember> GetCurrentUsersDokiNetMemberAsync();

        Task<DokiNetMember> GetUsersDokiNetMemberAsync(User localUser);

        /// <summary>
        /// Updates the stored date and returns that.
        /// </summary>
        /// <exception cref="UserHasNoMemberRightIdException"></exception>
        Task<DokiNetMember> UpdateAndGetCurrentUsersDokiNetMemberDataAsync();

        Task<IEnumerable<DokiNetMember>> GetDokiNetMembersFromManagersScopeByLocalUserIdsAsync(IEnumerable<string> sharedUserIds);

        Task<bool> SignInDokiNetMemberAsync(
            DokiNetMember dokiNetMember,
            Action<string> reportError,
            IUpdateModel updater,
            bool rememberMe);
    }
}
