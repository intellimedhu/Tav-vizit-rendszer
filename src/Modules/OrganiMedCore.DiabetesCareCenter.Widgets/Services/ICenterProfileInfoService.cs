using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Services
{
    public interface ICenterProfileInfoService
    {
        bool AllowedContentType(string contentType);

        Task<(ContentItem contentItem, bool isNew)> GetOrCreateNewContentItemAsync(string contentType);

        Task SaveContentItemAsync(ContentItem contentItem, bool isNew);
    }
}
