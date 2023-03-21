using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Helpers
{
    public static class CenterProfileHelpers
    {
        public static object GetBasicAdditionalData(DokiNetMember leaderDokiNetMember)
            => new
            {
                leader = new CenterProfileLeaderViewModel()
                {
                    FullName = leaderDokiNetMember.FullName,
                    MemberRightId = leaderDokiNetMember.MemberRightId,
                    Email = leaderDokiNetMember.Emails.FirstOrDefault()
                }
            };

        public static object GetAdditionalAdditionalData(IHtmlLocalizer t)
            => new
            {
                days = DayCaptions.GetLocalizedValues(t)
                    .Select(x => new KeyValuePair<int, string>((int)x.Key, x.Value))
            };

        public static Task<Dictionary<string, List<string>>> ValidateAdditionalDataAsync(CenterProfileAdditionalDataViewModel viewModel)
        {
            var result = new Dictionary<string, List<string>>();

            if (viewModel.VocationalClinic != true && viewModel.PartOfOtherVocationalClinic != true)
            {
                AddAdditionalDataValidationError(result, "Kérjük, adja meg a szakellátóhely típusát");
            }

            if (viewModel.PartOfOtherVocationalClinic == true &&
                (string.IsNullOrEmpty(viewModel.OtherVocationalClinic) ||
                string.IsNullOrWhiteSpace(viewModel.OtherVocationalClinic)))
            {
                AddAdditionalDataValidationError(result, "Kérjük, adja meg, hogy a szakellátóhely mely szakrendelés része");
            }

            if (viewModel.Neak != null && viewModel.Neak.Any())
            {
                var sumNumberOfHoursDiabetes = viewModel.Neak.Sum(neak => neak.NumberOfHoursDiabetes);
                var sumOfficeTimeInHours = viewModel.OfficeHours
                    .SelectMany(day => day.Hours.Select(hour => (hour.TimeTo - hour.TimeFrom).TotalHours))
                    .Sum();

                if (sumNumberOfHoursDiabetes != sumOfficeTimeInHours)
                {
                    AddAdditionalDataValidationError(result, "A cukorbetegekre fordított óraszámok összege nem egyezik a rendelési idők összegével");
                }

                if (viewModel.Neak.Any(neak => neak.NumberOfHours < neak.NumberOfHoursDiabetes))
                {
                    AddAdditionalDataValidationError(
                        result,
                        "A cukorbetegekre fordított óraszám nem lehet több egy NEAK adat esetén sem, mint a szerződésben foglalt heti óraszám");
                }
            }

            return Task.FromResult(result);
        }


        private static void AddAdditionalDataValidationError(Dictionary<string, List<string>> result, string error)
        {
            if (!result.ContainsKey(string.Empty))
            {
                result.Add(string.Empty, new List<string>());
            }

            result[string.Empty].Add(error);
        }
    }
}
