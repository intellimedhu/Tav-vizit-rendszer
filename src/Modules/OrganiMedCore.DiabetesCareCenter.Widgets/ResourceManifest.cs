using OrchardCore.ResourceManagement;

namespace OrganiMedCore.DiabetesCareCenter.Widgets
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("RenewalPeriodCounter")
                .SetDependencies("jQuery")
                .SetDependencies("Moment")
                .SetUrl("/OrganiMedCore.DiabetesCareCenter.Widgets/js/renewal-period-counter.js");
        }
    }
}
