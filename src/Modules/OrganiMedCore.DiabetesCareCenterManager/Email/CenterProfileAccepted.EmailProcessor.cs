using IntelliMed.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Extension;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Extensions;
using OrganiMedCore.Email.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class CenterProfileAcceptedEmailProcessor : IEmailDataProcessor
    {
        public IHtmlLocalizer T { get; set; }
        public string TemplateId => EmailTemplateIds.CenterProfileAccepted;


        public CenterProfileAcceptedEmailProcessor(IHtmlLocalizer<CenterProfileAcceptedEmailProcessor> htmlLocalizer)
        {
            T = htmlLocalizer;
        }


        public Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is CenterProfileAcceptedViewModel viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            var post = "";
            switch (viewModel.CurrentRole)
            {
                case CenterPosts.TerritorialRapporteur:
                case CenterPosts.MDTSecretary:
                    post = "területi referens";
                    break;

                case CenterPosts.OMKB:
                    post = "MDT-OMKB";
                    break;

                case CenterPosts.MDTManagement:
                    post = "MDT vezetőség";
                    break;
            }

            var status = AccreditationStatusCaptions.GetLocalizedValues(T)
                .First(x => x.Key == viewModel.AccreditationStatus)
                .Value;

            return Task.FromResult(
                rawBody
                    .ReplaceToken(nameof(CenterProfileAcceptedViewModel.PersonName), viewModel.PersonName)
                    .ReplaceToken(nameof(CenterProfileAcceptedViewModel.CenterName), viewModel.CenterName)
                    .ReplaceToken(nameof(CenterProfileAcceptedViewModel.CurrentRole), string.IsNullOrEmpty(post) ? string.Empty : T[post].Value)
                    .ReplaceToken(nameof(CenterProfileAcceptedViewModel.AccreditationStatus), status)
            );
        }
    }
}
