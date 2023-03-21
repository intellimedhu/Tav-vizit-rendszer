using IntelliMed.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Validators
{
    public class DiabetesUserProfileValidator
    {
        public async Task ValidateAsync(
            IQualificationService qualificationService,
            IHtmlLocalizer t,
            DiabetesUserProfilePartViewModel viewModel,
            int currentYear,
            Action<string, string> reportError)
        {
            qualificationService.ThrowIfNull();
            t.ThrowIfNull();
            viewModel.ThrowIfNull();
            reportError.ThrowIfNull();

            var yearMin = 1900;
            ValidateQualifications(await qualificationService.GetQualificationSettingsAsync(), viewModel, yearMin, currentYear, t, reportError);
            ValidateGraduation(viewModel, yearMin, currentYear, t, reportError);
        }

        public async Task ValidateAsync(
            Occupation occupation,
            IQualificationService qualificationService,
            IHtmlLocalizer t,
            DiabetesUserProfilePartViewModel viewModel,
            int currentYear,
            Action<string, string> reportError)
        {
            qualificationService.ThrowIfNull();
            t.ThrowIfNull();
            viewModel.ThrowIfNull();
            reportError.ThrowIfNull();

            var settings = await qualificationService.GetQualificationSettingsAsync();
            var requiredQualifications = settings[occupation];
            var yearMin = 1900;
            if (requiredQualifications.Any())
            {
                if (!requiredQualifications.Any(x => viewModel.Qualifications.Select(y => y.QualificationId).Contains(x)))
                {
                    reportError("RequiredQualifications", t["Legalább egy szakképesítés megadása kötelező"].Value);
                }

                ValidateQualifications(settings, viewModel, yearMin, currentYear, t, reportError);
            }

            if (OccupationExtensions.GetOccupationsRequiredForGraduation().Contains(occupation))
            {
                ValidateGraduation(viewModel, yearMin, currentYear, t, reportError);
            }

            if (OccupationExtensions.GetOccupationsRequiredForOtherQualification().Contains(occupation))
            {
                if (string.IsNullOrEmpty(viewModel.OtherQualification))
                {
                    reportError("OtherQualification", t["Az egyéb képzettség megadása kötelező."].Value);
                }
            }
        }


        private void ValidateQualifications(Settings.CenterProfileQualificationSettings settings, DiabetesUserProfilePartViewModel viewModel, int yearMin, int currentYear, IHtmlLocalizer t, Action<string, string> reportError)
        {
            var personQualificationIds = viewModel.Qualifications.Where(x => x.QualificationId.HasValue).Select(x => x.QualificationId.Value);
            if (new HashSet<Guid>(personQualificationIds).Count != personQualificationIds.Count())
            {
                reportError("QualificationIds", t["Minden szakképesítés csak egyszer rögzíthető."].Value);
            }

            if (viewModel.Qualifications.Any(x => !x.QualificationId.HasValue || !settings.Qualifications.Select(y => y.Id).Contains(x.QualificationId.Value)))
            {
                reportError("QualificationId", t["A megadott szakképesítés nem létezik."].Value);
            }

            if (viewModel.Qualifications.Any(x => string.IsNullOrEmpty(x.QualificationNumber)))
            {
                reportError("QualificationNumber", t["Minden szakképesítéshez kötelező megadni a számát."].Value);
            }

            if (viewModel.Qualifications.Any(x => !x.State.HasValue))
            {
                reportError("State", t["Minden szakképesítéshez kötelező megadni a státuszát."].Value);
            }

            if (viewModel.Qualifications.Any(x => !x.QualificationYear.HasValue || x.QualificationYear < yearMin || x.QualificationYear > currentYear))
            {
                reportError("QualificationYear", t["A szakképesítés éve helytelen."].Value);
            }
        }

        private void ValidateGraduation(DiabetesUserProfilePartViewModel viewModel, int yearMin, int currentYear, IHtmlLocalizer t, Action<string, string> reportError)
        {
            if (!string.IsNullOrEmpty(viewModel.GraduationIssuedBy) && !viewModel.GraduationYear.HasValue)
            {
                reportError("GraduationYear1", t["Az érettségi éve nincs megadva."].Value);
            }
            else if (string.IsNullOrEmpty(viewModel.GraduationIssuedBy) && viewModel.GraduationYear.HasValue)
            {
                reportError("GraduationIssuedBy", t["Az érettségi helye nincs megadva."].Value);
            }

            if (viewModel.GraduationYear.HasValue && (viewModel.GraduationYear < yearMin || viewModel.GraduationYear > currentYear))
            {
                reportError("GraduationYear2", t["A érettségi éve helytelen."].Value);
            }
        }
    }
}
