using IntelliMed.Core.Constants;
using IntelliMed.Core.Extensions;
using IntelliMed.Core.ViewModels;
using OrganiMedCore.Organization.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Organization.ViewModels
{
    public class PatientProfilePartViewModel : IContentPartViewModel<PatientProfilePart>
    {
        [RegularExpression(RegexPatterns.Phone, ErrorMessage = "A telefonszám formátuma nem megfelelő.")]
        public string Phone { get; set; }

        [RegularExpression(RegexPatterns.Phone, ErrorMessage = "A másodlagos telefonszám formátuma nem megfelelő.")]
        public string PhoneSecondary { get; set; }

        [RegularExpression(RegexPatterns.Email, ErrorMessage = "A másodlagos e-mail cím formátuma nem megfelelő.")]
        public string EmailSecondary { get; set; }

        public string Occupation { get; set; }

        public string PublicHealthCardNumber { get; set; }

        public DateTime? PublicHealthCardValidationDate { get; set; }

        public string Remarks { get; set; }


        public void UpdatePart(PatientProfilePart part)
        {
            part.ThrowIfNull();

            part.Phone = Phone;
            part.PhoneSecondary = PhoneSecondary;
            part.EmailSecondary = EmailSecondary;
            part.Occupation = Occupation;
            part.PublicHealthCardNumber = PublicHealthCardNumber;
            part.PublicHealthCardValidationDate = PublicHealthCardValidationDate;
            part.Remarks = Remarks;
        }

        public void UpdateViewModel(PatientProfilePart part)
        {
            part.ThrowIfNull();

            Phone = part.Phone;
            PhoneSecondary = part.PhoneSecondary;
            EmailSecondary = part.EmailSecondary;
            Occupation = part.Occupation;
            PublicHealthCardNumber = part.PublicHealthCardNumber;
            PublicHealthCardValidationDate = part.PublicHealthCardValidationDate;
            Remarks = part.Remarks;
        }
    }
}
