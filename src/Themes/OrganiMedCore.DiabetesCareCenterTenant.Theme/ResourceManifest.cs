using OrchardCore.ResourceManagement;
using System;

namespace OrganiMedCore.DiabetesCareCenterTenant.Theme
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            var version = $"?t={Guid.NewGuid().ToString("N")}";

            manifest
                .DefineStyle("DiabetesCareCenterTenant.Styles")
                .SetUrl(
                    $"/OrganiMedCore.DiabetesCareCenterTenant.Theme/styles/main.min.css{version}",
                    $"/OrganiMedCore.DiabetesCareCenterTenant.Theme/styles/main.css{version}");
        }
    }
}
