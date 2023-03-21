using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.UriAuthentication.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class ColleagueActionApplicationCancelledEmailProcessor : CenterProfileColleagueActionNotificationEmailProcessorBase
    {
        public override string TemplateId => EmailTemplateIds.ColleagueAction_ApplicationCancelled;


        public ColleagueActionApplicationCancelledEmailProcessor(
            IHtmlLocalizer<ColleagueActionApplicationCancelledEmailProcessor> htmlLocalizer,
            INonceService nonceService)
            : base(htmlLocalizer, nonceService)
        {
        }
    }
}
