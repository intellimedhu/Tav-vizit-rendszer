﻿using IntelliMed.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Extensions;
using OrganiMedCore.Email.Services;
using OrganiMedCore.UriAuthentication.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public abstract class CenterProfileColleagueActionNotificationEmailProcessorBase : IEmailDataProcessor
    {
        private readonly INonceService _nonceService;


        public IHtmlLocalizer T { get; set; }
        public abstract string TemplateId { get; }


        public CenterProfileColleagueActionNotificationEmailProcessorBase(
            IHtmlLocalizer htmlLocalizer,
            INonceService nonceService)
        {            
            _nonceService = nonceService;

            T = htmlLocalizer;
        }


        public virtual async Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is CenterProfileColleagueActionNotificationViewModel viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            var occupation = OccupationExtensions.GetLocalizedValues(T)
                .First(x => x.Key == viewModel.Occupation).Value;

            return await rawBody
                .ReplaceToken(nameof(CenterProfileColleagueActionNotificationViewModel.ColleagueName), viewModel.ColleagueName)
                .ReplaceToken(nameof(CenterProfileColleagueActionNotificationViewModel.LeaderName), viewModel.LeaderName)
                .ReplaceToken(nameof(CenterProfileColleagueActionNotificationViewModel.CenterName), viewModel.CenterName)
                .ReplaceToken(nameof(CenterProfileColleagueActionNotificationViewModel.Occupation), occupation)
                .ReplaceTokenAsync(
                    nameof(CenterProfileColleagueActionNotificationViewModel.Nonce),
                    async () => await _nonceService.GetUriAsync(viewModel.Nonce));
        }
    }
}
