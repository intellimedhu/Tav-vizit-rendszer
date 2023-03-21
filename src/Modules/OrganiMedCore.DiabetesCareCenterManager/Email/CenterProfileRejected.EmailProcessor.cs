using IntelliMed.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Extensions;
using OrganiMedCore.Email.Services;
using OrganiMedCore.UriAuthentication.Services;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class CenterProfileRejectedEmailProcessor : IEmailDataProcessor
    {
        private readonly INonceService _nonceService;


        public IHtmlLocalizer T { get; set; }
        public string TemplateId => EmailTemplateIds.CenterProfileRejected;


        public CenterProfileRejectedEmailProcessor(
            IHtmlLocalizer<CenterProfileRejectedEmailProcessor> htmlLocalizer,
            INonceService nonceService)
        {            
            _nonceService = nonceService;

            T = htmlLocalizer;
        }


        public async Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is CenterProfileRejectedViewModel viewModel))
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

            return await rawBody
                .ReplaceToken(nameof(CenterProfileRejectedViewModel.PersonName), viewModel.PersonName)
                .ReplaceToken(nameof(CenterProfileRejectedViewModel.CenterName), viewModel.CenterName)
                .ReplaceToken(nameof(CenterProfileRejectedViewModel.CurrentRole), string.IsNullOrEmpty(post) ? string.Empty : T[post].Value)
                .ReplaceToken(nameof(CenterProfileRejectedViewModel.RejectReason), viewModel.RejectReason)
                .ReplaceTokenAsync(nameof(CenterProfileRejectedViewModel.Nonce), async () => await _nonceService.GetUriAsync(viewModel.Nonce));
        }
    }
}
