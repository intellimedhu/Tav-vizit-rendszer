using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.Extension
{
    public static class CenterTypeCaptions
    {
        public static IDictionary<CenterType, string> GetLocalizedValues(IHtmlLocalizer t)
            => ((CenterType[])Enum.GetValues(typeof(CenterType)))
                .ToDictionary(
                    centerType => centerType,
                    centerType =>
                    {
                        switch (centerType)
                        {
                            case CenterType.Adult:
                                return t["Felnőtt szakellátóhely"].Value;

                            case CenterType.Child:
                                return t["Gyermek szakellátóhely"].Value;

                            case CenterType.Gestational:
                                return t["Gesztációs szakellátóhely"].Value;

                            case CenterType.ContinuousInsulinDelivery:
                                return t["Folyamatos inzulinadagoló szakellátóhely"].Value;

                            default:
                                return centerType.ToString();
                        }
                    }
                );
    }
}
