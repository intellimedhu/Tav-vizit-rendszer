using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Users.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Services
{
    public interface IOrganiMedCoreLoginService
    {
        Task<(bool success, bool foundUser)> TrySharedLoginAsync(LoginViewModel model, IUpdateModel updater);
    }
}
