using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.UriAuthentication.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class ColleagueActionApplicationByLeaderCancelInvitationToMemberEmailProcessor : ColleagueActionApplicationByLeaderEmailProcessorBase
    {
        public override string TemplateId => EmailTemplateIds.ColleagueAction_ByLeader_CancelInvitation_ToMember;


        public ColleagueActionApplicationByLeaderCancelInvitationToMemberEmailProcessor(
            IHtmlLocalizer<ColleagueActionApplicationByLeaderCancelInvitationToMemberEmailProcessor> htmlLocalizer,
            INonceService nonceService)
            : base(htmlLocalizer, nonceService)
        {
        }
    }
}
