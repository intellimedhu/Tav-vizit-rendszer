using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Models;
using OrchardCore.Liquid.Models;
using OrchardCore.Lists.Models;
using OrganiMedCore.Bootstrap.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.InfoWidgets.Extensions;
using OrganiMedCore.InfoWidgets.Models;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Migrations
{
    public class CenterProfileEditorInfoMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public CenterProfileEditorInfoMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            // Info block part
            _contentDefinitionManager.AlterPartDefinition(nameof(CenterProfileEditorInfoBlockPart), builder => builder
                .WithDescription("The info block part for the related container.")
            );

            // Info block container part
            _contentDefinitionManager.AlterPartDefinition(nameof(CenterProfileEditorInfoBlockContainerPart), builder => builder
                .WithDescription("The info block container part for the center profile editors.")
            );

            // Info widget part
            _contentDefinitionManager.AlterPartDefinition(nameof(CenterProfileEditorInfoPart), builder => builder
                .WithDescription("The info part for the center profile editors.")
            );

            // Info block content type
            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.CenterProfileEditorInfoBlock, builder => builder
                .DisplayedAs("Center Profile Editor Info Block")
                .WithPart(nameof(InfoBlockPart))
                .WithPart(nameof(BootstrapStylePart))
                .WithPart(nameof(CenterProfileEditorInfoBlockPart))
                .WithPart(nameof(LiquidPart), part => part
                    .WithDescription("Liquid content of the info")
                )
            );

            // Info block container content type
            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.CenterProfileEditorInfoBlockContainer, builder => builder
                .WithPart(nameof(CenterProfileEditorInfoBlockContainerPart))
                .WithPart(nameof(BagPart), part => part
                    .WithDescription("Contained Info Blocks")
                    .MergeSettings<ListPartSettings>(settings => settings
                        .ContainedContentTypes = new[] { ContentTypes.CenterProfileEditorInfoBlock })
                )
            );

            // Info block widget
            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.CenterProfileEditorInfo, builder => builder
                .WithPart(nameof(CenterProfileEditorInfoPart))
            );

            _contentDefinitionManager.ExtendInfoBlockContainerTypes(ContentTypes.CenterProfileEditorInfo);

            return 1;
        }
    }
}
