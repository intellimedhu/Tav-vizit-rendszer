using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.InfoWidgets.Extensions;

namespace OrganiMedCore.DiabetesCareCenter.Core.Migrations
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class CenterProfileStatusWidgetMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public CenterProfileStatusWidgetMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(CenterProfileStatusBlockPart), builder => builder
                .WithDescription("Provides a view where all the center profile statuses are described.")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.CenterProfileStatusBlock, builder => builder
               .DisplayedAs("Center profile status")
               .WithPart(nameof(CenterProfileStatusBlockPart))
            );

            _contentDefinitionManager.ExtendInfoBlockContainerTypes(ContentTypes.CenterProfileStatusBlock);

            return 1;
        }
    }
}
