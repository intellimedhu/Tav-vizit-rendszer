using OrganiMedCore.Navigation.Models;
using System.Threading.Tasks;

namespace OrganiMedCore.Navigation.Services
{
    public interface IMenuItemProvider
    {
        string MenuId { get; }

        Task BuildMenuAsync(Menu builder, object additionalData = null);
    }
}
