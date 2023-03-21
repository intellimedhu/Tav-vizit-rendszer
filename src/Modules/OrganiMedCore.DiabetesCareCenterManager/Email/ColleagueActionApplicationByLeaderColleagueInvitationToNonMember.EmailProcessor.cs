using IntelliMed.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Extensions;
using OrganiMedCore.Email.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class ColleagueActionApplicationByLeaderColleagueInvitationToNonMemberEmailProcessor : IEmailDataProcessor
    {
        public IHtmlLocalizer T { get; set; }
        public string TemplateId => EmailTemplateIds.ColleagueAction_ByLeader_ColleagueInvitation_ToNonMember;


        public ColleagueActionApplicationByLeaderColleagueInvitationToNonMemberEmailProcessor(
            IHtmlLocalizer<ColleagueActionApplicationByLeaderColleagueInvitationToNonMemberEmailProcessor> htmlLocalizer)
        {
            T = htmlLocalizer;
        }


        public Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is ColleagueInvitationByLeaderToNonMemberViewModel viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            var occupationCaption = OccupationExtensions.GetLocalizedValues(T)
                .FirstOrDefault(x => x.Key == viewModel.Occupation)
                .Value;

            return Task.FromResult(
                rawBody
                    .ReplaceToken(nameof(ColleagueInvitationByLeaderToNonMemberViewModel.AfterSignUpUrl), viewModel.AfterSignUpUrl)
                    .ReplaceToken(nameof(ColleagueInvitationByLeaderToNonMemberViewModel.CenterName), viewModel.CenterName)
                    .ReplaceToken(nameof(ColleagueInvitationByLeaderToNonMemberViewModel.ColleagueName), viewModel.ColleagueName)
                    .ReplaceToken(nameof(ColleagueInvitationByLeaderToNonMemberViewModel.LeaderName), viewModel.LeaderName)
                    .ReplaceToken(nameof(ColleagueInvitationByLeaderToNonMemberViewModel.Occupation), occupationCaption));
        }
    }
}
