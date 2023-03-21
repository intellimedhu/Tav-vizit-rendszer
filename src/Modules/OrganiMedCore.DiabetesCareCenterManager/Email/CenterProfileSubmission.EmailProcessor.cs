using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Extensions;
using OrganiMedCore.Email.Services;
using OrganiMedCore.UriAuthentication.Services;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class CenterProfileSubmissionEmailProcessor : IEmailDataProcessor
    {
        private readonly INonceService _nonceService;


        public CenterProfileSubmissionEmailProcessor(INonceService nonceService)
        {
            _nonceService = nonceService;
        }


        public string TemplateId => EmailTemplateIds.CenterProfileSubmission;


        public async Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is CenterProfileSubmissionViewModel viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            return await rawBody
                .ReplaceToken(nameof(CenterProfileSubmissionViewModel.ReviewerMemberFullName), viewModel.ReviewerMemberFullName)
                .ReplaceToken(nameof(CenterProfileSubmissionViewModel.CenterLeaderName), viewModel.CenterLeaderName)
                .ReplaceToken(nameof(CenterProfileSubmissionViewModel.CenterName), viewModel.CenterName)
                .ReplaceTokenAsync(nameof(CenterProfileSubmissionViewModel.Nonce), async () => await _nonceService.GetUriAsync(viewModel.Nonce));
        }
    }
}
