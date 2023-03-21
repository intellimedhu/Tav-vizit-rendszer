using IntelliMed.Core.Constants;
using OrchardCore.Security.Permissions;
using OrganiMedCore.Organization.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageOrganizationSettings = new Permission("ManageOrganizationSettings", "Manage organization settings");
        public static readonly Permission ManageOrganizationUserProfiles = new Permission("ManageOrganizationUserProfiles", "Manage organization user profiles");
        public static readonly Permission ManageOrganizationUnits = new Permission("ManageOrganizationUnits", "Manage organization units");
        public static readonly Permission ManagePatinets = new Permission("ManagePatinets", "Manage organization patients");
        public static readonly Permission ManageReception = new Permission("ManageReception", "Manage reception");
        public static readonly Permission ManageTreatmentWorkflow = new Permission("ManageTreatmentWorkflow", "Manage treatment workflow");


        public Task<IEnumerable<Permission>> GetPermissionsAsync()
            => Task.FromResult(new[]
            {
                ManageOrganizationSettings,
                ManageOrganizationUserProfiles,
                ManageOrganizationUnits,
                ManagePatinets,
                ManageReception,
                ManageTreatmentWorkflow
            }.AsEnumerable());

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = WellKnownNames.AdminRoleName,
                    Permissions = new[] { ManageOrganizationSettings }
                },
                new PermissionStereotype
                {
                    Name = OrganizationRoleNames.ChiefDoctor,
                    Permissions = new[] { ManagePatinets, ManageTreatmentWorkflow }
                },
                new PermissionStereotype
                {
                    Name = OrganizationRoleNames.Doctor,
                    Permissions = new[] { ManagePatinets, ManageTreatmentWorkflow }
                },
                new PermissionStereotype
                {
                    Name = OrganizationRoleNames.SpecialAssistant,
                    Permissions = new Permission[0]
                },
                new PermissionStereotype
                {
                    Name = OrganizationRoleNames.Assistant,
                    Permissions = new Permission[0]
                },
                new PermissionStereotype
                {
                    Name = OrganizationRoleNames.LaboratoryTechnician,
                    Permissions = new Permission[0]
                },
                new PermissionStereotype
                {
                    Name = OrganizationRoleNames.Receptionist,
                    Permissions = new[] { ManagePatinets, ManageReception }
                },
                new PermissionStereotype
                {
                    Name = OrganizationRoleNames.OrganizationAdmin,
                    Permissions = new[] { ManageOrganizationUserProfiles, ManageOrganizationUnits }
                }
            };
        }
    }
}