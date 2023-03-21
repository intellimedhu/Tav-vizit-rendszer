using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using IntelliMed.DokiNetIntegration.Models;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Entities;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenterManager.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class CenterUserService : ICenterUserService
    {
        private readonly ISession _session;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public CenterUserService(
            ISession session,
            ISharedDataAccessorService sharedDataAccessorService)
        {
            _session = session;
            _sharedDataAccessorService = sharedDataAccessorService;
        }


        public async Task<IEnumerable<LocalUserWithDokiNetMemberViewModel>> GetUsersByLocalUsersAsync(IEnumerable<User> localUsers)
        {
            localUsers.ThrowIfNull();

            var sharedUsers = Enumerable.Empty<User>();
            if (localUsers.Any())
            {
                var sharedUserIds = localUsers.Select(user => int.Parse(user.UserName)).ToArray();
                using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
                {
                    var session = scope.ServiceProvider.GetRequiredService<ISession>();
                    sharedUsers = await session.GetAsync<User>(sharedUserIds);
                }
            }

            return localUsers.Select(localUser => new LocalUserWithDokiNetMemberViewModel()
            {
                LocalUser = localUser,
                DokiNetMember = sharedUsers.FirstOrDefault(x => x.Id.ToString() == localUser.UserName)?.As<DokiNetMember>(),
            });
        }

        public async Task<IEnumerable<LocalUserWithDokiNetMemberViewModel>> GetUsersWithCenterRolesAsync()
        {
            var localUsers = (await _session
                .Query<User, UserByRoleNameIndex>(index => index.RoleName.IsIn(new[]
                {
                    CenterPosts.MDTManagement,
                    CenterPosts.MDTSecretary,
                    CenterPosts.OMKB,
                    CenterPosts.TerritorialRapporteur
                }))
                .ListAsync())
                .Distinct();

            return await GetUsersByLocalUsersAsync(localUsers);
        }
    }
}
