using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.Models.Enums;
using System.Collections.Generic;

namespace OrganiMedCore.Organization.Helpers
{
    public static class CheckInHelper
    {
        public static IEnumerable<CheckInStatus> GetStatusModels(IViewLocalizer localizer) =>
            new CheckInStatus[]
            {
                new CheckInStatus
                {
                    Value = CheckInStatuses.Waiting,
                    Text = localizer["Várakozás"].Value
                },
                new CheckInStatus
                {
                    Value = CheckInStatuses.TreatmentInProgress,
                    Text = localizer["Kezelés folyamatban"].Value
                },
                new CheckInStatus
                {
                    Value = CheckInStatuses.TreatementFinished,
                    Text = localizer["Kezelés befejezve"].Value
                }
            };
    }
}
