using OrganiMedCore.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Organization.ViewModels
{
    public class OrganizationUserIdentifierViewModel
    {
        [Required]
        public string Identifier { get; set; }

        [Required]
        public OrganizationUserIdentifierTypes IdentifierType { get; set; }

        public bool IdentifierChecked { get; set; }

        public bool OrganizationUserExist { get; set; }
    }
}
