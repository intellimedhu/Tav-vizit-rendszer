using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class CenterRenewalSettingsTests
    {
        [Theory]
        [InlineData("2018-02-25", true)]
        [InlineData("2018-03-10", false)]
        [InlineData("2018-03-21", true)]
        [InlineData("2018-10-10", true)]
        [InlineData("2019-07-01", false)]
        [InlineData("2019-07-17", true)]
        [InlineData("2019-07-26", true)]
        public void CenterRenewalSettings_CurrentRenewalPeriod_ShouldReturn_AsExpected(DateTime utcNow, bool shouldBeNull)
        {
            var settings = new CenterRenewalSettings()
            {
                RenewalPeriods = new[]
                {
                    new RenewalPeriod()
                    {
                        RenewalStartDate = DateTime.Parse("2018-03-01T02:00"),
                        ReviewStartDate = DateTime.Parse("2018-03-20T12:00"),
                        ReviewEndDate = DateTime.Parse("2018-04-01T00:00")
                    },
                    new RenewalPeriod()
                    {
                        RenewalStartDate = DateTime.Parse("2019-07-01T00:00"),
                        ReviewStartDate = DateTime.Parse("2019-07-16T00:00"),
                        ReviewEndDate = DateTime.Parse("2019-07-25T16:30")
                    }
                }
            };

            Assert.Equal(shouldBeNull, settings[utcNow] == null);
        }

        [Theory]
        [InlineData("1999-09-09", null)]
        [InlineData("2018-02-25", null)]
        [InlineData("2018-03-10", null)]

        [InlineData("2018-10-10", 0)]
        [InlineData("2019-07-01", 0)]
        [InlineData("2019-07-17", 0)]

        [InlineData("2019-07-26", 1)]
        [InlineData("2020-01-05", 1)]
        [InlineData("2020-02-15", 1)]

        [InlineData("2020-02-16", 2)]
        [InlineData("2035-12-06", 2)]
        public void CenterRenewalSettings_GetPreviousFullPeriod_ShouldReturn_AsExpected(DateTime utcNow, int? expectedPeriod)
        {
            // 0
            var firstRenewalStartDate = DateTime.Parse("2018-03-01T02:00");
            var firstReviewStartDate = DateTime.Parse("2018-03-20T12:00");
            var firstReviewEndDate = DateTime.Parse("2018-04-01T00:00");

            // 1
            var secondRenewalStartDate = DateTime.Parse("2019-07-01T00:00");
            var secondReviewStartDate = DateTime.Parse("2019-07-16T00:00");
            var secondReviewEndDate = DateTime.Parse("2019-07-25T16:30");

            // 2
            var thirdRenewalStartDate = DateTime.Parse("2020-01-01T00:00");
            var thirdReviewStartDate = DateTime.Parse("2020-01-25T00:00");
            var thirdReviewEndDate = DateTime.Parse("2020-02-15T23:59");

            var settings = new CenterRenewalSettings()
            {
                RenewalPeriods = new[]
                {
                    new RenewalPeriod()
                    {
                        RenewalStartDate = firstRenewalStartDate,
                        ReviewStartDate = firstReviewStartDate,
                        ReviewEndDate = firstReviewEndDate
                    },
                    new RenewalPeriod()
                    {
                        RenewalStartDate = secondRenewalStartDate,
                        ReviewStartDate = secondReviewStartDate,
                        ReviewEndDate = secondReviewEndDate
                    },
                    new RenewalPeriod()
                    {
                        RenewalStartDate = thirdRenewalStartDate,
                        ReviewStartDate = thirdReviewStartDate,
                        ReviewEndDate = thirdReviewEndDate
                    }
                }
            };

            var result = settings.GetPreviousFullPeriod(utcNow);

            if (!expectedPeriod.HasValue)
            {
                Assert.Null(result);
            }
            else if (expectedPeriod == 0)
            {
                Assert.Equal(firstRenewalStartDate, result.RenewalStartDate);
                Assert.Equal(firstReviewStartDate, result.ReviewStartDate);
                Assert.Equal(firstReviewEndDate, result.ReviewEndDate);
            }
            else if (expectedPeriod == 1)
            {
                Assert.Equal(secondRenewalStartDate, result.RenewalStartDate);
                Assert.Equal(secondReviewStartDate, result.ReviewStartDate);
                Assert.Equal(secondReviewEndDate, result.ReviewEndDate);
            }
            else if (expectedPeriod == 2)
            {
                Assert.Equal(thirdRenewalStartDate, result.RenewalStartDate);
                Assert.Equal(thirdReviewStartDate, result.ReviewStartDate);
                Assert.Equal(thirdReviewEndDate, result.ReviewEndDate);
            }
        }

        [Theory]
        [InlineData("1999-09-09", null)]
        [InlineData("2018-03-01T02:00", 0)]
        [InlineData("2018-03-01T03:00", 0)]
        [InlineData("2018-03-20T12:00", 0)]
        [InlineData("2018-03-31T23:59", 0)]
        [InlineData("2018-04-01T00:00", null)]

        [InlineData("2019-07-05", 1)]
        [InlineData("2019-07-20", 1)]
        [InlineData("2019-07-25T16:30", null)]
        [InlineData("2019-10-15", null)]

        [InlineData("2020-01-01", 2)]
        [InlineData("2020-01-14", 2)]
        [InlineData("2035-12-06", null)]
        public void CenterRenewalSettings_GetCurrentFullPeriod_ShouldReturn_AsExpected(DateTime utcNow, int? expectedPeriod)
        {
            // 0
            var firstRenewalStartDate = DateTime.Parse("2018-03-01T02:00");
            var firstReviewStartDate = DateTime.Parse("2018-03-20T12:00");
            var firstReviewEndDate = DateTime.Parse("2018-04-01T00:00");

            // 1
            var secondRenewalStartDate = DateTime.Parse("2019-07-01T00:00");
            var secondReviewStartDate = DateTime.Parse("2019-07-16T00:00");
            var secondReviewEndDate = DateTime.Parse("2019-07-25T16:30");

            // 2
            var thirdRenewalStartDate = DateTime.Parse("2020-01-01T00:00");
            var thirdReviewStartDate = DateTime.Parse("2020-01-25T00:00");
            var thirdReviewEndDate = DateTime.Parse("2020-02-15T23:59");

            var settings = new CenterRenewalSettings()
            {
                RenewalPeriods = new[]
                {
                    new RenewalPeriod()
                    {
                        RenewalStartDate = firstRenewalStartDate,
                        ReviewStartDate = firstReviewStartDate,
                        ReviewEndDate = firstReviewEndDate
                    },
                    new RenewalPeriod()
                    {
                        RenewalStartDate = secondRenewalStartDate,
                        ReviewStartDate = secondReviewStartDate,
                        ReviewEndDate = secondReviewEndDate
                    },
                    new RenewalPeriod()
                    {
                        RenewalStartDate = thirdRenewalStartDate,
                        ReviewStartDate = thirdReviewStartDate,
                        ReviewEndDate = thirdReviewEndDate
                    }
                }
            };

            var result = settings.GetCurrentFullPeriod(utcNow);

            if (!expectedPeriod.HasValue)
            {
                Assert.Null(result);
            }
            else if (expectedPeriod == 0)
            {
                Assert.Equal(firstRenewalStartDate, result.RenewalStartDate);
                Assert.Equal(firstReviewStartDate, result.ReviewStartDate);
                Assert.Equal(firstReviewEndDate, result.ReviewEndDate);
            }
            else if (expectedPeriod == 1)
            {
                Assert.Equal(secondRenewalStartDate, result.RenewalStartDate);
                Assert.Equal(secondReviewStartDate, result.ReviewStartDate);
                Assert.Equal(secondReviewEndDate, result.ReviewEndDate);
            }
            else if (expectedPeriod == 2)
            {
                Assert.Equal(thirdRenewalStartDate, result.RenewalStartDate);
                Assert.Equal(thirdReviewStartDate, result.ReviewStartDate);
                Assert.Equal(thirdReviewEndDate, result.ReviewEndDate);
            }
        }

        [Fact]
        public void CenterRenewalSettings_LatestFullPeriod()
        {
            // 0
            var firstRenewalStartDate = DateTime.Parse("2018-03-01T02:00");
            var firstReviewStartDate = DateTime.Parse("2018-03-20T12:00");
            var firstReviewEndDate = DateTime.Parse("2018-04-01T00:00");

            // 1
            var secondRenewalStartDate = DateTime.Parse("2019-07-01T00:00");
            var secondReviewStartDate = DateTime.Parse("2019-07-16T00:00");
            var secondReviewEndDate = DateTime.Parse("2019-07-25T16:30");

            // 2
            var thirdRenewalStartDate = DateTime.Parse("2020-01-01T00:00");
            var thirdReviewStartDate = DateTime.Parse("2020-01-25T00:00");
            var thirdReviewEndDate = DateTime.Parse("2020-02-15T23:59");

            var settings = new CenterRenewalSettings()
            {
                RenewalPeriods = new[]
                {
                    // 2
                    new RenewalPeriod()
                    {
                        RenewalStartDate = thirdRenewalStartDate,
                        ReviewStartDate = thirdReviewStartDate,
                        ReviewEndDate = thirdReviewEndDate
                    },

                    // 1
                    new RenewalPeriod()
                    {
                        RenewalStartDate = secondRenewalStartDate,
                        ReviewStartDate = secondReviewStartDate,
                        ReviewEndDate = secondReviewEndDate
                    },
                    
                    // 0
                    new RenewalPeriod()
                    {
                        RenewalStartDate = firstRenewalStartDate,
                        ReviewStartDate = firstReviewStartDate,
                        ReviewEndDate = firstReviewEndDate
                    },
                }
            };

            Assert.Equal(thirdRenewalStartDate, settings.LatestFullPeriod.RenewalStartDate);
            Assert.Equal(thirdReviewStartDate, settings.LatestFullPeriod.ReviewStartDate);
            Assert.Equal(thirdReviewEndDate, settings.LatestFullPeriod.ReviewEndDate);
        }

        [Theory]
        [InlineData("2018-03-01T01:59", null)]
        [InlineData("2018-03-01T02:00", 0)]
        [InlineData("2018-03-20T11:59", 0)]
        [InlineData("2018-03-20T12:00", null)]

        [InlineData("2019-06-30T23:59", null)]
        [InlineData("2019-07-01T00:00", 1)]
        [InlineData("2019-07-16T00:00", null)]
        [InlineData("2019-07-16T15:25", null)]
        public void CenterRenewalSettings_CurrentRenewalPeriod_Indexer(DateTime utcNow, int? expectedPeriod)
        {
            // 0
            var firstRenewalStartDate = DateTime.Parse("2018-03-01T02:00");
            var firstReviewStartDate = DateTime.Parse("2018-03-20T12:00");
            var firstReviewEndDate = DateTime.Parse("2018-04-01T00:00");

            // 1
            var secondRenewalStartDate = DateTime.Parse("2019-07-01T00:00");
            var secondReviewStartDate = DateTime.Parse("2019-07-16T00:00");
            var secondReviewEndDate = DateTime.Parse("2019-07-25T16:30");

            var settings = new CenterRenewalSettings()
            {
                RenewalPeriods = new[]
                {
                    // 0
                    new RenewalPeriod()
                    {
                        RenewalStartDate = firstRenewalStartDate,
                        ReviewStartDate = firstReviewStartDate,
                        ReviewEndDate = firstReviewEndDate
                    },
                    // 1
                    new RenewalPeriod()
                    {
                        RenewalStartDate = secondRenewalStartDate,
                        ReviewStartDate = secondReviewStartDate,
                        ReviewEndDate = secondReviewEndDate
                    }
                }
            };

            var result = settings[utcNow];

            if (!expectedPeriod.HasValue)
            {
                Assert.Null(result);
            }
            else if (expectedPeriod == 0)
            {
                Assert.Equal(firstRenewalStartDate, result.RenewalStartDate);
                Assert.Equal(firstReviewStartDate, result.ReviewStartDate);
                Assert.Equal(firstReviewEndDate, result.ReviewEndDate);
            }
            else if (expectedPeriod == 1)
            {
                Assert.Equal(secondRenewalStartDate, result.RenewalStartDate);
                Assert.Equal(secondReviewStartDate, result.ReviewStartDate);
                Assert.Equal(secondReviewEndDate, result.ReviewEndDate);
            }
        }
    }
}
