using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Liquid.Models;
using OrchardCore.Modules;
using OrganiMedCore.Bootstrap.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.InfoWidgets.Extensions;
using OrganiMedCore.InfoWidgets.Models;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Migrations
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class CenterProfileReviewBlocksMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public CenterProfileReviewBlocksMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            // Review block for the list
            _contentDefinitionManager.AlterPartDefinition(nameof(CenterProfileListReviewBlockPart), builder => builder
                .WithDescription("Review info widget for center profile list.")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.CenterProfileListReviewBlock, builder => builder
                .WithPart(nameof(CenterProfileListReviewBlockPart))
                .WithPart(nameof(InfoBlockPart))
                .WithPart(nameof(BootstrapStylePart))
                .WithPart(nameof(LiquidPart), part => part
                    .WithDescription("Liquid content of the info")
                )
            );

            // Review block one center profile
            _contentDefinitionManager.AlterPartDefinition(nameof(CenterProfileReviewBlockPart), builder => builder
                .WithDescription("Review info widget for center profile review.")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.CenterProfileReviewBlock, builder => builder
                .WithPart(nameof(CenterProfileReviewBlockPart))
                .WithPart(nameof(InfoBlockPart))
                .WithPart(nameof(BootstrapStylePart))
                .WithPart(nameof(LiquidPart), part => part
                    .WithDescription("Liquid content of the info")
                )
            );

            // Info block container content type
            _contentDefinitionManager.ExtendInfoBlockContainerTypes(
                ContentTypes.CenterProfileReviewBlock,
                ContentTypes.CenterProfileListReviewBlock
            );

            return UpdateFrom1();
        }

        public int UpdateFrom1()
        {
            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.CenterProfileListReviewBlock, builder => builder
                .RemovePart(nameof(InfoBlockPart))
                .RemovePart(nameof(BootstrapStylePart))
                .RemovePart(nameof(LiquidPart))
            );

            return 2;
        }
    }
}
