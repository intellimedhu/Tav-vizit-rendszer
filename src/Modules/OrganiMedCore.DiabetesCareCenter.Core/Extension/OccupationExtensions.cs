using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.Extensions
{
    public static class OccupationExtensions
    {
        public static IDictionary<Occupation, string> GetLocalizedValues(IHtmlLocalizer t)
            => ((Occupation[])Enum.GetValues(typeof(Occupation)))
                .ToDictionary(
                    occupation => occupation,
                    occupation =>
                    {
                        switch (occupation)
                        {
                            case Occupation.Doctor:
                                return t["Orvos"].Value;

                            case Occupation.Dietician:
                                return t["Dietetikus"].Value;

                            case Occupation.DiabetesNurseEducator:
                                return t["Diabetológiai szakápoló és edukátor"].Value;

                            case Occupation.Nurse:
                                return t["Ápoló"].Value;

                            case Occupation.CommunityNurse:
                                return t["Körzeti közösségi ápoló"].Value;

                            case Occupation.Physiotherapist:
                                return t["Gyógytornász"].Value;

                            case Occupation.Other:
                                return t["Egyéb munkatárs (adminisztrátor)"].Value;

                            default:
                                return occupation.ToString();
                        }
                    }
                );

        public static Occupation[] GetOccupationsRequiredForGraduation()
            => new[]
            {
                Occupation.Nurse,
                Occupation.CommunityNurse,
                Occupation.Physiotherapist,
                Occupation.Other
            };

        public static Occupation[] GetOccupationsRequiredForOtherQualification()
            => new[] { Occupation.Nurse };
    }
}
