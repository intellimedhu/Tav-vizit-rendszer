using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.UriAuthentication.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class ColleagueActionApplicationByLeaderAcceptApplicationEmailProcessor : ColleagueActionApplicationByLeaderEmailProcessorBase
    {
        public override string TemplateId => EmailTemplateIds.ColleagueAction_ByLeader_AcceptApplication;


        public ColleagueActionApplicationByLeaderAcceptApplicationEmailProcessor(
            IHtmlLocalizer<ColleagueActionApplicationByLeaderAcceptApplicationEmailProcessor> htmlLocalizer,
            INonceService nonceService)
            : base(htmlLocalizer, nonceService)
        {
        }
    }
}
