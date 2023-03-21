using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.UriAuthentication.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Email
{
    public class ColleagueActionApplicationByLeaderRemoveActiveEmailProcessor : ColleagueActionApplicationByLeaderEmailProcessorBase
    {
        public override string TemplateId => EmailTemplateIds.ColleagueAction_ByLeader_RemoveActive;


        public ColleagueActionApplicationByLeaderRemoveActiveEmailProcessor(
            IHtmlLocalizer<ColleagueActionApplicationByLeaderRemoveActiveEmailProcessor> htmlLocalizer,
            INonceService nonceService)
            : base(htmlLocalizer, nonceService)
        {
        }
    }
}
