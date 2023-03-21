using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.Validators
{
    public interface ICenterProfileValidator
    {
        void ValidateBasicData(CenterProfilePart part, IHtmlLocalizer t, Action<string, string> reportError);

        void ValidateAdditionalData(CenterProfilePart part, IHtmlLocalizer t, Action<string, string> reportError);
    }
}
