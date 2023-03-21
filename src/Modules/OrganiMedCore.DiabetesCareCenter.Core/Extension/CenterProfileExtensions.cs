using IntelliMed.Core.Extensions;
using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.Extensions
{
    public static class CenterProfileExtensions
    {
        public static bool Submitted(this CenterProfileManagerExtensionsPart part)
        {
            part.ThrowIfNull();

            return Submitted(part.RenewalCenterProfileStatus);
        }

        public static bool Submitted(this CenterProfileRenewalViewModel viewModel)
        {
            viewModel.ThrowIfNull();

            return Submitted(viewModel.RenewalCenterProfileStatus);
        }

        public static bool ApplicationEnabled(this CenterProfileManagerExtensionsPart part)
        {
            part.ThrowIfNull();

            return ApplicationEnabled(part.RenewalCenterProfileStatus);
        }

        public static bool ApplicationEnabled(this CenterProfileRenewalViewModel viewModel)
        {
            viewModel.ThrowIfNull();

            return ApplicationEnabled(viewModel.RenewalCenterProfileStatus);
        }

        public static bool IsInRenewalProcess(this CenterProfileRenewalViewModel viewModel)
            => IsInRenewalProcess(viewModel.RenewalCenterProfileStatus);

        public static bool IsInRenewalProcess(this ContentItem contentItem)
        {
            contentItem.ThrowIfNull();

            if (contentItem.ContentType != ContentTypes.CenterProfile)
            {
                throw new ArgumentException($"{ContentTypes.CenterProfile} exptected but got {contentItem.ContentType}.");
            }

            var part = contentItem.As<CenterProfileManagerExtensionsPart>();

            return IsInRenewalProcess(part.RenewalCenterProfileStatus);
        }

        public static CenterProfileReviewState GetCurrentReviewState(this CenterProfileReviewStatesPart part)
        {
            part.ThrowIfNull();

            return part.States.FirstOrDefault(x => x.Current);
        }


        private static bool Submitted(CenterProfileStatus? renewalCenterProfileStatus)
            => renewalCenterProfileStatus.HasValue &&
            renewalCenterProfileStatus != CenterProfileStatus.Unsubmitted &&
            renewalCenterProfileStatus != CenterProfileStatus.MDTAccepted;

        private static bool IsInRenewalProcess(CenterProfileStatus? centerProfileStatus)
            => centerProfileStatus.HasValue && centerProfileStatus != CenterProfileStatus.MDTAccepted;

        private static bool ApplicationEnabled(CenterProfileStatus? renewalCenterProfileStatus)
            => renewalCenterProfileStatus == CenterProfileStatus.Unsubmitted;
    }
}
