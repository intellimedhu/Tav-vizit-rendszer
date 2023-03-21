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
    public class RenewalPeriodSubmissionReminderEmailProcessor : IEmailDataProcessor
    {
        private readonly INonceService _nonceService;


        public string TemplateId => EmailTemplateIds.RenewalPeriodSubmissionReminder;


        public RenewalPeriodSubmissionReminderEmailProcessor(INonceService nonceService)
        {
            _nonceService = nonceService;
        }


        public async Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is RenewalPeriodSubmissionReminderViewModel viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            var result = rawBody
                .ReplaceToken(nameof(RenewalPeriodSubmissionReminderViewModel.CenterName), viewModel.CenterName)
                .ReplaceToken(nameof(RenewalPeriodSubmissionReminderViewModel.LeaderName), viewModel.LeaderName)
                .ReplaceToken(nameof(RenewalPeriodSubmissionReminderViewModel.Deadline), viewModel.Deadline.ToLocalTime().ToString("yyyy.MM.dd. HH:mm"))
                .Replace("\r\n", "<br />")
                .Replace("\n", "<br />");

            return await result.ReplaceTokenAsync(
                nameof(RenewalPeriodSubmissionReminderViewModel.Nonce),
                async () => await _nonceService.GetUriAsync(viewModel.Nonce));
        }
    }
}
