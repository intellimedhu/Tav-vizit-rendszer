using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates
{
    public class ColleagueInvitationByLeaderToNonMemberViewModel
    {
        public string ColleagueName { get; set; }

        public string CenterName { get; set; }

        public string LeaderName { get; set; }

        public Occupation? Occupation { get; set; }

        public string AfterSignUpUrl { get; set; }
    }
}
