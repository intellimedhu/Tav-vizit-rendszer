using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class CenterProfileExtensionsTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData(CenterProfileStatus.Unsubmitted, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, true)]
        [InlineData(CenterProfileStatus.MDTAccepted, false)]
        public void Submitted_Part(CenterProfileStatus? status, bool expectedResult)
        {
            var part = new CenterProfileManagerExtensionsPart() { RenewalCenterProfileStatus = status };
            var result = part.Submitted();
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(CenterProfileStatus.Unsubmitted, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, true)]
        [InlineData(CenterProfileStatus.MDTAccepted, false)]
        public void Submitted_ViewModel(CenterProfileStatus? status, bool expectedResult)
        {
            var part = new CenterProfileRenewalViewModel() { RenewalCenterProfileStatus = status };
            var result = part.Submitted();
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(CenterProfileStatus.Unsubmitted, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, false)]
        [InlineData(CenterProfileStatus.MDTAccepted, false)]
        public void ApplicationEnabled_Part(CenterProfileStatus? status, bool expectedResult)
        {
            var part = new CenterProfileManagerExtensionsPart() { RenewalCenterProfileStatus = status };
            var result = part.ApplicationEnabled();
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(CenterProfileStatus.Unsubmitted, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, false)]
        [InlineData(CenterProfileStatus.MDTAccepted, false)]
        public void ApplicationEnabled_ViewModel(CenterProfileStatus? status, bool expectedResult)
        {
            var part = new CenterProfileRenewalViewModel() { RenewalCenterProfileStatus = status };
            var result = part.ApplicationEnabled();
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(CenterProfileStatus.Unsubmitted, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, true)]
        [InlineData(CenterProfileStatus.MDTAccepted, false)]
        public void IsInRenewalProcess_ViewModel(CenterProfileStatus? status, bool expectedResult)
        {
            var viewModel = new CenterProfileRenewalViewModel() { RenewalCenterProfileStatus = status };
            var result = viewModel.IsInRenewalProcess();
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(CenterProfileStatus.Unsubmitted, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, true)]
        [InlineData(CenterProfileStatus.MDTAccepted, false)]
        public void IsInRenewalProcess_ContentItem(CenterProfileStatus? status, bool expectedResult)
        {
            var contentItem = new ContentItem() { ContentType = ContentTypes.CenterProfile };
            contentItem.Alter<CenterProfileManagerExtensionsPart>(part => part.RenewalCenterProfileStatus = status);
            var result = contentItem.IsInRenewalProcess();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void IsInRenewalProcess_ContentItem_ShouldThrow_ArgumentException()
            => Assert.Throws<ArgumentException>(() => new ContentItem().IsInRenewalProcess());
    }
}
