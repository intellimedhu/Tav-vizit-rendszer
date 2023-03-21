using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.Extensions
{
    public static class QualificationStateCaptions
    {
        public static IDictionary<QualificationState, string> GetLocalizedValues(IHtmlLocalizer t)
            => ((QualificationState[])Enum.GetValues(typeof(QualificationState)))
                .ToDictionary(
                    state => state,
                    state =>
                    {
                        switch (state)
                        {
                            case QualificationState.Obtained:
                                return t["Megszerezte"].Value;

                            case QualificationState.Student:
                                return t["Tanuló"].Value;

                            case QualificationState.Course:
                                return t["Tanfolyamra előjegyzett"].Value;

                            default:
                                return state.ToString();
                        }
                    }
                );
    }
}
