using IntelliMed.DokiNetIntegration.ViewModels;
using OrchardCore.Users.ViewModels;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class EditCenterUserFrontEndViewModel
    {
        public EditDokiNetMemberViewModel EditDokiNetMemberViewModel { get; set; }

        public RoleViewModel RoleManageMDTManagement { get; set; }

        public RoleViewModel RoleManageMDTSecretary { get; set; }

        public RoleViewModel RoleManageOMKB { get; set; }

        public RoleViewModel RoleManageTerritorialRapporteur { get; set; }
    }
}
