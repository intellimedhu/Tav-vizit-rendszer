using Microsoft.AspNetCore.Mvc.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliMed.Core.Extensions
{
    public static class DayCaptions
    {
        public static IDictionary<DayOfWeek, string> GetLocalizedValues(IHtmlLocalizer t)
            => ((DayOfWeek[])Enum.GetValues(typeof(DayOfWeek)))
                .ToDictionary(
                    day => day,
                    day =>
                    {
                        switch (day)
                        {
                            case DayOfWeek.Monday:
                                return t["Hétfő"].Value;

                            case DayOfWeek.Tuesday:
                                return t["Kedd"].Value;

                            case DayOfWeek.Wednesday:
                                return t["Szerda"].Value;

                            case DayOfWeek.Thursday:
                                return t["Csütörtök"].Value;

                            case DayOfWeek.Friday:
                                return t["Péntek"].Value;

                            case DayOfWeek.Saturday:
                                return t["Szombat"].Value;

                            case DayOfWeek.Sunday:
                                return t["Vasárnap"].Value;

                            default:
                                return null;
                        }
                    }
                );
    }
}
