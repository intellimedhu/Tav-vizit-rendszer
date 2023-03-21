using OrchardCore.ResourceManagement;

namespace OrganiMedCore.PatientApp
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("OrganiMedCore.PatientApp")
                .SetUrl("/OrganiMedCore.PatientApp/js/build.js");
        }
    }
}
