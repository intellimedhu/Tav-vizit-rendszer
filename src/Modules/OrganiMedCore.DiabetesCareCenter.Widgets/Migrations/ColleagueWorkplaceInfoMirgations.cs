using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Models;
using OrchardCore.Liquid.Models;
using OrchardCore.Lists.Models;
using OrchardCore.Modules;
using OrganiMedCore.Bootstrap.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.InfoWidgets.Extensions;
using OrganiMedCore.InfoWidgets.Models;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Migrations
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class ColleagueWorkplaceInfoMirgations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public ColleagueWorkplaceInfoMirgations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            // Colleague workplace block
            _contentDefinitionManager.AlterPartDefinition(nameof(ColleagueWorkplaceBlockPart), builder => builder
                .WithDescription("The colleague workplace block for the related container")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.ColleagueWorkplaceBlock, builder => builder
                .WithPart(nameof(InfoBlockPart))
                .WithPart(nameof(BootstrapStylePart))
                .WithPart(nameof(ColleagueWorkplaceBlockPart))
                .WithPart(nameof(LiquidPart), part => part
                    .WithDescription("Liquid content of the info")
                )
            );

            // Colleague workplace container block
            _contentDefinitionManager.AlterPartDefinition(nameof(ColleagueWorkplaceContainerBlockPart), builder => builder
                .WithDescription("The colleague workplace container block part for the colleagues.")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.ColleagueWorkplaceContainerBlock, builder => builder
                .WithPart(nameof(ColleagueWorkplaceContainerBlockPart))
                .WithPart(nameof(BagPart), part => part
                    .MergeSettings<ListPartSettings>(settings => settings
                        .ContainedContentTypes = new[] { ContentTypes.ColleagueWorkplaceBlock })
                )
            );

            // Adding new type for the info block container
            _contentDefinitionManager.ExtendInfoBlockContainerTypes(ContentTypes.ColleagueWorkplaceContainerBlock);

            return 1;
        }
    }
}
