using IntelliMed.DokiNetIntegration.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Validators;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.Testing.Core;
using OrganiMedCore.Testing.Core.MockServices;
using OrganiMedCore.Testing.Core.SchemaBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class DiabetesUserProfileServiceTest
    {
        [Fact]
        public async Task SetAndGet_ProfileByMemberRightIdAsync_ShouldSave()
        {
            var qualificationService = new QualificationService(new SiteServiceMock());
            var validator = new DiabetesUserProfileValidator();
            var clock = new ClockMock();
            var localizer = new LocalizerMock<DiabetesUserProfileService>();

            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
            var settings = await qualificationService.GetQualificationSettingsAsync();

            var memberRightId = 1000;

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new DiabetesUserProfileService(
                        validator,
                        clock,
                        new ContentManagerMock(session),
                        null,
                        localizer,
                        null,
                        qualificationService,
                        session);

                    await service.SetProfileByMemberRightIdAsync(
                        memberRightId,
                        new DiabetesUserProfilePartViewModel()
                        {
                            Qualifications = new[]
                            {
                                new PersonQualificationViewModel()
                                {
                                    QualificationId = settings.Qualifications.First().Id,
                                    QualificationNumber = "ABC",
                                    QualificationYear = 1999,
                                    State = QualificationState.Obtained
                                }
                            }
                        });
                },
                async session =>
                {
                    var service = new DiabetesUserProfileService(
                        validator,
                        clock,
                        new ContentManagerMock(session),
                        null,
                        localizer,
                        null,
                        qualificationService,
                        session);

                    await service.SetProfileByMemberRightIdAsync(
                        memberRightId,
                        new DiabetesUserProfilePartViewModel()
                        {
                            Qualifications = new[]
                            {
                                new PersonQualificationViewModel()
                                {
                                    QualificationId = settings.Qualifications.First().Id,
                                    QualificationNumber = "XYZ",
                                    QualificationYear = 1999,
                                    State = QualificationState.Student
                                }
                            }
                        });
                },
                async session =>
                {
                    var service = new DiabetesUserProfileService(
                        validator,
                        clock,
                        new ContentManagerMock(session),
                        null,
                        localizer,
                        null,
                        qualificationService,
                        session);

                    var contentItem = await service.GetProfileByMemberRightIdAsync(memberRightId);
                    var part = contentItem.As<DiabetesUserProfilePart>();

                    Assert.NotNull(contentItem);
                    Assert.Equal(settings.Qualifications.First().Id, part.Qualifications.First().QualificationId);
                    Assert.Equal("XYZ", part.Qualifications.First().QualificationNumber);
                    Assert.Equal(1999, part.Qualifications.First().QualificationYear);
                    Assert.Equal(QualificationState.Student, part.Qualifications.First().State);
                },
                async session =>
                {
                    var contentItems = await session
                        .Query<ContentItem, DiabetesUserProfilePartIndex>(index => index.MemberRightId == memberRightId)
                        .ListAsync();

                    Assert.Contains(contentItems, contentItem => contentItem
                        .As<DiabetesUserProfilePart>().Qualifications.First().State == QualificationState.Obtained);
                    Assert.Contains(contentItems, contentItem => contentItem
                        .As<DiabetesUserProfilePart>().Qualifications.First().State == QualificationState.Student);
                });
        }

        [Theory]
        [InlineData(Occupation.Doctor, true, false)]
        [InlineData(Occupation.Doctor, false, true)]
        [InlineData(Occupation.CommunityNurse, true, false)]
        [InlineData(Occupation.CommunityNurse, false, true)]
        [InlineData(Occupation.DiabetesNurseEducator, true, false)]
        [InlineData(Occupation.DiabetesNurseEducator, false, true)]
        [InlineData(Occupation.Dietician, true, false)]
        [InlineData(Occupation.Dietician, false, true)]
        [InlineData(Occupation.Nurse, true, false)]
        [InlineData(Occupation.Nurse, false, true)]
        [InlineData(Occupation.Other, true, false)]
        [InlineData(Occupation.Other, false, true)]
        [InlineData(Occupation.Physiotherapist, true, false)]
        [InlineData(Occupation.Physiotherapist, false, true)]
        public async Task HasMissingQualificationsForOccupation_QualificationsListOnly_ShouldReturnAsExpected(Occupation occupation, bool addRequiredQualifications, bool expectedResult)
        {
            var qualificationService = new QualificationService(new SiteServiceMock());
            var dokiNetMember = new DokiNetMember()
            {
                MemberRightId = 10001,
                DiabetLicenceNumber = "ABC"
            };

            await RequestSessionsAsync(
                async session =>
                {
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

                    var service = new DiabetesUserProfileService(null, null, new ContentManagerMock(session), null, null, null, qualificationService, session);

                    var personQualifications = new List<PersonQualificationViewModel>();
                    if (addRequiredQualifications)
                    {
                        personQualifications.Add(new PersonQualificationViewModel() { QualificationId = qa.Id, QualificationNumber = "A", QualificationYear = 2000, State = QualificationState.Obtained });
                    }

                    await service.SetProfileByMemberRightIdAsync(dokiNetMember.MemberRightId, new DiabetesUserProfilePartViewModel()
                    {
                        Qualifications = personQualifications,

                        // This method is not responsible for testing whether Graduation and OtherQualification fields are filled or empty,
                        // so these should not be empty for case of Nurse, CommunityNurse, Physiotherapist and Other occupations.
                        GraduationIssuedBy = "A",
                        GraduationYear = 2007,
                        OtherQualification = "X"
                    });
                },
                async session =>
                {
                    var service = new DiabetesUserProfileService(null, null, new ContentManagerMock(session), null, null, null, qualificationService, session);

                    var result = await service.HasMissingQualificationsForOccupation(occupation, dokiNetMember);

                    Assert.Equal(result, expectedResult);
                });
        }

        [Theory]
        [InlineData(Occupation.Nurse, false, true)]
        [InlineData(Occupation.Nurse, true, false)]
        [InlineData(Occupation.CommunityNurse, false, true)]
        [InlineData(Occupation.CommunityNurse, true, false)]
        [InlineData(Occupation.Physiotherapist, false, true)]
        [InlineData(Occupation.Physiotherapist, true, false)]
        [InlineData(Occupation.Other, false, true)]
        [InlineData(Occupation.Other, true, false)]

        // In case of the following occupations the expected result is always false regardless of the 'addGraduation' property.
        [InlineData(Occupation.DiabetesNurseEducator, false, false)]
        [InlineData(Occupation.DiabetesNurseEducator, true, false)]
        [InlineData(Occupation.Dietician, false, false)]
        [InlineData(Occupation.Dietician, true, false)]
        [InlineData(Occupation.Doctor, false, false)]
        [InlineData(Occupation.Doctor, true, false)]
        public async Task HasMissingQualificationsForOccupation_GraduationOnly_ShouldReturnAsExpected(Occupation occupation, bool addGraduation, bool expectedResult)
        {
            var qualificationService = new QualificationService(new SiteServiceMock());
            var dokiNetMember = new DokiNetMember()
            {
                MemberRightId = 10001,
                DiabetLicenceNumber = "ABC"
            };

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new DiabetesUserProfileService(null, null, new ContentManagerMock(session), null, null, null, qualificationService, session);

                    var profile = new DiabetesUserProfilePartViewModel();
                    if (addGraduation)
                    {
                        profile.GraduationIssuedBy = "A";
                        profile.GraduationYear = 1977;
                    }

                    // This method is not responsible for testing whether OtherQualification field is filled or empty, so this should not be empty for case of Nurse occupation.
                    profile.OtherQualification = "Other";

                    await service.SetProfileByMemberRightIdAsync(dokiNetMember.MemberRightId, profile);
                },
                async session =>
                {
                    var service = new DiabetesUserProfileService(null, null, new ContentManagerMock(session), null, null, null, qualificationService, session);

                    var result = await service.HasMissingQualificationsForOccupation(occupation, dokiNetMember);

                    Assert.Equal(result, expectedResult);
                });
        }

        [Theory]
        [InlineData(Occupation.Nurse, true, false)]
        [InlineData(Occupation.Nurse, false, true)]

        // In case of the following occupations the expected result is always false regardless of the 'addOtherQualification' property.
        [InlineData(Occupation.CommunityNurse, true, false)]
        [InlineData(Occupation.CommunityNurse, false, false)]
        [InlineData(Occupation.DiabetesNurseEducator, true, false)]
        [InlineData(Occupation.DiabetesNurseEducator, false, false)]
        [InlineData(Occupation.Dietician, true, false)]
        [InlineData(Occupation.Dietician, false, false)]
        [InlineData(Occupation.Doctor, true, false)]
        [InlineData(Occupation.Doctor, false, false)]
        [InlineData(Occupation.Other, true, false)]
        [InlineData(Occupation.Other, false, false)]
        [InlineData(Occupation.Physiotherapist, true, false)]
        [InlineData(Occupation.Physiotherapist, false, false)]
        public async Task HasMissingQualificationsForOccupation_OtherQualificationOnly_ShouldReturnAsExpected(Occupation occupation, bool addOtherQualification, bool expectedResult)
        {
            var qualificationService = new QualificationService(new SiteServiceMock());
            var dokiNetMember = new DokiNetMember()
            {
                MemberRightId = 220156,
                DiabetLicenceNumber = "ABC"
            };

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new DiabetesUserProfileService(null, null, new ContentManagerMock(session), null, null, null, qualificationService, session);

                    var profile = new DiabetesUserProfilePartViewModel();

                    if (addOtherQualification)
                    {
                        profile.OtherQualification = "Other";
                    }

                    // This method is not responsible for testing whether Graduation fields are filled or empty,
                    // so these should not be empty for case of Nurse, CommunityNurse, Physiotherapist and Other occupations.
                    profile.GraduationIssuedBy = "A";
                    profile.GraduationYear = 1977;

                    await service.SetProfileByMemberRightIdAsync(dokiNetMember.MemberRightId, profile);
                },
                async session =>
                {
                    var service = new DiabetesUserProfileService(null, null, new ContentManagerMock(session), null, null, null, qualificationService, session);

                    var result = await service.HasMissingQualificationsForOccupation(occupation, dokiNetMember);

                    Assert.Equal(result, expectedResult);
                });
        }

        [Theory]
        [InlineData(Occupation.Doctor, false, false)]
        [InlineData(Occupation.Dietician, false, false)]
        [InlineData(Occupation.DiabetesNurseEducator, false, false)]
        [InlineData(Occupation.CommunityNurse, true, false)]
        [InlineData(Occupation.Nurse, true, true)]
        [InlineData(Occupation.Other, true, false)]
        [InlineData(Occupation.Physiotherapist, true, false)]
        public async Task SetPartialProfileByMemberRightIdAsync(Occupation occupation, bool graduationShouldBeUpdated, bool otherQualificationShouldBeUpdated)
        {
            var qualificationService = new QualificationService(new SiteServiceMock());
            var validator = new DiabetesUserProfileValidator();
            var clock = new ClockMock();
            var localizer = new LocalizerMock<DiabetesUserProfileService>();

            // Required qualifications:
            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

            // Others
            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });
            await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

            var settings = await qualificationService.GetQualificationSettingsAsync();

            var qa = settings.Qualifications.First(x => x.Name == "A");
            var qb = settings.Qualifications.First(x => x.Name == "B");
            var qc = settings.Qualifications.First(x => x.Name == "C");
            var qd = settings.Qualifications.First(x => x.Name == "D");

            await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel
            {
                QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                {
                    new QualificationPerOccupationViewModel() { Occupation = occupation, QualificationId = qa.Id, IsSelected = true },
                    new QualificationPerOccupationViewModel() { Occupation = occupation, QualificationId = qb.Id, IsSelected = true },
                    new QualificationPerOccupationViewModel() { Occupation = occupation, QualificationId = qc.Id, IsSelected = false }, // !!!
                    new QualificationPerOccupationViewModel() { Occupation = occupation, QualificationId = qd.Id, IsSelected = false }  // !!!
                }
            });

            var memberRightId = 1000;

            await RequestSessionsAsync(
                // Adding all the qualifications except one required
                async session =>
                {
                    var service = new DiabetesUserProfileService(
                        validator,
                        clock,
                        new ContentManagerMock(session),
                        null,
                        localizer,
                        null,
                        qualificationService,
                        session);

                    await service.SetProfileByMemberRightIdAsync(memberRightId, new DiabetesUserProfilePartViewModel()
                    {
                        Qualifications = new[]
                        {
                            new PersonQualificationViewModel() { QualificationId = qa.Id, QualificationNumber = "A", QualificationYear = 1999, State = QualificationState.Obtained },
                            new PersonQualificationViewModel() { QualificationId = qc.Id, QualificationNumber = "C", QualificationYear = 2001, State = QualificationState.Obtained },
                            new PersonQualificationViewModel() { QualificationId = qd.Id, QualificationNumber = "D", QualificationYear = 2002, State = QualificationState.Obtained },
                        },
                        GraduationIssuedBy = "GI",
                        GraduationYear = 2012,
                        OtherQualification = "X"
                    });
                },
                // Adding and updating the required qualifications
                async session =>
                {
                    var service = new DiabetesUserProfileService(
                        validator,
                        clock,
                        new ContentManagerMock(session),
                        null,
                        localizer,
                        null,
                        qualificationService,
                        session);

                    var viewModel = new DiabetesUserProfilePartViewModel()
                    {
                        Qualifications = new[]
                        {
                            // Updating A
                            new PersonQualificationViewModel() { QualificationId = qa.Id, QualificationNumber = "updated A", QualificationYear = 1988, State = QualificationState.Obtained },

                            // Adding B
                            new PersonQualificationViewModel() { QualificationId = qb.Id, QualificationNumber = "added B", QualificationYear = 2006, State = QualificationState.Obtained }
                        }
                    };

                    if (graduationShouldBeUpdated)
                    {
                        viewModel.GraduationIssuedBy = "updated GI";
                        viewModel.GraduationYear = 2009;
                    }

                    if (otherQualificationShouldBeUpdated)
                    {
                        viewModel.OtherQualification = "XX";
                    }

                    await service.SetPartialProfileByMemberRightIdAsync(occupation, memberRightId, viewModel);
                },
                async session =>
                {
                    var service = new DiabetesUserProfileService(
                        validator,
                        clock,
                        new ContentManagerMock(session),
                        null,
                        localizer,
                        null,
                        qualificationService,
                        session);

                    var contentItem = await service.GetProfileByMemberRightIdAsync(memberRightId);
                    var part = contentItem.As<DiabetesUserProfilePart>();

                    Assert.Contains(part.Qualifications, q => q.QualificationId == qa.Id && q.QualificationNumber == "updated A");
                    Assert.Contains(part.Qualifications, q => q.QualificationId == qb.Id && q.QualificationNumber == "added B");

                    if (graduationShouldBeUpdated)
                    {
                        Assert.Equal("updated GI", part.GraduationIssuedBy);
                        Assert.Equal(2009, part.GraduationYear);
                    }
                    else
                    {
                        Assert.Equal("GI", part.GraduationIssuedBy);
                        Assert.Equal(2012, part.GraduationYear);
                    }

                    if (otherQualificationShouldBeUpdated)
                    {
                        Assert.Equal("XX", part.OtherQualification);
                    }
                    else
                    {
                        Assert.Equal("X", part.OtherQualification);
                    }
                });
        }


        private async Task RequestSessionsAsync(params Func<ISession, Task>[] sessions)
        {
            using (var sessionHandler = new YesSqlSessionHandler())
            {
                await sessionHandler.InitializeAsync();
                await sessionHandler.RegisterSchemaAndIndexes(
                    schemaBuilder =>
                    {
                        ContentItemSchemaBuilder.Build(schemaBuilder);
                        DiabetesUserProfileSchemaBuilder.Build(schemaBuilder);
                    },
                    store =>
                    {
                        store.RegisterIndexes<ContentItemIndexProvider>();
                        store.RegisterIndexes<DiabetesUserProfilePartIndexProvider>();
                    });
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }
    }
}
