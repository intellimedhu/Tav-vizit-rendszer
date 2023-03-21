using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Organization.ViewModels
{
    public class AddToDailyListViewModel
    {
        [Required]
        public string EVisitPatientProfileId { get; set; }

        [Required]
        public string OrganizationUnitId { get; set; }

        [Required]
        public string EVisitOrganizationUserProfileId { get; set; }
    }
}
