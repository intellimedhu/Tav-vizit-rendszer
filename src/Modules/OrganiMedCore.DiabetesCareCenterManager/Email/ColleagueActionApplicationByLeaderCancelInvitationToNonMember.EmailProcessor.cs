using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Extensions;
using OrganiMedCore.Email.Services;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class ColleagueActionApplicationByLeaderCancelInvitationToNonMemberEmailProcessor : IEmailDataProcessor
    {
        public string TemplateId => EmailTemplateIds.ColleagueAction_ByLeader_CancelInvitation_ToNonMember;


        public Task<string> ProcessAsync(object data, string rawBody)
        {
            data.ThrowIfNull();
            rawBody.ThrowIfNull();
            if (!(data is CenterProfileColleagueActionNotificationViewModel viewModel))
            {
                throw new ArgumentException(nameof(data));
            }

            return Task.FromResult(
                rawBody
                    .ReplaceToken(nameof(CenterProfileColleagueActionNotificationViewModel.LeaderName), viewModel.LeaderName)
                    .ReplaceToken(nameof(CenterProfileColleagueActionNotificationViewModel.CenterName), viewModel.CenterName)
                    .ReplaceToken(nameof(CenterProfileColleagueActionNotificationViewModel.ColleagueName), viewModel.ColleagueName)
            );
        }
    }
}
