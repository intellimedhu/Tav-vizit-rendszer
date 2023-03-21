using OrchardCore.ResourceManagement;

namespace OrganiMedCore.DiabetesCareCenterManager.CenterProfileEditor

{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("CenterProfileEditor")
                .SetUrl("/OrganiMedCore.DiabetesCareCenterManager.CenterProfileEditor/center-profile-editor.js");

            manifest
                .DefineStyle("CenterProfileEditor")
                .SetUrl(
                    "/OrganiMedCore.DiabetesCareCenterManager.CenterProfileEditor/center-profile-editor.min.css",
                    "/OrganiMedCore.DiabetesCareCenterManager.CenterProfileEditor/center-profile-editor.css");
        }
    }
}
