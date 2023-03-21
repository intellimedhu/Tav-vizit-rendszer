using IntelliMed.Core.Constants;
using OrchardCore.Users.ViewModels;
using OrganiMedCore.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Organization.ViewModels
{
    public class OrganizationUserProfilePartViewModel
    {
        [RegularExpression(RegexPatterns.Phone, ErrorMessage = "A telefonszám formátuma nem megfelelő.")]
        public string Phone { get; set; }

        [RegularExpression(RegexPatterns.Email, ErrorMessage = "Az e-mail cím formátuma nem megfelelő.")]
        public string Email { get; set; }

        public string OrganizationRank { get; set; }

        [RegularExpression(RegexPatterns.AntszLicenseNumber, ErrorMessage = "Az ANTSZ engedély szám formátuma nem megfelelő.")]
        public string AntszLicenseNumber { get; set; }

        public string ConsultationHours { get; set; }

        public string CheckInMode { get; set; }

        public PermittedOrganizationUnitViewModel[] OrganizationUnits { get; set; } = new PermittedOrganizationUnitViewModel[0];

        public RoleViewModel[] Roles { get; set; } = new RoleViewModel[0];

        public OrganizationUserProfileTypes? OrganizationUserProfileType { get; set; }
    }
}
