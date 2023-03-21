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
    public class CenterProfileLeaderChangedEmailProcessor : IEmailDataProcessor
    {
        private readonly INonceService _nonceService;


        public CenterProfileLeaderChangedEmailProcessor(INonceService nonceService)
        {
            _nonceService = nonceService;
        }


        public string TemplateId => EmailTemplateIds.CenterProfileLeaderChanged;


        public async Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is CenterProfileLeaderChanged viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            return await rawBody
                .ReplaceToken(nameof(CenterProfileLeaderChanged.NewLeaderName), viewModel.NewLeaderName)
                .ReplaceToken(nameof(CenterProfileLeaderChanged.CenterName), viewModel.CenterName)
                .ReplaceTokenAsync(nameof(CenterProfileLeaderChanged.Nonce), async () => await _nonceService.GetUriAsync(viewModel.Nonce));
        }
    }
}
