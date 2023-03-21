using OrchardCore.ContentManagement.Metadata.Builders;
using OrganiMedCore.Organization.Models;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public static class ContentTypeDefinitionBuilderExtensions
    {
        /// <summary>
        /// Alters the content item to a standard EVisit content which is versionable can be edited from the dashboard
        /// and stores metadata about the origins of the content.
        /// </summary>
        public static ContentTypeDefinitionBuilder StandardOrganizationData(this ContentTypeDefinitionBuilder builder)
        {
            return builder
                .WithPart(nameof(MetaDataPart))
                .Listable()
                .Versionable();
        }
    }
}
