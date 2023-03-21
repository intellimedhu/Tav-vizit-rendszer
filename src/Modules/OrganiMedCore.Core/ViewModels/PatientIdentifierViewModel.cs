using OrganiMedCore.Core.Models.Enums;
using OrganiMedCore.Core.Validations;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Core.ViewModels
{
    public class PatientIdentifierViewModel
    {
        [PatientIdentifier("Type")]
        public string Value { get; set; }

        [Required(ErrorMessage = "Az azonosító típus megadása kötelező.")]
        public PatientIdentifierTypes Type { get; set; }
    }
}
