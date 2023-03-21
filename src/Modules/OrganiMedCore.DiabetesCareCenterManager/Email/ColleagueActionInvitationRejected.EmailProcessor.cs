using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.UriAuthentication.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class ColleagueActionInvitationRejectedEmailProcessor : CenterProfileColleagueActionNotificationEmailProcessorBase
    {
        public override string TemplateId => EmailTemplateIds.ColleagueAction_InvitationRejected;


        public ColleagueActionInvitationRejectedEmailProcessor(
            IHtmlLocalizer<ColleagueActionInvitationRejectedEmailProcessor> htmlLocalizer,
            INonceService nonceService)
            : base(htmlLocalizer, nonceService)
        {
        }
    }
}
