using OrchardCore.ResourceManagement;

namespace OrganiMedCore.AgpReports
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineStyle("OrganiMedCore.AgpReports.Style")
                .SetUrl("/OrganiMedCore.AgpReports/static/css/app.css");

            manifest
                .DefineScript("OrganiMedCore.AgpReports.Manifest")
                .SetUrl("/OrganiMedCore.AgpReports/static/js/manifest.js");

            manifest
                .DefineScript("OrganiMedCore.AgpReports.Vendor")
                .SetUrl("/OrganiMedCore.AgpReports/static/js/vendor.js");

            manifest
                .DefineScript("OrganiMedCore.AgpReports.App")
                .SetUrl("/OrganiMedCore.AgpReports/static/js/app.js")
                .SetDependencies(
                    "OrganiMedCore.AgpReports.Manifest",
                    "OrganiMedCore.AgpReports.Vendor");
        }
    }
}
