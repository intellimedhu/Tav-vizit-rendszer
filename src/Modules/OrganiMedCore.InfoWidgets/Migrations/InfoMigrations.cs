using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Models;
using OrchardCore.Liquid.Models;
using OrchardCore.Lists.Models;
using OrganiMedCore.Bootstrap.Models;
using OrganiMedCore.InfoWidgets.Constants;
using OrganiMedCore.InfoWidgets.Models;

namespace OrganiMedCore.InfoWidgets.Migrations
{
    public class InfoMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public InfoMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            // InfoBlock
            _contentDefinitionManager.AlterPartDefinition(nameof(InfoBlockPart), builder => builder
                .WithDescription("Provides an info block for your contents.")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.InfoBlock, builder => builder
                .DisplayedAs("Info Block")
                .Draftable()
                .WithPart(nameof(InfoBlockPart))
                .WithPart(nameof(BootstrapStylePart), part => part
                    .WithDescription("Style of the info block.")
                )
                .WithPart(nameof(LiquidPart), part => part
                    .WithDescription("Liquid content of the info")
                )
            );

            // InfoBlockContainer
            _contentDefinitionManager.AlterPartDefinition(nameof(InfoBlockContainerPart), builder => builder
                .WithDescription("Provides an info block container for your contents.")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.InfoBlockContainer, builder => builder
                .DisplayedAs("Info Block Container")
                .Stereotype("Widget")
                .WithPart(nameof(InfoBlockContainerPart))
                .WithPart(nameof(BagPart), part => part
                    .WithDescription("Contained Info Blocks")
                    .MergeSettings<ListPartSettings>(settings => settings
                        .ContainedContentTypes = new[] { ContentTypes.InfoBlock })
                )
            );

            return 1;
        }
    }
}
