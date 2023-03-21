using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Validators;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.Testing.Core.MockServices;
using System;
using System.Collections.Generic;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class CenterProfileValidatorBasicDataTests
    {
        [Fact]
        public void ValidateBasicData_ShouldBeValid()
        {
            var report = CreateAlterValidatePart(null);

            Assert.Empty(report);
        }

        [Fact]
        public void ValidateBasicData_ShouldReport_CenterName()
        {
            var report1 = CreateAlterValidatePart(part => part.CenterName = null);
            var report2 = CreateAlterValidatePart(part => part.CenterName = "");

            Assert.Contains(report1, x => x.Key == nameof(CenterProfileBasicDataViewModel.CenterName));
            Assert.Contains(report2, x => x.Key == nameof(CenterProfileBasicDataViewModel.CenterName));
        }

        [Fact]
        public void ValidateBasicData_ShouldReport_CenterSettlementName()
        {
            var report1 = CreateAlterValidatePart(part => part.CenterSettlementName = null);
            var report2 = CreateAlterValidatePart(part => part.CenterSettlementName = "");

            Assert.Contains(report1, x => x.Key == nameof(CenterProfileBasicDataViewModel.CenterSettlementName));
            Assert.Contains(report2, x => x.Key == nameof(CenterProfileBasicDataViewModel.CenterSettlementName));
        }

        [Fact]
        public void ValidateBasicData_ShouldReport_CenterAddress()
        {
            var report1 = CreateAlterValidatePart(part => part.CenterAddress = null);
            var report2 = CreateAlterValidatePart(part => part.CenterAddress = "");

            Assert.Contains(report1, x => x.Key == nameof(CenterProfileBasicDataViewModel.CenterAddress));
            Assert.Contains(report2, x => x.Key == nameof(CenterProfileBasicDataViewModel.CenterAddress));
        }

        [Theory]
        // No adult, no child
        [InlineData(nameof(CenterProfileBasicDataViewModel.CenterTypes), CenterType.ContinuousInsulinDelivery)]
        [InlineData(nameof(CenterProfileBasicDataViewModel.CenterTypes), CenterType.Gestational)]
        [InlineData(nameof(CenterProfileBasicDataViewModel.CenterTypes), CenterType.ContinuousInsulinDelivery, CenterType.Gestational)]
        // Adult ok
        [InlineData(null, CenterType.Adult)]
        [InlineData(null, CenterType.Adult, CenterType.ContinuousInsulinDelivery)]
        [InlineData(null, CenterType.Adult, CenterType.Gestational)]
        [InlineData(null, CenterType.Adult, CenterType.Gestational, CenterType.ContinuousInsulinDelivery)]
        // Child ok
        [InlineData(null, CenterType.Child)]
        [InlineData(null, CenterType.Child, CenterType.ContinuousInsulinDelivery)]
        // Adult and child at the same time
        [InlineData(nameof(CenterType.Adult) + "&" + nameof(CenterType.Child), CenterType.Adult, CenterType.Child)]
        [InlineData(nameof(CenterType.Adult) + "&" + nameof(CenterType.Child), CenterType.Adult, CenterType.Child, CenterType.ContinuousInsulinDelivery)]
        [InlineData(nameof(CenterType.Adult) + "&" + nameof(CenterType.Child), CenterType.Adult, CenterType.Child, CenterType.Gestational)]
        [InlineData(nameof(CenterType.Adult) + "&" + nameof(CenterType.Child), CenterType.Adult, CenterType.Child, CenterType.Gestational, CenterType.ContinuousInsulinDelivery)]
        // Child errors
        [InlineData(nameof(CenterType.Child) + "&" + nameof(CenterType.Gestational), CenterType.Child, CenterType.Gestational)]
        [InlineData(nameof(CenterType.Child) + "&" + nameof(CenterType.Gestational), CenterType.Child, CenterType.Gestational, CenterType.ContinuousInsulinDelivery)]

        [InlineData(nameof(CenterProfileBasicDataViewModel.CenterTypes))]
        public void ValidateBasicData_ShouldReport_CenterTypes(string errorType, params CenterType[] combination)
        {
            var report = CreateAlterValidatePart(part => part.CenterTypes = new HashSet<CenterType>(combination));

            if (errorType == null)
            {
                Assert.Empty(report);
            }
            else
            {
                Assert.Contains(report, x => x.Key == errorType);
            }
        }

        [Fact]
        public void ValidateBasicData_ShouldReport_CenterZipCode()
        {
            var report1 = CreateAlterValidatePart(part => part.CenterZipCode = 0);
            var report2 = CreateAlterValidatePart(part => part.CenterZipCode = 500);
            var report3 = CreateAlterValidatePart(part => part.CenterZipCode = 15000);

            Assert.Contains(report1, x => x.Key == nameof(CenterProfileBasicDataViewModel.CenterZipCode));
            Assert.Contains(report2, x => x.Key == nameof(CenterProfileBasicDataViewModel.CenterZipCode));
            Assert.Contains(report3, x => x.Key == nameof(CenterProfileBasicDataViewModel.CenterZipCode));
        }

        [Fact]
        public void ValidateBasicData_ShouldReport_Email()
        {
            var report1 = CreateAlterValidatePart(part => part.Email = null);
            var report2 = CreateAlterValidatePart(part => part.Email = "invalid");

            Assert.Contains(report1, x => x.Key == nameof(CenterProfileBasicDataViewModel.Email));
            Assert.Contains(report2, x => x.Key == nameof(CenterProfileBasicDataViewModel.Email));
        }

        [Fact]
        public void ValidateBasicData_ShouldReport_Fax()
        {
            var report1 = CreateAlterValidatePart(part => part.Fax = "ab");
            var report2 = CreateAlterValidatePart(part => part.Fax = "");
            var report3 = CreateAlterValidatePart(part => part.Fax = null);

            Assert.Contains(report1, x => x.Key == nameof(CenterProfileBasicDataViewModel.Fax));
            Assert.DoesNotContain(report2, x => x.Key == nameof(CenterProfileBasicDataViewModel.Fax));
            Assert.DoesNotContain(report3, x => x.Key == nameof(CenterProfileBasicDataViewModel.Fax));
        }

        [Fact]
        public void ValidateBasicData_ShouldReport_Phone()
        {
            var report1 = CreateAlterValidatePart(part => part.Phone = "123");
            var report2 = CreateAlterValidatePart(part => part.Phone = "");
            var report3 = CreateAlterValidatePart(part => part.Phone = null);

            Assert.Contains(report1, x => x.Key == nameof(CenterProfileBasicDataViewModel.Phone));
            Assert.Contains(report2, x => x.Key == nameof(CenterProfileBasicDataViewModel.Phone));
            Assert.Contains(report3, x => x.Key == nameof(CenterProfileBasicDataViewModel.Phone));
        }


        private Dictionary<string, string> CreateAlterValidatePart(Action<CenterProfilePart> alteration)
        {
            var part = GetValidCenterProfile();
            var validator = new CenterProfileValidator();
            var report = new Dictionary<string, string>();

            alteration?.Invoke(part);

            validator.ValidateBasicData(part, GetLocalizer(), report.Add);

            return report;
        }

        private CenterProfilePart GetValidCenterProfile()
            => new CenterProfilePart()
            {
                CenterAddress = "ca",
                CenterName = "cn",
                CenterSettlementName = "csn",
                CenterTypes = new HashSet<CenterType>() { CenterType.Adult },
                CenterZipCode = 1234,
                Email = "a@b.c",
                Fax = "123456789",
                Phone = "123456789"
            };

        private LocalizerMock<CenterProfileValidator> GetLocalizer()
            => new LocalizerMock<CenterProfileValidator>();
    }
}
