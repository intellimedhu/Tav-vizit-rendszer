using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Indexes;
using OrganiMedCore.Organization.Models;

namespace OrganiMedCore.Organization.Migrations
{
    public class OrganizationUserProfileMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public OrganizationUserProfileMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(OrganizationUserProfilePart), builder => builder
                .WithDescription("The organization user profile part."));

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.OrganizationUserProfile, builder => builder
                .WithPart(nameof(OrganizationUserProfilePart))
                .Versionable());

            const string Index = nameof(OrganizationUserProfilePartIndex);
            const string EVisitOrganizationUserProfileId = nameof(OrganizationUserProfilePartIndex.EVisitOrganizationUserProfileId);

            SchemaBuilder.CreateMapIndexTable(Index, table => table
                .Column<string>(EVisitOrganizationUserProfileId)
            );

            SchemaBuilder.AlterTable(Index, table =>
            {
                var baseName = $"IDX_{Index}_";
                table.CreateIndex(baseName + EVisitOrganizationUserProfileId, EVisitOrganizationUserProfileId);
            });

            return 1;
        }
    }
}