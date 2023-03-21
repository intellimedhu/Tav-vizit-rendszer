using OrchardCore.ResourceManagement;

namespace OrganiMedCore.InformationOrientedTheme
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineStyle("InformationOrientedTheme.Styles")
                .SetUrl(
                    "/OrganiMedCore.InformationOrientedTheme/css/main.min.css",
                    "/OrganiMedCore.InformationOrientedTheme/css/main.css");

            manifest
                .DefineScript("InformationOrientedBsModals")
                .SetDependencies("jQuery")
                .SetUrl("/OrganiMedCore.InformationOrientedTheme/js/iobsmodals.js");
        }
    }
}
