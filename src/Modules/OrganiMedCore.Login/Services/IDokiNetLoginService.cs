using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Users.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Services
{
    public interface IDokiNetLoginService
    {
        Task<bool> TryDokiNetLoginAsync(LoginViewModel viewModel, IUpdateModel updater);
    }
}
