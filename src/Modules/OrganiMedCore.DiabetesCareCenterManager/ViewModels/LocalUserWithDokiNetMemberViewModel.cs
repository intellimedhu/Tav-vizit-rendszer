using IntelliMed.DokiNetIntegration.Models;
using OrchardCore.Users.Models;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class LocalUserWithDokiNetMemberViewModel
    {
        public User LocalUser { get; set; }

        public DokiNetMember DokiNetMember { get; set; }
    }
}