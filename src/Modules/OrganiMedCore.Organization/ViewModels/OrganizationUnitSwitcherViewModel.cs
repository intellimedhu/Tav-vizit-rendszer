using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Organization.ViewModels
{
    public class OrganizationUnitSwitcherViewModel
    {
        [Required]
        public string SelectedOrganizationId { get; set; }
    }
}
