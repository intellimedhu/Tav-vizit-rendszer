using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Models;
using OrchardCore.Lists.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Migrations
{
    public class CenterProfileOverviewMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public CenterProfileOverviewMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(CenterProfileOverviewContainerBlockPart), builder => builder
                .WithDescription("The center profile overview container part for the center profile overview.")
            );

            _contentDefinitionManager.AlterTypeDefinition(nameof(ContentTypes.CenterProfileOverviewContainerBlock), builder => builder
                .WithPart(nameof(CenterProfileOverviewContainerBlockPart))
                .WithPart(nameof(BagPart), part => part
                    .WithDescription("Contained Info Blocks")
                    .MergeSettings<ListPartSettings>(settings => settings
                        .ContainedContentTypes = new[]
                        {
                            ContentTypes.AccreditationStatusBlock,
                            InfoWidgets.Constants.ContentTypes.InfoBlock
                        }
                    )
                )
            );

            return 1;
        }
    }
}
