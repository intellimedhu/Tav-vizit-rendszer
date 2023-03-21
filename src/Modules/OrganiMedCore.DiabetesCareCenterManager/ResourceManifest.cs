using OrchardCore.ResourceManagement;

namespace OrganiMedCore.DiabetesCareCenterManager
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineStyle("QualificationsPerOccupationEditors")
                .SetUrl("/OrganiMedCore.DiabetesCareCenterManager/styles/qpo.min.css", "/OrganiMedCore.DiabetesCareCenterManager/styles/qpo.css");

            manifest
                .DefineStyle("DataTableCenterProfiles")
                .SetDependencies("DataTables")
                .SetUrl(
                    "/OrganiMedCore.DiabetesCareCenterManager/styles/datatable-center-profiles.min.css",
                    "/OrganiMedCore.DiabetesCareCenterManager/styles/datatable-center-profiles.css");

            manifest
                .DefineScript("CenterUsers")
                .SetDependencies("jQuery")
                .SetUrl("/OrganiMedCore.DiabetesCareCenterManager/js/center-users.js");

            manifest
                .DefineScript("SettlementEditor")
                .SetDependencies("UIBootstrap")
                .SetUrl("/OrganiMedCore.DiabetesCareCenterManager/js/settlement-editor.js");

            manifest
                .DefineScript("ColleagueApplicationEditor")
                .SetUrl("/OrganiMedCore.DiabetesCareCenterManager/js/ca-editor.js");

            manifest
                .DefineScript("DiabetesCareCenterManager.MapView")
                .SetUrl("/OrganiMedCore.DiabetesCareCenterManager/js/map-view.js");

            manifest
                .DefineScript("DiabetesCareCenterManager.CenterProfilesList")
                .SetDependencies("jQuery", "DataTables", "DataTables.HungarianOrder", "Moment")
                .SetUrl("/OrganiMedCore.DiabetesCareCenterManager/js/center-profiles-list.js");
        }
    }
}
