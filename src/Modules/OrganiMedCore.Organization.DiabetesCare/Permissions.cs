using IntelliMed.Core.Constants;
using OrchardCore.Security.Permissions;
using OrganiMedCore.Organization.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.DiabetesCare
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission AgpAvailable = new Permission(nameof(AgpAvailable), "Access AGP+ view");


        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
            => new[]
            {
                new PermissionStereotype
                {
                    Name = WellKnownNames.AdminRoleName,
                    Permissions = new[] { AgpAvailable }
                },
                new PermissionStereotype
                {
                    Name = OrganizationRoleNames.ChiefDoctor,
                    Permissions = new[] { AgpAvailable }
                },
                new PermissionStereotype
                {
                    Name = OrganizationRoleNames.Doctor,
                    Permissions = new[] { AgpAvailable }
                },
                new PermissionStereotype
                {
                    Name = OrganizationRoleNames.OrganizationAdmin,
                    Permissions = new[] { AgpAvailable }
                }
            };

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
            => Task.FromResult(
                new[]
                {
                    AgpAvailable
                }.AsEnumerable());
    }
}
