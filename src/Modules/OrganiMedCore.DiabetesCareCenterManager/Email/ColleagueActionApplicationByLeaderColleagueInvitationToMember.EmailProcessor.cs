using IntelliMed.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
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
    public class ColleagueActionApplicationByLeaderColleagueInvitationToMemberEmailProcessor : IEmailDataProcessor
    {
        private readonly INonceService _nonceService;


        public IHtmlLocalizer T { get; set; }
        public string TemplateId => EmailTemplateIds.ColleagueAction_ByLeader_ColleagueInvitation_ToMember;


        public ColleagueActionApplicationByLeaderColleagueInvitationToMemberEmailProcessor(
            IHtmlLocalizer<ColleagueActionApplicationByLeaderColleagueInvitationToMemberEmailProcessor> htmlLocalizer,
            INonceService nonceService)
        {
            _nonceService = nonceService;

            T = htmlLocalizer;            
        }


        public async Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is ColleagueInvitationByLeaderToMemberViewModel viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            var occupationCaption = OccupationExtensions.GetLocalizedValues(T)
                .FirstOrDefault(x => x.Key == viewModel.Occupation)
                .Value;

            var result = await rawBody
                .ReplaceToken(nameof(ColleagueInvitationByLeaderToMemberViewModel.ColleagueName), viewModel.ColleagueName)
                .ReplaceToken(nameof(ColleagueInvitationByLeaderToMemberViewModel.Occupation), occupationCaption)
                .ReplaceToken(nameof(ColleagueInvitationByLeaderToMemberViewModel.LeaderName), viewModel.LeaderName)
                .ReplaceToken(nameof(ColleagueInvitationByLeaderToMemberViewModel.CenterName), viewModel.CenterName)
                .ReplaceTokenAsync(
                    nameof(ColleagueInvitationByLeaderToMemberViewModel.NonceAccept),
                    async () => await _nonceService.GetUriAsync(viewModel.NonceAccept));

            result = await result.ReplaceTokenAsync(
                nameof(ColleagueInvitationByLeaderToMemberViewModel.NonceReject),
                async () => await _nonceService.GetUriAsync(viewModel.NonceReject));

            return result;
        }
    }
}
