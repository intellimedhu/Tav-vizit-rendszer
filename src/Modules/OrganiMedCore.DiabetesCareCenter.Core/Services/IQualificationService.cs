using IntelliMed.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface IQualificationService
    {
        /// <exception cref="NotFoundException"></exception>
        Task DeleteQualificationAsync(Guid id);

        /// <exception cref="NotFoundException"></exception>
        Task<QualificationViewModel> GetQualificationAsync(Guid? id);

        Task<CenterProfileQualificationSettings> GetQualificationSettingsAsync();

        /// <exception cref="NotFoundException"></exception>
        Task UpdateQualificationAsync(QualificationViewModel viewModel);

        Task UpdateQualificationsPerOccupationsAsync(CenterProfileQualificationSettingsViewModel viewModel);
    }
}
