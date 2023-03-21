using OrchardCore.ResourceManagement;

namespace IntelliMed.DokiNetIntegration
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("MemberSearch")
                .SetDependencies("jQuery", "jQuery-UI")
                .SetUrl("/IntelliMed.DokiNetIntegration/js/member-search.js");
        }
    }
}
