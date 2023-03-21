using OrchardCore.ContentManagement.Handlers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Handlers
{
    // Updates the DisplayText of the content item to be able to search for it on the admin site.
    public class CenterProfilePartHandler : ContentPartHandler<CenterProfilePart>
    {
        public override Task PublishingAsync(PublishContentContext context, CenterProfilePart instance)
        {
            context.ContentItem.DisplayText = instance.CenterName;

            return Task.CompletedTask;
        }

        public override Task CreatingAsync(CreateContentContext context, CenterProfilePart instance)
        {
            context.ContentItem.DisplayText = instance.CenterName;

            return Task.CompletedTask;
        }

        public override Task UpdatingAsync(UpdateContentContext context, CenterProfilePart instance)
        {
            context.ContentItem.DisplayText = instance.CenterName;

            return Task.CompletedTask;
        }
    }
}
