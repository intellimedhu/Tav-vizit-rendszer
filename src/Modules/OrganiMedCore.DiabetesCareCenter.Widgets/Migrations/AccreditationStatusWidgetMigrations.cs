using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.InfoWidgets.Extensions;

namespace OrganiMedCore.DiabetesCareCenter.Core.Migrations
{
    public class AccreditationStatusWidgetMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public AccreditationStatusWidgetMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(AccreditationStatusBlockPart), builder => builder
                .WithDescription("Provides a view where all the accreditation statuses are described.")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.AccreditationStatusBlock, builder => builder
                .DisplayedAs("Accreditation status")
                .WithPart(nameof(AccreditationStatusBlockPart))
            );

            _contentDefinitionManager.ExtendInfoBlockContainerTypes(ContentTypes.AccreditationStatusBlock);

            return 1;
        }
    }
}
