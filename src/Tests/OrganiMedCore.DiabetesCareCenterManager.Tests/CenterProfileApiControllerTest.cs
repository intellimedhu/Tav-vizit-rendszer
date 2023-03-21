using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class CenterProfileApiControllerTest
    {
        [Fact]
        public async Task ValidateAdditionalData_ShouldReturnVocationalClinicsError()
        {
            var validationResult = await CenterProfileHelpers.ValidateAdditionalDataAsync(
                new CenterProfileAdditionalDataViewModel()
                {
                    VocationalClinic = false,
                    PartOfOtherVocationalClinic = false
                });

            Assert.Contains(validationResult, x => x.Value.Contains("Kérjük, adja meg a szakellátóhely típusát"));
        }

        [Fact]
        public async Task ValidateAdditionalData_ShouldNotReturnVocationalClinicsError()
        {
            var validationResult = await CenterProfileHelpers.ValidateAdditionalDataAsync(
                new CenterProfileAdditionalDataViewModel()
                {
                    VocationalClinic = true,
                    PartOfOtherVocationalClinic = false
                });

            Assert.DoesNotContain(validationResult, x => x.Value.Contains("Kérjük, adja meg a szakellátóhely típusát"));
        }

        [Fact]
        public async Task ValidateAdditionalData_ShouldReturnOtherVocationalClinic()
        {
            var validationResult = await CenterProfileHelpers.ValidateAdditionalDataAsync(
                new CenterProfileAdditionalDataViewModel()
                {
                    VocationalClinic = false,
                    PartOfOtherVocationalClinic = true
                });

            Assert.Contains(validationResult, x => x.Value.Contains("Kérjük, adja meg, hogy a szakellátóhely mely szakrendelés része"));
        }

        [Fact]
        public async Task ValidateAdditionalData_ShouldNotReturnOtherVocationalClinic()
        {
            var validationResult = await CenterProfileHelpers.ValidateAdditionalDataAsync(
                new CenterProfileAdditionalDataViewModel()
                {
                    VocationalClinic = false,
                    PartOfOtherVocationalClinic = true,
                    OtherVocationalClinic = "sample"
                });

            Assert.DoesNotContain(validationResult, x => x.Value.Contains("Kérjük, adja meg, hogy a szakellátóhely mely szakrendelés része"));
        }

        [Fact]
        public async Task ValidateAdditionalData_ShouldReturnOfficeAndNeakHoursNotEqualError()
        {
            var validationResult = await CenterProfileHelpers.ValidateAdditionalDataAsync(
                new CenterProfileAdditionalDataViewModel()
                {
                    Neak = new List<CenterProfileNeakDataViewModel>()
                    {
                        new CenterProfileNeakDataViewModel() { NumberOfHoursDiabetes = 10 }
                    },
                    OfficeHours = new List<DailyOfficeHours>()
                    {
                        new DailyOfficeHours()
                        {
                            Day = DayOfWeek.Monday,
                            Hours = new List<TimePeriod>()
                            {
                                new TimePeriod() { TimeFrom = TimeSpan.FromHours(8), TimeTo = TimeSpan.FromHours(16) } // 8
                            }
                        }
                    }
                });

            Assert.Contains(validationResult.Values, x => x.Contains("A cukorbetegekre fordított óraszámok összege nem egyezik a rendelési idők összegével"));
        }

        [Theory]
        [InlineData(5, 4, false)]
        [InlineData(5, 5, false)]
        [InlineData(5, 6, true)]
        public async Task ValidateAdditionalData_ShouldReturnNeakHoursDiabetesHoursError(int numberOfHours, int numberOfHoursDiabetes, bool shouldContainError)
        {
            var validationResult = await CenterProfileHelpers.ValidateAdditionalDataAsync(
                new CenterProfileAdditionalDataViewModel()
                {
                    Neak = new List<CenterProfileNeakDataViewModel>()
                    {
                        new CenterProfileNeakDataViewModel() { NumberOfHours = numberOfHours, NumberOfHoursDiabetes = numberOfHoursDiabetes }
                    }
                });

            if (shouldContainError)
            {
                Assert.Contains(
                    validationResult.Values,
                    x => x.Contains("A cukorbetegekre fordított óraszám nem lehet több egy NEAK adat esetén sem, mint a szerződésben foglalt heti óraszám"));
            }
            else
            {
                Assert.DoesNotContain(
                    validationResult.Values,
                    x => x.Contains("A cukorbetegekre fordított óraszám nem lehet több egy NEAK adat esetén sem, mint a szerződésben foglalt heti óraszám"));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ValidateAdditionalData_NoNeakData_ShouldNotReturnNeakError(bool neakIsNull)
        {
            var validationResult = await CenterProfileHelpers.ValidateAdditionalDataAsync(
                new CenterProfileAdditionalDataViewModel()
                {
                    Neak = neakIsNull
                        ? null
                        : new CenterProfileNeakDataViewModel[0],
                    OfficeHours = new DailyOfficeHours[]
                    {
                        new DailyOfficeHours()
                        {
                            Day = DayOfWeek.Wednesday,
                            Hours = new TimePeriod[]
                            {
                                new TimePeriod()
                                {
                                    TimeFrom = TimeSpan.FromHours(8),
                                    TimeTo = TimeSpan.FromHours(9)
                                }
                            }
                        }
                    }
                });

            Assert.DoesNotContain(validationResult.Values, x => x.Any(y => y == "A cukorbetegekre fordított óraszámok összege nem egyezik a rendelési idők összegével"));
            Assert.DoesNotContain(validationResult.Values, x => x.Any(y => y == "A cukorbetegekre fordított óraszám nem lehet több egy NEAK adat esetén sem, mint a szerződésben foglalt heti óraszám"));
        }

        [Fact]
        public async Task ValidateAdditionalData_ShouldNotReturnOfficeAndNeakHoursNotEqualError()
        {
            var validationResult = await CenterProfileHelpers.ValidateAdditionalDataAsync(
                new CenterProfileAdditionalDataViewModel()
                {
                    Neak = new List<CenterProfileNeakDataViewModel>()
                    {
                        new CenterProfileNeakDataViewModel() { NumberOfHours = 12 }
                    },
                    OfficeHours = new List<DailyOfficeHours>()
                    {
                        new DailyOfficeHours()
                        {
                            Day = DayOfWeek.Monday,
                            Hours = new List<TimePeriod>()
                            {
                                new TimePeriod() { TimeFrom = TimeSpan.FromHours(8), TimeTo = TimeSpan.FromHours(16) } // 8
                            }
                        },
                        new DailyOfficeHours()
                        {
                            Day = DayOfWeek.Thursday,
                            Hours = new List<TimePeriod>()
                            {
                                new TimePeriod() { TimeFrom = TimeSpan.FromHours(10), TimeTo = TimeSpan.FromHours(14) } // 4
                            }
                        }
                    }
                });

            Assert.DoesNotContain(validationResult.Values, x => x.Contains("A szerződésben foglalt heti óraszámok összege nem egyezik a rendelsi idők összegével"));
        }

        [Fact]
        public async Task ValidateAdditionalData_ShouldNotReturnAnyErrors1()
        {
            var validationResult = await CenterProfileHelpers.ValidateAdditionalDataAsync(
                new CenterProfileAdditionalDataViewModel()
                {
                    Antsz = new CenterProfileAntszDataViewModel()
                    {
                        Id = "1",
                        Date = DateTime.Parse("2015-03-15"),
                        Number = "123456"
                    },
                    VocationalClinic = true,
                    PartOfOtherVocationalClinic = false,
                    OtherVocationalClinic = null,
                    Neak = new List<CenterProfileNeakDataViewModel>()
                    {
                        new CenterProfileNeakDataViewModel() { NumberOfHours = 6, NumberOfHoursDiabetes = 5 }
                    },
                    OfficeHours = new List<DailyOfficeHours>()
                    {
                        new DailyOfficeHours()
                        {
                            Day = DayOfWeek.Monday,
                            Hours = new List<TimePeriod>()
                            {
                                new TimePeriod() { TimeFrom = TimeSpan.FromHours(8), TimeTo = TimeSpan.FromHours(10) } // 2
                            }
                        },
                        new DailyOfficeHours()
                        {
                            Day = DayOfWeek.Thursday,
                            Hours = new List<TimePeriod>()
                            {
                                new TimePeriod() { TimeFrom = TimeSpan.FromHours(8), TimeTo = TimeSpan.FromHours(11) } // 3
                            }
                        }
                    }
                });

            Assert.Empty(validationResult);
        }

        [Fact]
        public async Task ValidateAdditionalData_ShouldNotReturnAnyErrors2()
        {
            var validationResult = await CenterProfileHelpers.ValidateAdditionalDataAsync(
                new CenterProfileAdditionalDataViewModel()
                {
                    Antsz = new CenterProfileAntszDataViewModel()
                    {
                        Id = "1",
                        Date = DateTime.Parse("2015-03-15"),
                        Number = "123456"
                    },
                    VocationalClinic = false,
                    PartOfOtherVocationalClinic = true,
                    OtherVocationalClinic = "sample",
                    Neak = new List<CenterProfileNeakDataViewModel>()
                    {
                        new CenterProfileNeakDataViewModel() { NumberOfHours = 10, NumberOfHoursDiabetes = 2 }
                    },
                    OfficeHours = new List<DailyOfficeHours>()
                    {
                        new DailyOfficeHours()
                        {
                            Day = DayOfWeek.Monday,
                            Hours = new List<TimePeriod>()
                            {
                                new TimePeriod() { TimeFrom = TimeSpan.FromHours(8.4), TimeTo = TimeSpan.FromHours(10.4) } // 2
                            }
                        }
                    }
                });

            Assert.Empty(validationResult);
        }
    }
}
