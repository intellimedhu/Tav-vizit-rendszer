using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.Extension
{
    public static class CenterProfileStatusCaptions
    {
        public static IDictionary<CenterProfileStatus, string> GetLocalizedEmptyValues(IHtmlLocalizer t)
            => ((CenterProfileStatus[])Enum.GetValues(typeof(CenterProfileStatus)))
                .ToDictionary(
                    status => status,
                    status =>
                    {
                        switch (status)
                        {
                            case CenterProfileStatus.Unsubmitted:
                                return t["Szakellátóhely vezető még nem kezdte el az ellenőrzést"].Value;

                            default:
                                return string.Empty;
                        }
                    }
                );

        public static IDictionary<CenterProfileStatus, string> GetLocalizedCurrentValues(IHtmlLocalizer t)
            => ((CenterProfileStatus[])Enum.GetValues(typeof(CenterProfileStatus)))
                .ToDictionary(
                    status => status,
                    status =>
                    {
                        switch (status)
                        {
                            case CenterProfileStatus.Unsubmitted:
                                return t["Szakellátóhely vezető ellenőrzése alatt"].Value;

                            case CenterProfileStatus.UnderReviewAtTR:
                                return t["Területi referens véleményezés alatt"].Value;

                            case CenterProfileStatus.UnderReviewAtOMKB:
                                return t["MDT-OMKB döntésre vár"].Value;

                            case CenterProfileStatus.UnderReviewAtMDT:
                                return t["MDT Vezetőségi határozatra vár"].Value;

                            default:
                                return string.Empty;
                        }
                    }
                );

        public static IDictionary<CenterProfileStatus, string> GetLocalizedFilledValues(IHtmlLocalizer t)
            => ((CenterProfileStatus[])Enum.GetValues(typeof(CenterProfileStatus)))
                .ToDictionary(
                    status => status,
                    status =>
                    {
                        switch (status)
                        {
                            case CenterProfileStatus.Unsubmitted:
                                return t["Szakellátóhely vezető a szakellátóhely adatlapot elküldte területi referensi véleményezésre"].Value;

                            case CenterProfileStatus.UnderReviewAtTR:
                                return t["Területi referens által jóváhagyva"].Value;

                            case CenterProfileStatus.UnderReviewAtOMKB:
                                return t["MDT-OMKB által jóváhagyva"].Value;

                            case CenterProfileStatus.UnderReviewAtMDT:
                                return t["MDT Vezetőség által jóváhagyva"].Value;

                            case CenterProfileStatus.MDTAccepted:
                                return t["MDT Vezetőség által jóváhagyva"].Value;

                            default:
                                return string.Empty;
                        }
                    }
                );
    }
}
