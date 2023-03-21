using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.Extension
{
    public static class AccreditationStatusCaptions
    {
        public static IDictionary<AccreditationStatus, string> GetLocalizedValues(IHtmlLocalizer t)
            => ((AccreditationStatus[])Enum.GetValues(typeof(AccreditationStatus)))
                .ToDictionary(
                    status => status,
                    status =>
                    {
                        switch (status)
                        {
                            case AccreditationStatus.Accredited:
                                return t["Akkreditált szakellátóhely"].Value;

                            case AccreditationStatus.TemporarilyAccredited:
                                return t["Ideiglenesen akkreditált szakellátóhely"].Value;

                            case AccreditationStatus.Registered:
                                return t["Nyilvántartott szakellátóhely"].Value;

                            case AccreditationStatus.New:
                                return t["Új szakellátóhely"].Value;

                            default:
                                return status.ToString();
                        }
                    }
                );
    }
}
