using OrchardCore.ResourceManagement;

namespace OrganiMedCore.DiabetesCareCenterTenant
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("ColleagueInvitation")
                .SetDependencies("jQuery", "jQuery-UI")
                .SetUrl("/OrganiMedCore.DiabetesCareCenterTenant/js/colleague-invitation.js");
        }
    }
}
