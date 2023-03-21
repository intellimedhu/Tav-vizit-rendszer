using IntelliMed.DokiNetIntegration.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenter.Core.Helpers
{
    internal class ColleagueAccreditationHelper
    {
        public int MemberRightId { get; set; }

        public Occupation Occupation { get; set; }

        public DiabetesUserProfilePart DiabetesUserProfile { get; set; }

        public DokiNetMember DokiNetMember { get; set; }
    }
}
