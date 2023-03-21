using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Extensions;
using OrganiMedCore.Email.Services;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class CenterProfileCreatedEmailProcessor : IEmailDataProcessor
    {
        public string TemplateId => EmailTemplateIds.CenterProfileCreated;


        public Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is CenterProfileCreatedTemplateViewModel viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            return Task.FromResult(
                rawBody
                    .ReplaceToken(nameof(CenterProfileCreatedTemplateViewModel.CenterName), viewModel.CenterName)
                    .ReplaceToken(nameof(CenterProfileCreatedTemplateViewModel.LeaderName), viewModel.LeaderName)
            );
        }
    }
}
