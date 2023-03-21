using OrchardCore.ResourceManagement;

namespace OrganiMedCore.Organization.BaseTheme
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineStyle("OrganiMedCore.Organization.BaseTheme.Styles")
                .SetUrl(
                    "/OrganiMedCore.Organization.BaseTheme/Styles/site.min.css",
                    "/OrganiMedCore.Organization.BaseTheme/Styles/site.css");
        }
    }
}
