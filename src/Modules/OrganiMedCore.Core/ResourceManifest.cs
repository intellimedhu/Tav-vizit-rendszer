using OrchardCore.ResourceManagement;

namespace OrganiMedCore.Core
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("Vue")
                .SetUrl("/OrganiMedCore.Core/Scripts/Vue/vue.min.js", "/OrganiMedCore.Core/Scripts/Vue/vue.js");

            manifest
                .DefineScript("Axios")
                .SetUrl("/OrganiMedCore.Core/Scripts/Axios/axios.min.js", "/OrganiMedCore.Core/Scripts/Axios/axios.js");

            manifest
                .DefineScript("Moment")
                .SetUrl("/OrganiMedCore.Core/Scripts/moment.min.js");

            manifest
                .DefineScript("DataTablesBase")
                .SetUrl("/OrganiMedCore.Core/Scripts/DataTables/jquery.dataTables.min.js", "/OrganiMedCore.Core/Scripts/DataTables/jquery.dataTables.js")
                .SetDependencies("jQuery");

            manifest
                .DefineScript("DataTables")
                .SetUrl("/OrganiMedCore.Core/Scripts/DataTables/dataTables.bootstrap4.min.js", "/OrganiMedCore.Core/Scripts/DataTables/dataTables.bootstrap4.js")
                .SetDependencies("DataTablesBase");

            manifest
                .DefineScript("DataTables.HungarianOrder")
                .SetUrl("/OrganiMedCore.Core/Scripts/DataTables/datatable-hunord.js");

            manifest
                .DefineStyle("DataTables")
                .SetUrl("/OrganiMedCore.Core/Styles/DataTables/dataTables.bootstrap4.min.css", "/OrganiMedCore.Core/Styles/DataTables/dataTables.bootstrap4.css");

            manifest
                .DefineScript("IdTypeValuePicker")
                .SetUrl("/OrganiMedCore.Core/Scripts/id-type-value-picker.js")
                .SetDependencies("Vue");

            manifest
                .DefineScript("DoctorPicker")
                .SetUrl("/OrganiMedCore.Core/Scripts/doctor-picker.js")
                .SetDependencies("Vue", "Axios");

            manifest
                .DefineScript("Commons")
                .SetUrl("/OrganiMedCore.Core/Scripts/commons.js")
                .SetDependencies("jQuery");
        }
    }
}
