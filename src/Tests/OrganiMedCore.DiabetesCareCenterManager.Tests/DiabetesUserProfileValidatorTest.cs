using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Validators;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.Testing.Core.MockServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class DiabetesUserProfileValidatorTest
    {
        public Dictionary<string, string> Reports { get; set; }

        [Fact]
        public async Task ValidateAsync_ShouldReport_QualificationIds()
        {
            var validator = new DiabetesUserProfileValidator();
            var siteService = new SiteServiceMock();
            var qualificationService = new QualificationService(siteService);
            Reports = new Dictionary<string, string>();

            await qualificationService.UpdateQualificationAsync(new QualificationViewModel()
            {
                Name = "A"
            });
            await qualificationService.UpdateQualificationAsync(new QualificationViewModel()
            {
                Name = "B"
            });

            var settings = await qualificationService.GetQualificationSettingsAsync();

            var id1 = settings.Qualifications.First().Id;
            var id2 = settings.Qualifications.Skip(1).First().Id;

            await validator.ValidateAsync(
                qualificationService,
                new LocalizerMock<DiabetesUserProfileValidator>(),
                new DiabetesUserProfilePartViewModel()
                {
                    Qualifications = new List<PersonQualificationViewModel>()
                    {
                        new PersonQualificationViewModel() { QualificationId = id1 },
                        new PersonQualificationViewModel() { QualificationId = id1 },
                        new PersonQualificationViewModel() { QualificationId = id2 }
                    }
                },
                DateTime.Now.Year,
                ReportError);

            Assert.True(Reports.ContainsKey("QualificationIds"));
        }

        [Fact]
        public async Task ValidateAsync_ShouldReport_QualificationId()
        {
            var validator = new DiabetesUserProfileValidator();
            var qualificationService = new QualificationService(new SiteServiceMock());
            Reports = new Dictionary<string, string>();

            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });

            var settings = await qualificationService.GetQualificationSettingsAsync();

            var id1 = settings.Qualifications.First().Id;

            await validator.ValidateAsync(
                qualificationService,
                new LocalizerMock<DiabetesUserProfileValidator>(),
                new DiabetesUserProfilePartViewModel()
                {
                    Qualifications = new List<PersonQualificationViewModel>()
                    {
                        new PersonQualificationViewModel() { QualificationId = id1 },
                        new PersonQualificationViewModel() { QualificationId = null }
                    }
                },
                DateTime.Now.Year,
                ReportError);

            Assert.False(Reports.ContainsKey("QualificationIds"));
            Assert.True(Reports.ContainsKey("QualificationId"));
        }

        [Fact]
        public async Task ValidateAsync_ShouldReport_QualificationNumber()
        {
            var validator = new DiabetesUserProfileValidator();
            var qualificationService = new QualificationService(new SiteServiceMock());
            Reports = new Dictionary<string, string>();

            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });

            var settings = await qualificationService.GetQualificationSettingsAsync();

            var id1 = settings.Qualifications.First().Id;

            await validator.ValidateAsync(
                qualificationService,
                new LocalizerMock<DiabetesUserProfileValidator>(),
                new DiabetesUserProfilePartViewModel()
                {
                    Qualifications = new List<PersonQualificationViewModel>()
                    {
                        new PersonQualificationViewModel() { QualificationId = id1 }
                    }
                },
                DateTime.Now.Year,
                ReportError);

            Assert.False(Reports.ContainsKey("QualificationIds"));
            Assert.False(Reports.ContainsKey("QualificationId"));
            Assert.True(Reports.ContainsKey("QualificationNumber"));
        }

        [Fact]
        public async Task ValidateAsync_ShouldReport_State()
        {
            var validator = new DiabetesUserProfileValidator();
            var qualificationService = new QualificationService(new SiteServiceMock());
            Reports = new Dictionary<string, string>();

            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });

            var settings = await qualificationService.GetQualificationSettingsAsync();

            var id1 = settings.Qualifications.First().Id;

            await validator.ValidateAsync(
                qualificationService,
                new LocalizerMock<DiabetesUserProfileValidator>(),
                new DiabetesUserProfilePartViewModel()
                {
                    Qualifications = new List<PersonQualificationViewModel>()
                    {
                        new PersonQualificationViewModel()
                        {
                            QualificationId = id1,
                            QualificationNumber = "ABC123"
                        }
                    }
                },
                DateTime.Now.Year,
                ReportError);

            Assert.False(Reports.ContainsKey("QualificationIds"));
            Assert.False(Reports.ContainsKey("QualificationId"));
            Assert.False(Reports.ContainsKey("QualificationNumber"));
            Assert.True(Reports.ContainsKey("State"));
        }

        [Fact]
        public async Task ValidateAsync_NoYear_ShouldReport_QualificationYear()
        {
            var validator = new DiabetesUserProfileValidator();
            var qualificationService = new QualificationService(new SiteServiceMock());
            Reports = new Dictionary<string, string>();

            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });

            var settings = await qualificationService.GetQualificationSettingsAsync();

            var id1 = settings.Qualifications.First().Id;

            await validator.ValidateAsync(
                qualificationService,
                new LocalizerMock<DiabetesUserProfileValidator>(),
                new DiabetesUserProfilePartViewModel()
                {
                    Qualifications = new List<PersonQualificationViewModel>()
                    {
                        new PersonQualificationViewModel()
                        {
                            QualificationId = id1,
                            QualificationNumber = "ABC123",
                            QualificationYear = null,
                            State = QualificationState.Course
                        }
                    }
                },
                DateTime.Now.Year,
                ReportError);

            Assert.False(Reports.ContainsKey("QualificationIds"));
            Assert.False(Reports.ContainsKey("QualificationId"));
            Assert.False(Reports.ContainsKey("QualificationNumber"));
            Assert.False(Reports.ContainsKey("State"));
            Assert.True(Reports.ContainsKey("QualificationYear"));
        }

        [Theory]
        [InlineData(1888, 2005)]
        [InlineData(2006, 2005)]
        public async Task ValidateAsync_WrongYear_ShouldReport_QualificationYear(int qualificationYear, int currentYear)
        {
            var validator = new DiabetesUserProfileValidator();
            var qualificationService = new QualificationService(new SiteServiceMock());
            Reports = new Dictionary<string, string>();

            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });

            var settings = await qualificationService.GetQualificationSettingsAsync();

            var id1 = settings.Qualifications.First().Id;

            await validator.ValidateAsync(
                qualificationService,
                new LocalizerMock<DiabetesUserProfileValidator>(),
                new DiabetesUserProfilePartViewModel()
                {
                    Qualifications = new List<PersonQualificationViewModel>()
                    {
                        new PersonQualificationViewModel()
                        {
                            QualificationId = id1,
                            QualificationNumber = "ABC123",
                            QualificationYear = qualificationYear,
                            State = QualificationState.Course
                        }
                    }
                },
                currentYear,
                ReportError);

            Assert.False(Reports.ContainsKey("QualificationIds"));
            Assert.False(Reports.ContainsKey("QualificationId"));
            Assert.False(Reports.ContainsKey("QualificationNumber"));
            Assert.False(Reports.ContainsKey("State"));
            Assert.True(Reports.ContainsKey("QualificationYear"));
        }

        [Fact]
        public async Task ValidateAsync_ShouldReport_GraduationYear1()
        {
            var validator = new DiabetesUserProfileValidator();
            var qualificationService = new QualificationService(new SiteServiceMock());
            Reports = new Dictionary<string, string>();

            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });

            var settings = await qualificationService.GetQualificationSettingsAsync();

            var id1 = settings.Qualifications.First().Id;

            await validator.ValidateAsync(
                qualificationService,
                new LocalizerMock<DiabetesUserProfileValidator>(),
                new DiabetesUserProfilePartViewModel()
                {
                    Qualifications = new List<PersonQualificationViewModel>()
                    {
                        new PersonQualificationViewModel()
                        {
                            QualificationId = id1,
                            QualificationNumber = "ABC123",
                            QualificationYear = 1969,
                            State = QualificationState.Course
                        }
                    },
                    GraduationIssuedBy = "CBA321"
                },
                2005,
                ReportError);

            Assert.False(Reports.ContainsKey("QualificationIds"));
            Assert.False(Reports.ContainsKey("QualificationId"));
            Assert.False(Reports.ContainsKey("QualificationNumber"));
            Assert.False(Reports.ContainsKey("State"));
            Assert.False(Reports.ContainsKey("QualificationYear"));
            Assert.True(Reports.ContainsKey("GraduationYear1"));
            Assert.False(Reports.ContainsKey("GraduationIssuedBy"));
        }

        [Fact]
        public async Task ValidateAsync_ShouldReport_GraduationIssuedBy()
        {
            var validator = new DiabetesUserProfileValidator();
            var qualificationService = new QualificationService(new SiteServiceMock());
            Reports = new Dictionary<string, string>();

            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });

            var settings = await qualificationService.GetQualificationSettingsAsync();

            var id1 = settings.Qualifications.First().Id;

            await validator.ValidateAsync(
                qualificationService,
                new LocalizerMock<DiabetesUserProfileValidator>(),
                new DiabetesUserProfilePartViewModel()
                {
                    Qualifications = new List<PersonQualificationViewModel>()
                    {
                        new PersonQualificationViewModel()
                        {
                            QualificationId = id1,
                            QualificationNumber = "ABC123",
                            QualificationYear = 1969,
                            State = QualificationState.Course
                        }
                    },
                    GraduationYear = 2000
                },
                2005,
                ReportError);

            Assert.False(Reports.ContainsKey("QualificationIds"));
            Assert.False(Reports.ContainsKey("QualificationId"));
            Assert.False(Reports.ContainsKey("QualificationNumber"));
            Assert.False(Reports.ContainsKey("State"));
            Assert.False(Reports.ContainsKey("QualificationYear"));
            Assert.False(Reports.ContainsKey("GraduationYear1"));
            Assert.True(Reports.ContainsKey("GraduationIssuedBy"));
        }

        [Theory]
        [InlineData(1058, 2010)]
        [InlineData(2001, 1999)]
        public async Task ValidateAsync_ShouldReport_GraduationYear2(int graduationYear, int currentYear)
        {
            var validator = new DiabetesUserProfileValidator();
            var qualificationService = new QualificationService(new SiteServiceMock());
            Reports = new Dictionary<string, string>();

            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });

            var settings = await qualificationService.GetQualificationSettingsAsync();

            var id1 = settings.Qualifications.First().Id;

            await validator.ValidateAsync(
                qualificationService,
                new LocalizerMock<DiabetesUserProfileValidator>(),
                new DiabetesUserProfilePartViewModel()
                {
                    Qualifications = new List<PersonQualificationViewModel>()
                    {
                        new PersonQualificationViewModel()
                        {
                            QualificationId = id1,
                            QualificationNumber = "ABC123",
                            QualificationYear = 1999,
                            State = QualificationState.Course
                        }
                    },
                    GraduationYear = graduationYear,
                    GraduationIssuedBy = "XXX-ZZZ"
                },
                currentYear,
                ReportError);

            Assert.False(Reports.ContainsKey("QualificationIds"));
            Assert.False(Reports.ContainsKey("QualificationId"));
            Assert.False(Reports.ContainsKey("QualificationNumber"));
            Assert.False(Reports.ContainsKey("State"));
            Assert.False(Reports.ContainsKey("QualificationYear"));
            Assert.False(Reports.ContainsKey("GraduationYear1"));
            Assert.False(Reports.ContainsKey("GraduationIssuedBy"));
            Assert.True(Reports.ContainsKey("GraduationYear2"));
        }

        [Theory]
        [InlineData(Occupation.Doctor, false, true)]
        [InlineData(Occupation.Doctor, true, false)]
        [InlineData(Occupation.Nurse, false, true)]
        [InlineData(Occupation.Nurse, true, false)]
        public async Task ValidateAsync_ForOccupation_ForQualifications(Occupation occupation, bool addRequired, bool expectedResult)
        {
            var qualificationService = new QualificationService(new SiteServiceMock());
            Reports = new Dictionary<string, string>();

            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

            var settings = await qualificationService.GetQualificationSettingsAsync();

            var qa = settings.Qualifications.First(x => x.Name == "A");
            var qb = settings.Qualifications.First(x => x.Name == "B");

            await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
            {
                QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                {
                    new QualificationPerOccupationViewModel() { Occupation = occupation, QualificationId = qa.Id, IsSelected = true },
                    new QualificationPerOccupationViewModel() { Occupation = occupation, QualificationId = qb.Id, IsSelected = true }
                }
            });

            var qualifications = new List<PersonQualificationViewModel>();
            if (addRequired)
            {
                qualifications.Add(new PersonQualificationViewModel()
                {
                    QualificationId = qa.Id,
                    QualificationNumber = "AA",
                    QualificationYear = 1978,
                    State = QualificationState.Obtained
                });
            }

            var validator = new DiabetesUserProfileValidator();
            await validator.ValidateAsync(
                occupation,
                qualificationService,
                new LocalizerMock<DiabetesUserProfileValidator>(),
                new DiabetesUserProfilePartViewModel()
                {
                    Qualifications = qualifications,
                    GraduationIssuedBy = "GI",
                    GraduationYear = DateTime.UtcNow.AddYears(-8).Year,
                    OtherQualification = "X"
                },
                DateTime.UtcNow.Year,
                ReportError);

            Assert.Equal(expectedResult, Reports.ContainsKey("RequiredQualifications"));
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task ValidateAsync_ForOccupation_ForOtherQualification(bool addOtherQualification, bool expectedResult)
        {
            Reports = new Dictionary<string, string>();

            var validator = new DiabetesUserProfileValidator();
            await validator.ValidateAsync(
                Occupation.Nurse,
                new QualificationService(new SiteServiceMock()),
                new LocalizerMock<DiabetesUserProfileValidator>(),
                new DiabetesUserProfilePartViewModel()
                {
                    GraduationIssuedBy = "GI",
                    GraduationYear = DateTime.UtcNow.AddYears(-8).Year,
                    OtherQualification = addOtherQualification ? "X" : null
                },
                DateTime.UtcNow.Year,
                ReportError);

            Assert.Equal(expectedResult, Reports.ContainsKey("OtherQualification"));
        }


        private void ReportError(string key, string value)
        {
            Reports.Add(key, value);
        }
    }
}
