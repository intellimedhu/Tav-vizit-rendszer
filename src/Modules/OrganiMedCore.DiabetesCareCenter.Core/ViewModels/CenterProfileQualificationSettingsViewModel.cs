using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileQualificationSettingsViewModel
    {
        /// <summary>
        /// Required qualifications per occupations.
        /// </summary>
        public List<QualificationPerOccupationViewModel> QualificationsPerOccupations { get; set; } = new List<QualificationPerOccupationViewModel>();


        public void UpdateViewModel(CenterProfileQualificationSettings settings)
        {
            settings.ThrowIfNull();

            var occupations = (Occupation[])Enum.GetValues(typeof(Occupation));
            QualificationsPerOccupations = settings.Qualifications
                .SelectMany(qualification =>
                {
                    return occupations.Select(occupation => new QualificationPerOccupationViewModel()
                    {
                        IsSelected = settings[occupation].Contains(qualification.Id),
                        Occupation = occupation,
                        QualificationId = qualification.Id
                    });
                })
                .ToList();


        }
    }
}
