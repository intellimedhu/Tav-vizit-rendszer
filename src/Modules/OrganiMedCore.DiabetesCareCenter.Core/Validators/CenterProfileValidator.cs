using IntelliMed.Core.Constants;
using IntelliMed.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace OrganiMedCore.DiabetesCareCenter.Core.Validators
{
    public class CenterProfileValidator : ICenterProfileValidator
    {
        public void ValidateBasicData(CenterProfilePart part, IHtmlLocalizer t, Action<string, string> reportError)
        {
            if (string.IsNullOrEmpty(part.CenterName))
            {
                reportError(
                    nameof(CenterProfileBasicDataViewModel.CenterName),
                    t["A szakellátóhely nevének megadása kötelező."].Value);
            }

            if (part.CenterZipCode == 0)
            {
                reportError(
                    nameof(CenterProfileBasicDataViewModel.CenterZipCode),
                    t["Az irányítószám megadása kötelező."].Value);
            }
            else if (part.CenterZipCode < 1000 || part.CenterZipCode > 9999)
            {
                reportError(
                    nameof(CenterProfileBasicDataViewModel.CenterZipCode),
                    t["Az irányítószám 4 db számjegy."].Value);
            }

            if (string.IsNullOrEmpty(part.CenterSettlementName))
            {
                reportError(
                    nameof(CenterProfileBasicDataViewModel.CenterSettlementName),
                    t["A település megadása kötelező."].Value);
            }

            if (string.IsNullOrEmpty(part.CenterAddress))
            {
                reportError(
                    nameof(CenterProfileBasicDataViewModel.CenterAddress),
                    t["A cím megadása kötelező."].Value);
            }

            if (part.CenterTypes == null ||
                (!part.CenterTypes.Contains(CenterType.Adult) && !part.CenterTypes.Contains(CenterType.Child)))
            {
                reportError(
                    nameof(CenterProfileBasicDataViewModel.CenterTypes),
                    t["Legalább egy profil típust kötelező választani."].Value);
            }
            else
            {
                if (part.CenterTypes.Contains(CenterType.Adult) && part.CenterTypes.Contains(CenterType.Child))
                {
                    reportError(
                        nameof(CenterType.Adult) + "&" + nameof(CenterType.Child),
                        t["Felnőtt és gyermek szakellátóhelyek közül csak az egyiket lehet kiválasztani"].Value);
                }

                if (part.CenterTypes.Contains(CenterType.Child) && part.CenterTypes.Contains(CenterType.Gestational))
                {
                    reportError(
                        nameof(CenterType.Child) + "&" + nameof(CenterType.Gestational),
                        t["Gyermek szakellátóhely nem lehet egyben gesztációs szakellátóhely is."].Value);
                }
            }

            var regexPhone = new Regex(RegexPatterns.Phone);

            if (string.IsNullOrEmpty(part.Phone))
            {
                reportError(
                    nameof(CenterProfileBasicDataViewModel.Phone),
                    t["A telefonszám megadása kötelező."].Value);
            }
            else
            {
                if (!regexPhone.IsMatch(part.Phone))
                {
                    reportError(
                        nameof(CenterProfileBasicDataViewModel.Phone),
                        t["A telefonszám formátuma nem megfelelő."].Value);
                }
            }

            if (!string.IsNullOrEmpty(part.Fax) && !regexPhone.IsMatch(part.Fax))
            {
                reportError(
                    nameof(CenterProfileBasicDataViewModel.Fax),
                    t["A fax formátuma nem megfelelő."].Value);
            }

            if (string.IsNullOrEmpty(part.Email))
            {
                reportError(
                    nameof(CenterProfileBasicDataViewModel.Email),
                    t["Az email megadása kötelező."].Value);
            }
            else if (!part.Email.IsEmail())
            {
                reportError(
                    nameof(CenterProfileBasicDataViewModel.Email),
                    t["Az email cím formátuma nem megfelelő."].Value);
            }
        }

        public void ValidateAdditionalData(CenterProfilePart part, IHtmlLocalizer t, Action<string, string> reportError)
        {
            if (!part.VocationalClinic && !part.PartOfOtherVocationalClinic)
            {
                reportError(
                    nameof(CenterProfileAdditionalDataViewModel.VocationalClinic),
                    t["Kérjük adja meg, hogy a szakellátóhely önálló diabétesz szakrendelés és/vagy más szakrendelés része"].Value);
            }
            else if (part.PartOfOtherVocationalClinic && string.IsNullOrEmpty(part.OtherVocationalClinic))
            {
                reportError(
                    nameof(CenterProfileAdditionalDataViewModel.OtherVocationalClinic),
                    t["Kérjük adja meg, hogy a szakellátóhely mely más szakrendelés része"].Value);
            }

            if (part.Neak != null && part.Neak.Any())
            {
                if (part.Neak.Any(x => x.NumberOfHours < 1))
                {
                    reportError(
                        nameof(CenterProfileNeakDataViewModel.NumberOfHours),
                        t["Minden NEAK órszám megadása kötelező"].Value);
                }

                if (part.Neak.Any(x => string.IsNullOrEmpty(x.WorkplaceCode)))
                {
                    reportError(
                        nameof(CenterProfileNeakDataViewModel.WorkplaceCode),
                        t["Minden NEAK munkahelyi kód megadása kötelező"].Value);
                }

                var validHours = part.OfficeHours.Where(h => h.Hours.Any(x => x.TimeTo > x.TimeFrom));
                if (validHours.Count() != part.OfficeHours.Count())
                {
                    reportError(
                        nameof(CenterProfileAdditionalDataViewModel.OfficeHours),
                        t["Nem minden rendelési időre igaz, hogy a vége idő későbbi időpont, mint a kezdete idő."].Value);
                }
                else if (part.Neak.Any(x => x.NumberOfHours < x.NumberOfHoursDiabetes))
                {
                    reportError(
                        nameof(CenterProfileNeakDataViewModel.NumberOfHours) +
                        ">=" +
                        nameof(CenterProfileNeakDataViewModel.NumberOfHoursDiabetes),
                        t["A szerződésben foglalt heti óraszám egyetlen NEAK adat esetén sem lehet kevesebb, mint a cukorbetegekre fordított óraszám"].Value);
                }
                else
                {
                    var sumHoursDiabetes = part.Neak.Sum(x => x.NumberOfHoursDiabetes);
                    var hours = validHours.SelectMany(x => x.Hours.Select(h => h.TimeTo.TotalHours - h.TimeFrom.TotalHours)).Sum();
                    if (sumHoursDiabetes != hours)
                    {
                        reportError(
                            nameof(CenterProfileAdditionalDataViewModel.OfficeHours),
                            t["A cukorbetegekre furdított óraszámok összege nem egyezik meg a rendelési idők összegével."].Value);
                    }
                }
            }

            if (part.Antsz == null)
            {
                reportError(
                    nameof(CenterProfileAdditionalDataViewModel.Antsz),
                    t["A Kormányhivatal (ÁNTSZ) engedély megadása kötelező"].Value);
            }
            else
            {
                if (string.IsNullOrEmpty(part.Antsz?.Number))
                {
                    reportError(
                        nameof(CenterProfileAntszDataViewModel.Number),
                        t["A Kormányhivatal (ÁNTSZ) engedély szám megadása kötelező"].Value);
                }

                if (!(part.Antsz?.Date).HasValue)
                {
                    reportError(
                        nameof(CenterProfileAntszDataViewModel.Date),
                        t["A Kormányhivatal (ÁNTSZ) engedély keltének megadása kötelező"].Value);
                }

                if (string.IsNullOrEmpty(part.Antsz?.Id))
                {
                    reportError(
                        nameof(CenterProfileAntszDataViewModel.Id),
                        t["A Kormányhivatal (ÁNTSZ) engedély azonosító kód megadása kötelező"].Value);
                }
            }
        }
    }
}
