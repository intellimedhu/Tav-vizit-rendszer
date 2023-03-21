using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Extensions;
using OrganiMedCore.Email.Services;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class CenterProfileLeaderChangedBeforeAssignmentEmailProcessor : IEmailDataProcessor
    {
        public string TemplateId => EmailTemplateIds.CenterProfileLeaderChangedBeforeAssignment;


        public Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is CenterProfileLeaderChanged viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            return Task.FromResult(
                rawBody
                    .ReplaceToken(nameof(CenterProfileLeaderChanged.NewLeaderName), viewModel.NewLeaderName)
                    .ReplaceToken(nameof(CenterProfileLeaderChanged.CenterName), viewModel.CenterName)
            );
        }
    }
}
