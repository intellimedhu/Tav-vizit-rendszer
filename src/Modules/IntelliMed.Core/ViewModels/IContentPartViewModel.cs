using OrchardCore.ContentManagement;

namespace IntelliMed.Core.ViewModels
{
    public interface IContentPartViewModel<TContentPart> where TContentPart : ContentPart
    {
        void UpdateViewModel(TContentPart part);

        void UpdatePart(TContentPart part);
    }
}
