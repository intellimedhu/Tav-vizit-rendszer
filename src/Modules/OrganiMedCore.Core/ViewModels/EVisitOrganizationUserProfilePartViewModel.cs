using IntelliMed.Core.Constants;
using Newtonsoft.Json;
using OrganiMedCore.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Core.ViewModels
{
    public class EVisitOrganizationUserProfilePartViewModel
    {
        [RegularExpression(RegexPatterns.DoctorStampNumber, ErrorMessage = "A pecsétszám formátuma nem megfelelő.")]
        public string StampNumber { get; set; }

        [Required]
        [RegularExpression(RegexPatterns.Email, ErrorMessage = "Az e-mail cím formátuma nem megfelelő.")]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [RegularExpression(RegexPatterns.Phone, ErrorMessage = "A telefonszám formátuma nem megfelelő.")]
        public string Phone { get; set; }

        public string EesztId { get; set; }

        public string EesztName { get; set; }

        public string HighestRankOrEducation { get; set; }

        public string MedicalExams { get; set; }

        public string QualificationLicenceExams { get; set; }

        public string QualificationNameNumberDate { get; set; }

        public OrganizationUserProfileTypes? OrganizationUserProfileType { get; set; }
    }
}
