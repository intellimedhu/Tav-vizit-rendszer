using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.UriAuthentication.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class ColleagueActionApplicationByLeaderRejectApplicationEmailProcessor : ColleagueActionApplicationByLeaderEmailProcessorBase
    {
        public override string TemplateId => EmailTemplateIds.ColleagueAction_ByLeader_RejectApplication;


        public ColleagueActionApplicationByLeaderRejectApplicationEmailProcessor(
            IHtmlLocalizer<ColleagueActionApplicationByLeaderRejectApplicationEmailProcessor> htmlLocalizer,
            INonceService nonceService)
            : base(htmlLocalizer, nonceService)
        {
        }
    }
}
