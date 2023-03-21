using IntelliMed.DokiNetIntegration.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Services
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class ColleagueWorkplaceZoneService : IColleagueWorkplaceZoneService
    {
        public JObject Default => new JObject()
        {
            [nameof(ColleagueStatusExtensions.GreenZone)] = false,
            [nameof(ColleagueStatusExtensions.PendingZone)] = false,
            [nameof(ColleagueStatusExtensions.RemovedZone)] = false
        };

        public JObject GetZones(ContentItem[] contentItems, DokiNetMember dokiNetMember)
        {
            var result = Default;

            var greenZone = ColleagueStatusExtensions.GreenZone;
            var pendingZone = ColleagueStatusExtensions.PendingZone;
            var removedZone = ColleagueStatusExtensions.RemovedZone;

            var i = 0;
            var hasGreenZone = false;
            var hasPendingZone = false;
            var hasRemovedZone = false;
            while (i < contentItems.Length && (!hasGreenZone || !hasPendingZone || !hasRemovedZone))
            {
                var colleague = contentItems[i].As<CenterProfilePart>().Colleagues.FirstOrDefault(x => x.MemberRightId == dokiNetMember.MemberRightId);
                if (colleague != null)
                {
                    if (!hasGreenZone && greenZone.Contains(colleague.LatestStatusItem.Status))
                    {
                        hasGreenZone = true;
                    }

                    if (!hasPendingZone && pendingZone.Contains(colleague.LatestStatusItem.Status))
                    {
                        hasPendingZone = true;
                    }

                    if (!hasRemovedZone && removedZone.Contains(colleague.LatestStatusItem.Status))
                    {
                        hasRemovedZone = true;
                    }
                }

                i++;
            }

            result[nameof(ColleagueStatusExtensions.GreenZone)] = hasGreenZone;
            result[nameof(ColleagueStatusExtensions.PendingZone)] = hasPendingZone;
            result[nameof(ColleagueStatusExtensions.RemovedZone)] = hasRemovedZone;

            return result;
        }
    }
}
