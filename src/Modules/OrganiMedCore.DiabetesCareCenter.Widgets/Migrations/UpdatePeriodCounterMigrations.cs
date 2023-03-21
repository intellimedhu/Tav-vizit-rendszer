using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.InfoWidgets.Extensions;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Migrations
{
    public class UpdatePeriodCounterMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public UpdatePeriodCounterMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(UpdatePeriodCounterPart), builder => builder
                .WithDescription("The update period counter part")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.UpdatePeriodCounter, builder => builder
                .DisplayedAs("Update Period Counter")
                .WithPart(nameof(UpdatePeriodCounterPart))
            );

            _contentDefinitionManager.ExtendInfoBlockContainerTypes(ContentTypes.UpdatePeriodCounter);

            return 1;
        }
    }
}
