using OrchardCore.ResourceManagement;

namespace IntelliMed.Core
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("jQueryHighlight")
                .SetDependencies("jQuery")
                .SetUrl("/IntelliMed.Core/js/jquery.highlight.js");
        }
    }
}
