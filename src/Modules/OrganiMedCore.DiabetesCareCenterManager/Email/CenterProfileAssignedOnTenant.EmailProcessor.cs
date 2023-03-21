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
    public class CenterProfileAssignedOnTenantEmailProcessor : IEmailDataProcessor
    {
        private readonly INonceService _nonceService;


        public string TemplateId => EmailTemplateIds.CenterProfileAssignedOnTenant;


        public CenterProfileAssignedOnTenantEmailProcessor(INonceService nonceService)
        {
            _nonceService = nonceService;
        }


        public async Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is CenterProfileAssignedOnTenantViewModel viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            return await rawBody
                .ReplaceToken(nameof(CenterProfileAssignedOnTenantViewModel.LeaderName), viewModel.LeaderName)
                .ReplaceToken(nameof(CenterProfileAssignedOnTenantViewModel.CenterName), viewModel.CenterName)
                .ReplaceTokenAsync(
                    nameof(CenterProfileAssignedOnTenantViewModel.Nonce),
                    async () => await _nonceService.GetUriAsync(viewModel.Nonce));
        }
    }
}
