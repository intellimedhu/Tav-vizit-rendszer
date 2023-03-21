using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.InfoWidgets.Extensions;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Migrations
{
    [Feature("OrganiMedCore.Organization.DiabetesCareCenter.Widgets")]
    public class CenterProfileOverviewOrganizationMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public CenterProfileOverviewOrganizationMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(CenterProfileOverviewInfoPart), builder => builder
                .WithDescription("The center profile overview info part.")
            );

            _contentDefinitionManager.AlterTypeDefinition(nameof(ContentTypes.CenterProfileOverviewInfo), builder => builder
                .WithPart(nameof(CenterProfileOverviewInfoPart))
            );

            _contentDefinitionManager.ExtendInfoBlockContainerTypes(ContentTypes.CenterProfileOverviewInfo);

            return 1;
        }
    }
}
