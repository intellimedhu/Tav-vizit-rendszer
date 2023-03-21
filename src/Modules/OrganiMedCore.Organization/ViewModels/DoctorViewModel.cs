using OrganiMedCore.Core.Models;
using OrganiMedCore.Organization.Models;

namespace OrganiMedCore.Organization.ViewModels
{
    public class DoctorViewModel
    {
        public EVisitOrganizationUserProfilePart EVisitOrganizationUserProfilePart { get; set; }

        public OrganizationUserProfilePart OrganizationUserProfilePart { get; set; }
    }
}
