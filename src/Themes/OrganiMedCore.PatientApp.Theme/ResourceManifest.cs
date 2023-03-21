using OrchardCore.ResourceManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrganiMedCore.PatientApp.Theme
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineStyle("OrganiMedCore.PatientApp.Styles")
                .SetUrl(
                    "/OrganiMedCore.PatientApp.Theme/css/site.min.css",
                    "/OrganiMedCore.PatientApp.Theme/css/site.css");
        }
    }
}
