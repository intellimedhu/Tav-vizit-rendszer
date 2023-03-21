using IntelliMed.DokiNetIntegration.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Services
{
    public interface IColleagueWorkplaceZoneService
    {
        JObject Default { get; }

        JObject GetZones(ContentItem[] contentItems, DokiNetMember dokiNetMember);
    }
}
