using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.Extension
{
    public static class CenterProfileEditorStepExtensions
    {
        public static IDictionary<CenterProfileEditorStep, string> GetLocalizedValues(IHtmlLocalizer t)
            => ((CenterProfileEditorStep[])Enum.GetValues(typeof(CenterProfileEditorStep)))
                .ToDictionary(
                    step => step,
                    step =>
                    {
                        switch (step)
                        {
                            case CenterProfileEditorStep.BasicData:
                                return t["Alapadatok"].Value;

                            case CenterProfileEditorStep.AdditionalData:
                                return t["Kiegészítő adatok"].Value;

                            case CenterProfileEditorStep.Equipments:
                                return t["Tárgyi eszközök"].Value;

                            case CenterProfileEditorStep.Colleagues:
                                return t["Munkatársak"].Value;

                            case CenterProfileEditorStep.Summary:
                                return t["Összegzés"].Value;

                            default:
                                return step.ToString();
                        }
                    }
                );
    }
}
