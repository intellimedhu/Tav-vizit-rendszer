using OrchardCore.ResourceManagement;

namespace OrganiMedCore.DiabetesCareCenter.Core
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("DiabetesUserProfileEditor")
                .SetUrl("/OrganiMedCore.DiabetesCareCenter.Core/js/dupe.js");
        }
    }
}
