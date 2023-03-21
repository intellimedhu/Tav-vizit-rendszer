using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.UriAuthentication.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class ColleagueActionApplicationSubmittedEmailProcessor : CenterProfileColleagueActionNotificationEmailProcessorBase
    {
        public override string TemplateId => EmailTemplateIds.ColleagueAction_ApplicationSubmitted;


        public ColleagueActionApplicationSubmittedEmailProcessor(
            IHtmlLocalizer<ColleagueActionApplicationSubmittedEmailProcessor> htmlLocalizer,
            INonceService nonceService)
            : base(htmlLocalizer, nonceService)
        {
        }
    }
}
