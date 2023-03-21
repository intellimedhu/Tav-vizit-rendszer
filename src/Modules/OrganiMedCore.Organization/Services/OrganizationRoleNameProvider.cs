using OrganiMedCore.Core.Services;
using OrganiMedCore.Organization.Constants;
using System.Collections.Generic;

namespace OrganiMedCore.Organization.Services
{
    public class OrganizationRoleNameProvider : IRoleNameProvider
    {
        public IEnumerable<string> GetRoleNames()
            => new string[]
            {
                OrganizationRoleNames.ChiefDoctor,
                OrganizationRoleNames.Doctor,
                OrganizationRoleNames.SpecialAssistant,
                OrganizationRoleNames.Assistant,
                OrganizationRoleNames.LaboratoryTechnician,
                OrganizationRoleNames.Receptionist
            };
    }
}
