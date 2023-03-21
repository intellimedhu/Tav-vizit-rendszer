using IntelliMed.Core.Exceptions;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class QualificationService : IQualificationService
    {
        private readonly ISiteService _siteService;


        public QualificationService(ISiteService siteService)
        {
            _siteService = siteService;
        }


        public async Task DeleteQualificationAsync(Guid id)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            var settings = siteSettings.As<CenterProfileQualificationSettings>();
            if (!settings.Qualifications.Any(x => x.Id == id))
            {
                throw new NotFoundException();
            }

            siteSettings.Alter<CenterProfileQualificationSettings>(nameof(CenterProfileQualificationSettings), x =>
            {
                x.Qualifications = x.Qualifications
                    .Where(qualification => qualification.Id != id)
                    .ToList();

                var idInArray = new[] { id };
                x.QualificationsPerOccupations = x.QualificationsPerOccupations
                    .Where(y => y.Value.Except(idInArray).Any())
                    .ToDictionary(y => y.Key, y => new HashSet<Guid>(y.Value.Except(idInArray)));
            });

            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }

        public async Task<QualificationViewModel> GetQualificationAsync(Guid? id)
        {
            var viewModel = new QualificationViewModel();
            if (id.HasValue)
            {
                var settings = await GetQualificationSettingsAsync();
                var qualificaton = settings.Qualifications.FirstOrDefault(x => x.Id == id.Value);
                if (qualificaton == null)
                {
                    throw new NotFoundException();
                }

                viewModel.UpdateViewModel(qualificaton);
            }

            return viewModel;
        }

        public async Task<CenterProfileQualificationSettings> GetQualificationSettingsAsync()
            => (await _siteService.GetSiteSettingsAsync()).As<CenterProfileQualificationSettings>();

        public async Task UpdateQualificationAsync(QualificationViewModel viewModel)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var settings = siteSettings.As<CenterProfileQualificationSettings>();

            var nameExists = settings.Qualifications.Any(x =>
                x.Id != viewModel.Id &&
                string.Equals(x.Name, viewModel.Name, StringComparison.InvariantCulture));
            if (nameExists)
            {
                throw new ArgumentException("Adott elnevezéssel már szerepel szakképesítés az adatbázisban.");
            }

            Qualification qualification;
            var isNew = !viewModel.Id.HasValue;
            if (isNew)
            {
                qualification = new Qualification() { Id = Guid.NewGuid() };
            }
            else
            {
                qualification = settings.Qualifications.FirstOrDefault(x => x.Id == viewModel.Id.Value);
                if (qualification == null)
                {
                    throw new NotFoundException();
                }
            }

            viewModel.UpdateModel(qualification);
            siteSettings.Alter<CenterProfileQualificationSettings>(nameof(CenterProfileQualificationSettings), x =>
            {
                if (!isNew)
                {
                    x.Qualifications = x.Qualifications.Where(y => y.Id != qualification.Id).ToList();
                }

                x.Qualifications.Add(qualification);
            });

            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }

        public async Task UpdateQualificationsPerOccupationsAsync(CenterProfileQualificationSettingsViewModel viewModel)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            siteSettings.Alter<CenterProfileQualificationSettings>(nameof(CenterProfileQualificationSettings), settings =>
            {
                settings.QualificationsPerOccupations = viewModel.QualificationsPerOccupations
                    .Where(qualificationToOccupation =>
                        qualificationToOccupation.IsSelected &&
                        settings.Qualifications.Any(qualification => qualification.Id == qualificationToOccupation.QualificationId))
                    .GroupBy(qualificationToOccupation => qualificationToOccupation.Occupation)
                    .ToDictionary(
                        group => group.Key,
                        group => new HashSet<Guid>(group.Select(qualificationToOccupation => qualificationToOccupation.QualificationId)));
            });

            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }
    }
}
