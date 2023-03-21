using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.Core.Constants;
using OrganiMedCore.Core.Indexes;
using OrganiMedCore.Core.Models;

namespace OrganiMedCore.Manager.SharedData.Migrations
{
    public class EVisitOrganizationUserProfileMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public EVisitOrganizationUserProfileMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(EVisitOrganizationUserProfilePart), builder => builder
                .WithDescription("The eVisit organization user profile part."));

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.EVisitOrganizationUserProfile, builder => builder
                .WithPart(nameof(EVisitOrganizationUserProfilePart))
                .Versionable());

            const string Index = nameof(EVisitOrganizationUserProfilePartIndex);
            const string SharedUserId = nameof(EVisitOrganizationUserProfilePartIndex.SharedUserId);
            const string StampNumber = nameof(EVisitOrganizationUserProfilePartIndex.StampNumber);
            const string Email = nameof(EVisitOrganizationUserProfilePartIndex.Email);

            SchemaBuilder.CreateMapIndexTable(Index, table => table
                .Column<int>(SharedUserId)
                .Column<string>(StampNumber)
                .Column<string>(Email));

            SchemaBuilder.AlterTable(Index, table =>
                {
                    var indexPrefix = $"IDX_{Index}_";
                    table.CreateIndex(indexPrefix + SharedUserId, SharedUserId);
                    table.CreateIndex(indexPrefix + StampNumber, StampNumber);
                    table.CreateIndex(indexPrefix + Email, Email);
                });

            return 1;
        }
    }
}
