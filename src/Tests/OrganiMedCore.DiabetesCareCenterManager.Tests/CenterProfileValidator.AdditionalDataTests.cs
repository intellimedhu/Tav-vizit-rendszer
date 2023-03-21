using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Validators;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.Testing.Core.MockServices;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class CenterProfileValidatorAdditionalDataTests
    {
        [Fact]
        public void ValidateAdditionalData_ShouldBeValid()
        {
            var report = CreateAlterValidatePart(null);

            Assert.Empty(report);
        }

        [Theory]
        [InlineData(true, "1999-01-01", "valid-id", "valid-number", nameof(CenterProfileAdditionalDataViewModel.Antsz))]
        [InlineData(false, "", "valid-id", "valid-number", nameof(CenterProfileAntszDataViewModel.Date))]
        [InlineData(false, null, "valid-id", "valid-number", nameof(CenterProfileAntszDataViewModel.Date))]
        [InlineData(false, "1999-01-01", "", "valid-number", nameof(CenterProfileAntszDataViewModel.Id))]
        [InlineData(false, "1999-01-01", null, "valid-number", nameof(CenterProfileAntszDataViewModel.Id))]
        [InlineData(false, "1999-01-01", "valid-id", "", nameof(CenterProfileAntszDataViewModel.Number))]
        [InlineData(false, "1999-01-01", "valid-id", null, nameof(CenterProfileAntszDataViewModel.Number))]
        public void ValidateAdditionalData_ShouldReport_Antsz(bool isNull, string dateAsString, string id, string number, string expected)
        {
            Dictionary<string, string> report;
            if (isNull)
            {
                report = CreateAlterValidatePart(part => part.Antsz = null);
            }
            else
            {
                report = CreateAlterValidatePart(part =>
                {
                    part.Antsz.Date = DateTime.TryParse(dateAsString, out var date) ? date : default(DateTime?);
                    part.Antsz.Id = id;
                    part.Antsz.Number = number;
                });
            }

            Assert.Contains(report, x => x.Key == expected);
        }

        [Theory]
        [InlineData(0, "valid-wp", nameof(CenterProfileNeakData.NumberOfHours))]
        [InlineData(-1, "valid-wp", nameof(CenterProfileNeakData.NumberOfHours))]
        [InlineData(10, "", nameof(CenterProfileNeakData.WorkplaceCode))]
        [InlineData(10, null, nameof(CenterProfileNeakData.WorkplaceCode))]
        public void ValidateAdditionalData_ShouldReport_Neak(int numOfHours, string wpCode, string expected)
        {
            var report = CreateAlterValidatePart(part =>
            {
                var firstNeak = part.Neak.First();

                firstNeak.NumberOfHours = numOfHours;
                firstNeak.WorkplaceCode = wpCode;
            });

            Assert.Contains(report, x => x.Key == expected);
        }

        [Theory]
        [InlineData(false, false, null, nameof(CenterProfileAdditionalDataViewModel.VocationalClinic))]
        [InlineData(false, true, null, nameof(CenterProfileAdditionalDataViewModel.OtherVocationalClinic))]
        [InlineData(false, true, "", nameof(CenterProfileAdditionalDataViewModel.OtherVocationalClinic))]
        public void ValidateAdditionalData_ShouldReport_VocationalClinic(
            bool vocationalClinic,
            bool partOfOtherVocationalClinic,
            string otherVocationalClinic,
            string expected)
        {
            var report = CreateAlterValidatePart(part =>
            {
                part.VocationalClinic = vocationalClinic;
                part.PartOfOtherVocationalClinic = partOfOtherVocationalClinic;
                part.OtherVocationalClinic = otherVocationalClinic;
            });

            Assert.Contains(report, x => x.Key == expected);
        }

        [Fact]
        public void ValidateAdditionalData_ShouldReport_OfficeHours_InvalidTimePeriod()
        {
            var report = CreateAlterValidatePart(part =>
            {
                part.OfficeHours.First().Hours.First().TimeFrom = TimeSpan.FromHours(13);
            });

            Assert.Contains(report, x => x.Key == nameof(CenterProfileAdditionalDataViewModel.OfficeHours));
        }

        [Theory]
        [InlineData(10, 11, true)]
        [InlineData(10, 10, false)]
        [InlineData(10, 9, false)]
        public void ValidateAdditionalData_ShouldReport_NumOfHours_And_NumOfHoursDiabetes(int numOfHours, int numOfHoursDiabetes, bool shouldContainError)
        {
            var report = CreateAlterValidatePart(part =>
            {
                part.Neak = new[]
                {
                    new CenterProfileNeakData() { NumberOfHours = numOfHours, NumberOfHoursDiabetes = numOfHoursDiabetes }
                };
            });

            if (shouldContainError)
            {
                Assert.Contains(
                    report,
                    x => x.Key == nameof(CenterProfileNeakDataViewModel.NumberOfHours) + ">=" + nameof(CenterProfileNeakDataViewModel.NumberOfHoursDiabetes));
            }
            else
            {
                Assert.DoesNotContain(
                    report,
                    x => x.Key == nameof(CenterProfileNeakDataViewModel.NumberOfHours) + ">=" + nameof(CenterProfileNeakDataViewModel.NumberOfHoursDiabetes));
            }
        }

        [Fact]
        public void ValidateAdditionalData_ShouldReport_OfficeHours_SumHoursNotMeet()
        {
            var report = CreateAlterValidatePart(part =>
            {
                part.OfficeHours.First().Hours.First().TimeFrom = TimeSpan.FromHours(9);
            });

            Assert.Contains(report, x => x.Key == nameof(CenterProfileAdditionalDataViewModel.OfficeHours));
        }


        private Dictionary<string, string> CreateAlterValidatePart(Action<CenterProfilePart> alteration)
        {
            var part = GetValidCenterProfile();
            var validator = new CenterProfileValidator();
            var report = new Dictionary<string, string>();

            alteration?.Invoke(part);

            validator.ValidateAdditionalData(part, GetLocalizer(), report.Add);

            return report;
        }

        private CenterProfilePart GetValidCenterProfile()
            => new CenterProfilePart()
            {
                Antsz = new CenterProfileAntszData()
                {
                    Date = DateTime.Now,
                    Id = "id",
                    Number = "number"
                },
                Neak = new List<CenterProfileNeakData>()
                {
                    new CenterProfileNeakData()
                    {
                        Primary = true,
                        NumberOfHours = 11,
                        NumberOfHoursDiabetes = 10,
                        WorkplaceCode = "wpc"
                    }
                },
                OfficeHours = new[]
                {
                    new DailyOfficeHours()
                    {
                        Day = DayOfWeek.Monday,
                        Hours = new[]
                        {
                            new TimePeriod() { TimeFrom = TimeSpan.FromHours(10), TimeTo = TimeSpan.FromHours(13) } // 3
                        }
                    },
                    new DailyOfficeHours()
                    {
                        Day = DayOfWeek.Tuesday,
                        Hours = new[]
                        {
                            new TimePeriod() { TimeFrom = TimeSpan.FromHours(12), TimeTo = TimeSpan.FromHours(17) } // 5
                        }
                    },
                    new DailyOfficeHours()
                    {
                        Day = DayOfWeek.Friday,
                        Hours = new[]
                        {
                            new TimePeriod() { TimeFrom = TimeSpan.FromHours(18.5), TimeTo = TimeSpan.FromHours(20.5) } // 2
                        }
                    }
                },
                VocationalClinic = true
            };

        private LocalizerMock<CenterProfileValidator> GetLocalizer()
            => new LocalizerMock<CenterProfileValidator>();
    }
}
