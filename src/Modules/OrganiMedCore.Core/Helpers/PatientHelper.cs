using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Models.Enums;
using Microsoft.AspNetCore.Mvc.Localization;
using System.Collections.Generic;

namespace OrganiMedCore.Core.Helpers
{
    public static class PatientHelper
    {
        public static IEnumerable<PatientIdentifier> GetIdentifierModels(IViewLocalizer localizer) =>
            new PatientIdentifier[]
            {
                new PatientIdentifier
                {
                    Value = PatientIdentifierTypes.None,
                    Text = localizer["Nincs"].Value
                },
                new PatientIdentifier
                {
                    Value = PatientIdentifierTypes.Taj,
                    Text = localizer["TAJ szám"].Value
                },
                new PatientIdentifier
                {
                    Value = PatientIdentifierTypes.PassportNumber,
                    Text = localizer["Útlevél szám"].Value
                },
                new PatientIdentifier
                {
                    Value = PatientIdentifierTypes.SeekingAsylumIdNumber,
                    Text = localizer["Menedékes, kérelmező, befogadó igazolvány száma"].Value
                },
                new PatientIdentifier
                {
                    Value = PatientIdentifierTypes.EuropeanHealthInsuranceCard,
                    Text = localizer["Európa kártya szám"].Value
                }
            };
    }
}
