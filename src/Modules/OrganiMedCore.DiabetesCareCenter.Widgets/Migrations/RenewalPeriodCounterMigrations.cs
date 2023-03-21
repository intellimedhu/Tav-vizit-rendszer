using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.InfoWidgets.Extensions;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Migrations
{
    public class RenewalPeriodCounterMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public RenewalPeriodCounterMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(RenewalPeriodCounterPart), builder => builder
                .WithDescription("The renewal period counter part")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.RenewalPeriodCounter, builder => builder
                .DisplayedAs("Renewal Period Counter")
                .WithPart(nameof(RenewalPeriodCounterPart))
            );

            _contentDefinitionManager.ExtendInfoBlockContainerTypes(ContentTypes.RenewalPeriodCounter);

            return 1;
        }
    }
}
