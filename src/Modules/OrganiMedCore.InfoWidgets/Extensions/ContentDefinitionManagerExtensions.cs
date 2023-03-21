using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Flows.Models;
using OrchardCore.Lists.Models;
using OrganiMedCore.InfoWidgets.Constants;

namespace OrganiMedCore.InfoWidgets.Extensions
{
    public static class ContentDefinitionManagerExtensions
    {
        public static void ExtendInfoBlockContainerTypes(this IContentDefinitionManager contentDefinitionManager, params string[] containedContentTypes)
            => contentDefinitionManager.AlterTypeDefinition(ContentTypes.InfoBlockContainer, builder => builder
                .WithPart(nameof(BagPart), part => part
                    .MergeSettings<ListPartSettings>(settings =>
                    {
                        settings.ContainedContentTypes = containedContentTypes;
                    })
                )
            );
    }
}
