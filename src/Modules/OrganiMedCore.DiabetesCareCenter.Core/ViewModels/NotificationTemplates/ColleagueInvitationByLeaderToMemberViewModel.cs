using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates
{
    public class ColleagueInvitationByLeaderToMemberViewModel
    {
        public string ColleagueName { get; set; }

        public string CenterName { get; set; }

        public string LeaderName { get; set; }

        public Occupation? Occupation { get; set; }

        public Guid NonceAccept { get; set; }

        public Guid NonceReject { get; set; }
    }
}
