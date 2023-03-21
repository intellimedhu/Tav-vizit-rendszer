using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Extensions;
using OrganiMedCore.Email.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class CenterProfileSubmissionFeedbackEmailProcessor : IEmailDataProcessor
    {
        public string TemplateId => EmailTemplateIds.CenterProfileSubmissionFeedback;


        public Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is CenterProfileSubmissionFeedbackViewModel viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            return Task.FromResult(
                rawBody
                    .ReplaceToken(nameof(CenterProfileSubmissionFeedbackViewModel.CenterLeaderName), viewModel.CenterLeaderName)
                    .ReplaceToken(nameof(CenterProfileSubmissionFeedbackViewModel.CenterName), viewModel.CenterName)
                    .ReplaceToken(nameof(CenterProfileSubmissionFeedbackViewModel.Reviewers), string.Join(", ", viewModel.Reviewers.OrderBy(x => x)))
            );
        }
    }
}
