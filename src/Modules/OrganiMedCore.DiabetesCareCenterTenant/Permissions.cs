using IntelliMed.Core.Constants;
using OrchardCore.Security.Permissions;
using OrganiMedCore.DiabetesCareCenterTenant.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterTenant
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageCenterProfile = new Permission(nameof(ManageCenterProfile), "Manage Diabetes Care Center Profile");


        public Task<IEnumerable<Permission>> GetPermissionsAsync()
            => Task.FromResult(new[]
            {
                ManageCenterProfile
            }.AsEnumerable());

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
            => new[]
            {
                new PermissionStereotype
                {
                    Name = WellKnownNames.AdminRoleName,
                    Permissions = new[]
                    {
                        ManageCenterProfile
                    }
                },
                new PermissionStereotype
                {
                    Name = DiabetesCareCenterRoleNames.DiabetesCareCenterLeader,
                    Permissions = new[]
                    {
                        ManageCenterProfile
                    }
                }
            };
    }
}