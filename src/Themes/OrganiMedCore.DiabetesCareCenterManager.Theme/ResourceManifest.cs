using OrchardCore.ResourceManagement;

namespace OrganiMedCore.DiabetesCareCenterManager.Theme
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineStyle("DiabetesCareCenterManager.Theme.Styles")
                .SetUrl(
                    "/OrganiMedCore.DiabetesCareCenterManager.Theme/styles/main.min.css",
                    "/OrganiMedCore.DiabetesCareCenterManager.Theme/styles/main.css");
        }
    }
}
