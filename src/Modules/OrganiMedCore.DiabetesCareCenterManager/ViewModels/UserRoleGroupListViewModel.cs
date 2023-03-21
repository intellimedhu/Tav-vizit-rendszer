using Microsoft.AspNetCore.Mvc.Localization;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class UserRoleGroupListViewModel
    {
        public IEnumerable<LocalUserWithDokiNetMemberViewModel> Users { get; set; }

        public LocalizedHtmlString RoleName { get; set; }

        public bool AuthorizedToManageUsers { get; set; }

        public string CenterRole { get; set; }
    }
}
