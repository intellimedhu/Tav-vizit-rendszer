using IntelliMed.Core.Constants;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Manager
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageManagerSettings = new Permission("ManageManagerSettings", "Manage manager settings");
        public static readonly Permission ViewAccessLog = new Permission("ViewAccessLog", "View access log");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
            => Task.FromResult(new[]
            {
                ManageManagerSettings,
                ViewAccessLog
            }.AsEnumerable());

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = WellKnownNames.AdminRoleName,
                    Permissions = new[] { ManageManagerSettings, ViewAccessLog }
                }
            };
        }
    }
}