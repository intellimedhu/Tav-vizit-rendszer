using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.UriAuthentication.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class ColleagueActionInvitationAcceptedEmailProcessor : CenterProfileColleagueActionNotificationEmailProcessorBase
    {
        public override string TemplateId => EmailTemplateIds.ColleagueAction_InvitationAccepted;


        public ColleagueActionInvitationAcceptedEmailProcessor(
            IHtmlLocalizer<ColleagueActionInvitationAcceptedEmailProcessor> htmlLocalizer,
            INonceService nonceService)
            : base(htmlLocalizer, nonceService)
        {
        }
    }
}
