using OrganiMedCore.DiabetesCareCenterManager.ViewModels;
using OrchardCore.Users.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public interface ICenterUserService
    {
        // TODO: better naming
        Task<IEnumerable<LocalUserWithDokiNetMemberViewModel>> GetUsersByLocalUsersAsync(IEnumerable<User> localUsers);

        Task<IEnumerable<LocalUserWithDokiNetMemberViewModel>> GetUsersWithCenterRolesAsync();
    }
}
