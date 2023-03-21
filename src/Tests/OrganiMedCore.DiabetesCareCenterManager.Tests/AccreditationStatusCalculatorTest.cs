using IntelliMed.DokiNetIntegration.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.DiabetesCareCenterManager.Tests.MockServices;
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
    public class AccreditationStatusCalculatorTest
    {
        [Fact]
        public async Task CalculateAccreditationStatus_ShouldReturn_Accredited()
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                siteSettings.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), x =>
                {
                    x.LaboratoryList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "10", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "11", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "12", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "13", Required = false }
                    };

                    x.ToolsList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "1", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "2", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "3", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "4", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "5", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "6", Required = false }
                    };
                });

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);

                // Creating a Doctor
                var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                doctor.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 11;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(doctor);

                // Creating a Dietician
                var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                dietician.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 12;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qB.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(dietician);

                // Creating a DiabetesNurseEducator
                var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 13;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(diabetesNurseEducator);

                colleaguesAndLeader.Add(leader);
                colleaguesAndLeader.Add(doctor);
                colleaguesAndLeader.Add(dietician);
                colleaguesAndLeader.Add(diabetesNurseEducator);


                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>()
                    {
                        new CenterProfileEquipment<int?>() { Id = "1", Value = 1 },
                        new CenterProfileEquipment<int?>() { Id = "2", Value = 5 },
                        new CenterProfileEquipment<int?>() { Id = "3", Value = 7 },
                        new CenterProfileEquipment<int?>() { Id = "4", Value = 4 },

                        new CenterProfileEquipment<int?>() { Id = "5", Value = 0 },
                        new CenterProfileEquipment<int?>() { Id = "6", Value = null },
                    },
                    Laboratory = new List<CenterProfileEquipment<bool>>()
                    {
                        new CenterProfileEquipment<bool>() { Id = "10", Value = true },
                        new CenterProfileEquipment<bool>() { Id = "11", Value = true },

                        new CenterProfileEquipment<bool>() { Id = "12", Value = false }
                    },
                    Colleagues = new List<Colleague>()
                    {
                        new Colleague()
                        {
                            Occupation = Occupation.Doctor,
                            MemberRightId = 11,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.Dietician,
                            MemberRightId = 12,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            MemberRightId = 13,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                            }
                        }
                    },
                    BackgroundConcilium = true,
                    BackgroundInpatient = true
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(new[]
                    {
                        new DokiNetMember() { MemberRightId = 11, HasMemberShip = true, IsDueOk = true , DiabetLicenceNumber = "ABC" },
                        new DokiNetMember() { MemberRightId = 10, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 12, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 13, HasMemberShip = true, IsDueOk = true }
                    }),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);

                // Assert
                Assert.Equal(AccreditationStatus.Accredited, accreditationStatusResult.AccreditationStatus);
                Assert.False(accreditationStatusResult.BackgroundConcilium);
                Assert.False(accreditationStatusResult.BackgroundInpatient);
                Assert.False(accreditationStatusResult.MdtLicence);
                Assert.Empty(accreditationStatusResult.Membership);
                Assert.Empty(accreditationStatusResult.MembershipFee);
                Assert.Empty(accreditationStatusResult.PersonalConditions);
                Assert.Empty(accreditationStatusResult.Laboratory);
                Assert.Empty(accreditationStatusResult.Tools);
            });
        }

        [Fact]
        public async Task CalculateAccreditationStatus_InvalidTools_ShouldReturn_Registered()
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                siteSettings.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), x =>
                {
                    x.LaboratoryList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "10", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "11", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "12", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "13", Required = false }
                    };

                    x.ToolsList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "1", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "2", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "3", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "4", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "5", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "6", Required = false }
                    };
                });

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);

                // Creating a Doctor
                var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                doctor.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 11;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(doctor);

                // Creating a Dietician
                var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                dietician.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 12;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qB.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(dietician);

                // Creating a DiabetesNurseEducator
                var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 13;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(diabetesNurseEducator);

                colleaguesAndLeader.Add(leader);
                colleaguesAndLeader.Add(doctor);
                colleaguesAndLeader.Add(dietician);
                colleaguesAndLeader.Add(diabetesNurseEducator);


                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>()
                    {
                        new CenterProfileEquipment<int?>() { Id = "1", Value = 1 },
                        new CenterProfileEquipment<int?>() { Id = "2", Value = 5 },
                        //new CenterProfileEquipment<int?>() { Id = "3", Value = 7 },
                        new CenterProfileEquipment<int?>() { Id = "4", Value = 4 },

                        new CenterProfileEquipment<int?>() { Id = "5", Value = 0 },
                        new CenterProfileEquipment<int?>() { Id = "6", Value = null },
                    },
                    Laboratory = new List<CenterProfileEquipment<bool>>()
                    {
                        new CenterProfileEquipment<bool>() { Id = "10", Value = true },
                        new CenterProfileEquipment<bool>() { Id = "11", Value = true },

                        new CenterProfileEquipment<bool>() { Id = "12", Value = false }
                    },
                    Colleagues = new List<Colleague>()
                    {
                        new Colleague()
                        {
                            Occupation = Occupation.Doctor,
                            MemberRightId = 11,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.Dietician,
                            MemberRightId = 12,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            MemberRightId = 13,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                            }
                        }
                    },
                    BackgroundConcilium = true,
                    BackgroundInpatient = true
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(new[]
                    {
                        new DokiNetMember() { MemberRightId = 10, DiabetLicenceNumber = "LDR", IsDueOk = true, HasMemberShip = true },
                        new DokiNetMember() { MemberRightId = 11, DiabetLicenceNumber = "A", IsDueOk = true, HasMemberShip = true },
                        new DokiNetMember() { MemberRightId = 12, DiabetLicenceNumber = "B", IsDueOk = true, HasMemberShip = true },
                        new DokiNetMember() { MemberRightId = 13, DiabetLicenceNumber = "C", IsDueOk = true, HasMemberShip = true }
                    }),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);

                // Assert
                Assert.Equal(AccreditationStatus.Registered, accreditationStatusResult.AccreditationStatus);
                Assert.Single(accreditationStatusResult.Tools);
                Assert.Contains(accreditationStatusResult.Tools, x => x == "3");

                Assert.False(accreditationStatusResult.BackgroundConcilium);
                Assert.False(accreditationStatusResult.BackgroundInpatient);
                Assert.Empty(accreditationStatusResult.Laboratory);
                Assert.False(accreditationStatusResult.MdtLicence);
                Assert.Empty(accreditationStatusResult.Membership);
                Assert.Empty(accreditationStatusResult.MembershipFee);
                Assert.Empty(accreditationStatusResult.PersonalConditions);
            });
        }

        [Fact]
        public async Task CalculateAccreditationStatus_InvalidLaboratoryEquipments_ShouldReturn_Registered()
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                siteSettings.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), x =>
                {
                    x.ToolsList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "1", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "2", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "3", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "4", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "5", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "6", Required = false }
                    };

                    x.LaboratoryList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "10", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "11", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "12", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "13", Required = false }
                    };
                });

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);

                // Creating a Doctor
                var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                doctor.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 11;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(doctor);

                // Creating a Dietician
                var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                dietician.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 12;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qB.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(dietician);

                // Creating a DiabetesNurseEducator
                var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 13;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(diabetesNurseEducator);

                colleaguesAndLeader.Add(leader);
                colleaguesAndLeader.Add(doctor);
                colleaguesAndLeader.Add(dietician);
                colleaguesAndLeader.Add(diabetesNurseEducator);

                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>()
                    {
                        new CenterProfileEquipment<int?>() { Id = "1", Value = 1 },
                        new CenterProfileEquipment<int?>() { Id = "2", Value = 5 },
                        new CenterProfileEquipment<int?>() { Id = "3", Value = 7 },
                        new CenterProfileEquipment<int?>() { Id = "4", Value = 4 },

                        new CenterProfileEquipment<int?>() { Id = "5", Value = 0 },
                        new CenterProfileEquipment<int?>() { Id = "6", Value = null },
                    },
                    Laboratory = new List<CenterProfileEquipment<bool>>()
                    {
                        new CenterProfileEquipment<bool>() { Id = "10", Value = true },
                        new CenterProfileEquipment<bool>() { Id = "11", Value = false },

                        new CenterProfileEquipment<bool>() { Id = "12", Value = false }
                    },
                    Colleagues = new List<Colleague>()
                    {
                        new Colleague()
                        {
                            Occupation = Occupation.Doctor,
                            MemberRightId = 11,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.Dietician,
                            MemberRightId = 12,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            MemberRightId = 13,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                            }
                        }
                    },
                    BackgroundConcilium = true,
                    BackgroundInpatient = true
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(new[]
                    {
                        new DokiNetMember() { MemberRightId = 10, DiabetLicenceNumber = "LDR", HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 11, DiabetLicenceNumber = "ABC", HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 12, DiabetLicenceNumber = "BCA", HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 13, DiabetLicenceNumber = "DBA", HasMemberShip = true, IsDueOk = true }
                    }),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);

                // Assert
                Assert.Equal(AccreditationStatus.Registered, accreditationStatusResult.AccreditationStatus);
                Assert.Single(accreditationStatusResult.Laboratory);
                Assert.Contains(accreditationStatusResult.Laboratory, x => x == "11");

                Assert.False(accreditationStatusResult.BackgroundConcilium);
                Assert.False(accreditationStatusResult.BackgroundInpatient);
                Assert.False(accreditationStatusResult.MdtLicence);
                Assert.Empty(accreditationStatusResult.Membership);
                Assert.Empty(accreditationStatusResult.MembershipFee);
                Assert.Empty(accreditationStatusResult.PersonalConditions);
                Assert.Empty(accreditationStatusResult.Tools);
            });
        }

        [Fact]
        public async Task CalculateAccreditationStatus_NoTools_ShouldReturn_Accredited()
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                siteSettings.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), x =>
                {
                    x.LaboratoryList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "10", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "11", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "12", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "13", Required = false }
                    };

                    x.ToolsList = new List<CenterProfileEquipmentSetting>();
                });

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);

                // Creating a Doctor
                var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                doctor.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 11;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(doctor);

                // Creating a Dietician
                var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                dietician.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 12;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qB.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(dietician);

                // Creating a DiabetesNurseEducator
                var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 13;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(diabetesNurseEducator);

                colleaguesAndLeader.Add(leader);
                colleaguesAndLeader.Add(doctor);
                colleaguesAndLeader.Add(dietician);
                colleaguesAndLeader.Add(diabetesNurseEducator);


                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>(),
                    Laboratory = new List<CenterProfileEquipment<bool>>()
                    {
                        new CenterProfileEquipment<bool>() { Id = "10", Value = true },
                        new CenterProfileEquipment<bool>() { Id = "11", Value = true },

                        new CenterProfileEquipment<bool>() { Id = "12", Value = false }
                    },
                    Colleagues = new List<Colleague>()
                    {
                        new Colleague()
                        {
                            Occupation = Occupation.Doctor,
                            MemberRightId = 11,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.Dietician,
                            MemberRightId = 12,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            MemberRightId = 13,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                            }
                        }
                    },
                    BackgroundConcilium = true,
                    BackgroundInpatient = true
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(new[]
                    {
                        new DokiNetMember() { MemberRightId = 10, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 11, DiabetLicenceNumber = "ABC", HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 12, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 13, HasMemberShip = true, IsDueOk = true }

                    }),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);

                // Assert
                Assert.Equal(AccreditationStatus.Accredited, accreditationStatusResult.AccreditationStatus);
                Assert.Empty(accreditationStatusResult.Tools);

                Assert.False(accreditationStatusResult.BackgroundConcilium);
                Assert.False(accreditationStatusResult.BackgroundInpatient);
                Assert.Empty(accreditationStatusResult.Laboratory);
                Assert.False(accreditationStatusResult.MdtLicence);
                Assert.Empty(accreditationStatusResult.Membership);
                Assert.Empty(accreditationStatusResult.MembershipFee);
                Assert.Empty(accreditationStatusResult.PersonalConditions);
            });
        }

        [Theory]
        [InlineData(true, true, AccreditationStatus.Accredited)]
        [InlineData(true, false, AccreditationStatus.Registered)]
        [InlineData(false, true, AccreditationStatus.Registered)]
        [InlineData(false, false, AccreditationStatus.Registered)]
        public async Task CalculateAccreditationStatus_Backgrounds(
            bool backgroundConcilium,
            bool backgroundInpatient,
            AccreditationStatus expectedAccreditationStatus)
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                siteSettings.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), x =>
                {
                    x.LaboratoryList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "10", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "11", Required = true }
                    };

                    x.ToolsList = new List<CenterProfileEquipmentSetting>();
                });

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);

                // Creating a Doctor
                var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                doctor.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 11;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(doctor);

                // Creating a Dietician
                var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                dietician.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 12;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qB.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(dietician);

                // Creating a DiabetesNurseEducator
                var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 13;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(diabetesNurseEducator);

                colleaguesAndLeader.Add(leader);
                colleaguesAndLeader.Add(doctor);
                colleaguesAndLeader.Add(dietician);
                colleaguesAndLeader.Add(diabetesNurseEducator);

                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>(),
                    Laboratory = new List<CenterProfileEquipment<bool>>()
                    {
                        new CenterProfileEquipment<bool>() { Id = "10", Value = true },
                        new CenterProfileEquipment<bool>() { Id = "11", Value = true }
                    },
                    Colleagues = new List<Colleague>()
                    {
                        new Colleague()
                        {
                            Occupation = Occupation.Doctor,
                            MemberRightId = 11,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.Dietician,
                            MemberRightId = 12,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            MemberRightId = 13,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                            }
                        }
                    },
                    BackgroundConcilium = backgroundConcilium,
                    BackgroundInpatient = backgroundInpatient
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(new[]
                    {
                        new DokiNetMember()
                        {
                            MemberRightId = 11,
                            DiabetLicenceNumber = "ABC",
                            HasMemberShip = true,
                            IsDueOk = true
                        },
                        new DokiNetMember() { MemberRightId = 10, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 12, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 13, HasMemberShip = true, IsDueOk = true }
                    }),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);

                // Assert
                Assert.Equal(expectedAccreditationStatus, accreditationStatusResult.AccreditationStatus);

                Assert.Equal(backgroundConcilium, !accreditationStatusResult.BackgroundConcilium);
                Assert.Equal(backgroundInpatient, !accreditationStatusResult.BackgroundInpatient);

                Assert.Empty(accreditationStatusResult.Laboratory);
                Assert.False(accreditationStatusResult.MdtLicence);
                Assert.Empty(accreditationStatusResult.Membership);
                Assert.Empty(accreditationStatusResult.MembershipFee);
                Assert.Empty(accreditationStatusResult.PersonalConditions);
                Assert.Empty(accreditationStatusResult.Tools);
            });
        }

        [Fact]
        public async Task CalculateAccreditationStatus_NoLaboratoryEquipments_ShouldReturn_Accredited()
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                siteSettings.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), x =>
                {
                    x.LaboratoryList = new List<CenterProfileEquipmentSetting>();

                    x.ToolsList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "1", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "2", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "3", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "4", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "5", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "6", Required = false }
                    };
                });

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);

                // Creating a Doctor
                var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                doctor.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 11;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(doctor);

                // Creating a Dietician
                var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                dietician.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 12;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qB.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(dietician);

                // Creating a DiabetesNurseEducator
                var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 13;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(diabetesNurseEducator);

                colleaguesAndLeader.Add(leader);
                colleaguesAndLeader.Add(doctor);
                colleaguesAndLeader.Add(dietician);
                colleaguesAndLeader.Add(diabetesNurseEducator);


                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>()
                    {
                        new CenterProfileEquipment<int?>() { Id = "1", Value = 1 },
                        new CenterProfileEquipment<int?>() { Id = "2", Value = 5 },
                        new CenterProfileEquipment<int?>() { Id = "3", Value = 7 },
                        new CenterProfileEquipment<int?>() { Id = "4", Value = 4 },

                        new CenterProfileEquipment<int?>() { Id = "5", Value = 0 },
                        new CenterProfileEquipment<int?>() { Id = "6", Value = null },
                    },
                    Laboratory = new List<CenterProfileEquipment<bool>>(),
                    Colleagues = new List<Colleague>()
                    {
                        new Colleague()
                        {
                            Occupation = Occupation.Doctor,
                            MemberRightId = 11,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.Dietician,
                            MemberRightId = 12,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            MemberRightId = 13,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                            }
                        }
                    },
                    BackgroundConcilium = true,
                    BackgroundInpatient = true
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(new[]
                    {
                        new DokiNetMember() { MemberRightId = 11, HasMemberShip = true, IsDueOk = true, DiabetLicenceNumber = "ABC" },
                        new DokiNetMember() { MemberRightId = 12, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 13, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 10, HasMemberShip = true, IsDueOk = true }
                    }),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);

                // Assert
                Assert.Equal(AccreditationStatus.Accredited, accreditationStatusResult.AccreditationStatus);
                Assert.Empty(accreditationStatusResult.Laboratory);

                Assert.False(accreditationStatusResult.BackgroundConcilium);
                Assert.False(accreditationStatusResult.BackgroundInpatient);
                Assert.Empty(accreditationStatusResult.Laboratory);
                Assert.False(accreditationStatusResult.MdtLicence);
                Assert.Empty(accreditationStatusResult.Membership);
                Assert.Empty(accreditationStatusResult.MembershipFee);
                Assert.Empty(accreditationStatusResult.PersonalConditions);
                Assert.Empty(accreditationStatusResult.Tools);
            });
        }

        [Fact]
        public async Task CalculateAccreditationStatus_NoEquipments_ShouldReturn_Accredited()
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                siteSettings.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), x =>
                {
                    x.LaboratoryList = new List<CenterProfileEquipmentSetting>();
                    x.ToolsList = new List<CenterProfileEquipmentSetting>();
                });

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);

                // Creating a Doctor
                var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                doctor.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 11;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(doctor);

                // Creating a Dietician
                var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                dietician.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 12;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qB.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(dietician);

                // Creating a DiabetesNurseEducator
                var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 13;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(diabetesNurseEducator);

                colleaguesAndLeader.Add(leader);
                colleaguesAndLeader.Add(doctor);
                colleaguesAndLeader.Add(dietician);
                colleaguesAndLeader.Add(diabetesNurseEducator);


                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>(),
                    Laboratory = new List<CenterProfileEquipment<bool>>(),
                    Colleagues = new List<Colleague>()
                    {
                        new Colleague()
                        {
                            Occupation = Occupation.Doctor,
                            MemberRightId = 11,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.Dietician,
                            MemberRightId = 12,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            MemberRightId = 13,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                            }
                        }
                    },
                    BackgroundConcilium = true,
                    BackgroundInpatient = true
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(new[]
                    {
                        new DokiNetMember() { MemberRightId = 11, DiabetLicenceNumber = "ABC", HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 12, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 13, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 10, HasMemberShip = true, IsDueOk = true }
                    }),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);

                // Assert
                Assert.Equal(AccreditationStatus.Accredited, accreditationStatusResult.AccreditationStatus);
                Assert.Empty(accreditationStatusResult.Laboratory);
                Assert.Empty(accreditationStatusResult.Tools);

                Assert.False(accreditationStatusResult.BackgroundConcilium);
                Assert.False(accreditationStatusResult.BackgroundInpatient);
                Assert.False(accreditationStatusResult.MdtLicence);
                Assert.Empty(accreditationStatusResult.Membership);
                Assert.Empty(accreditationStatusResult.MembershipFee);
                Assert.Empty(accreditationStatusResult.PersonalConditions);
            });
        }

        [Theory]
        // Not enough doctors to be Accredited or TemporarilyAccredited
        [InlineData(10, 10, 8, 2, 2, 2, 1, 1, 1, AccreditationStatus.Registered)]
        // Not enough dieticians to be Accredited or TemporarilyAccredited
        [InlineData(10, 10, 10, 2, 2, 1, 1, 1, 1, AccreditationStatus.Registered)]
        // Not enough educators to be Accredited or TemporarilyAccredited
        [InlineData(2, 2, 2, 1, 1, 1, 3, 2, 1, AccreditationStatus.Registered)]
        // Enough doctors to be TemporarilyAccredited
        [InlineData(3, 2, 2, 1, 1, 1, 1, 1, 1, AccreditationStatus.TemporarilyAccredited)]
        // Enough dieticians to be TemporarilyAccredited
        [InlineData(4, 4, 4, 3, 2, 2, 1, 1, 1, AccreditationStatus.TemporarilyAccredited)]
        // Enough educators to be TemporarilyAccredited
        [InlineData(7, 7, 7, 2, 2, 2, 3, 1, 2, AccreditationStatus.TemporarilyAccredited)]

        [InlineData(2, 2, 2, 1, 1, 1, 1, 1, 1, AccreditationStatus.Accredited)]
        [InlineData(2, 2, 3, 1, 1, 4, 1, 1, 5, AccreditationStatus.Accredited)]
        [InlineData(1, 1, 1, 0, 0, 0, 0, 0, 0, AccreditationStatus.Accredited)]
        [InlineData(1, 1, 0, 0, 0, 0, 0, 0, 0, AccreditationStatus.Accredited)]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0, 0, AccreditationStatus.Accredited)]
        public async Task CalculateAccreditationStatus_HeadCountCheck_ShouldReturnAsExpected(
            int requiredDoctorsCountAccredited,
            int requiredDoctorsCountTemporarilyAccredited,
            int doctorsCountAddredited,
            int requiredDieticianCountAddredited,
            int requiredDieticianCountTemporarilyAddredited,
            int dieticianCountAddredited,
            int requiredDiabetesNurseEducatorCountAccredited,
            int requiredDiabetesNurseEducatorCountTemporarilyAccredited,
            int diabetesNurseEducatorCount,
            AccreditationStatus expectedAccreditationStatus)
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                siteSettings.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), x =>
                {
                    x.LaboratoryList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "10", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "11", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "12", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "13", Required = false }
                    };

                    x.ToolsList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "1", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "2", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "3", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "4", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "5", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "6", Required = false }
                    };
                });

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = requiredDoctorsCountAccredited },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = requiredDieticianCountAddredited },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = requiredDiabetesNurseEducatorCountAccredited }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = requiredDoctorsCountTemporarilyAccredited },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = requiredDieticianCountTemporarilyAddredited },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount =  requiredDiabetesNurseEducatorCountTemporarilyAccredited }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);
                var dokiNetMembers = new List<DokiNetMember>()
                {
                    new DokiNetMember() { MemberRightId = 10, DiabetLicenceNumber = "X", HasMemberShip = true, IsDueOk = true }
                };

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);
                colleaguesAndLeader.Add(leader);

                // Creating Doctors
                for (var i = 1; i < doctorsCountAddredited; i++)
                {
                    var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                    doctor.Alter<DiabetesUserProfilePart>(p =>
                    {
                        p.MemberRightId = 1000000 + i;
                        p.Qualifications = new[]
                        {
                            new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                        };

                        dokiNetMembers.Add(new DokiNetMember()
                        {
                            MemberRightId = p.MemberRightId,
                            HasMemberShip = true,
                            IsDueOk = true
                        });
                    });
                    await contentManager.CreateAsync(doctor);
                    colleaguesAndLeader.Add(doctor);
                }

                // Creating Dieticians
                for (var i = 0; i < dieticianCountAddredited; i++)
                {
                    var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                    dietician.Alter<DiabetesUserProfilePart>(p =>
                    {
                        p.MemberRightId = 2000000 + i;
                        p.Qualifications = new[]
                        {
                            new PersonQualification() { QualificationId = qB.Id, State = QualificationState.Obtained }
                        };

                        dokiNetMembers.Add(new DokiNetMember()
                        {
                            MemberRightId = p.MemberRightId,
                            HasMemberShip = true,
                            IsDueOk = true
                        });
                    });
                    await contentManager.CreateAsync(dietician);
                    colleaguesAndLeader.Add(dietician);
                }

                for (var i = 0; i < diabetesNurseEducatorCount; i++)
                {
                    // Creating a DiabetesNurseEducator
                    var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                    diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                    {
                        p.MemberRightId = 3000000 + i;
                        p.Qualifications = new[]
                        {
                            new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Obtained }
                        };

                        dokiNetMembers.Add(new DokiNetMember()
                        {
                            MemberRightId = p.MemberRightId,
                            HasMemberShip = true,
                            IsDueOk = true
                        });
                    });
                    await contentManager.CreateAsync(diabetesNurseEducator);
                    colleaguesAndLeader.Add(diabetesNurseEducator);
                }

                var colleagues = new List<Colleague>();
                for (var i = 1; i < doctorsCountAddredited; i++)
                {
                    colleagues.Add(new Colleague()
                    {
                        Occupation = Occupation.Doctor,
                        MemberRightId = 1000000 + i,
                        StatusHistory = new List<ColleagueStatusItem>()
                        {
                            new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                        }
                    });
                }

                for (var i = 0; i < dieticianCountAddredited; i++)
                {
                    colleagues.Add(new Colleague()
                    {
                        Occupation = Occupation.Dietician,
                        MemberRightId = 2000000 + i,
                        StatusHistory = new List<ColleagueStatusItem>()
                        {
                            new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                        }
                    });
                }

                for (var i = 0; i < diabetesNurseEducatorCount; i++)
                {
                    colleagues.Add(new Colleague()
                    {
                        Occupation = Occupation.DiabetesNurseEducator,
                        MemberRightId = 3000000 + i,
                        StatusHistory = new List<ColleagueStatusItem>()
                        {
                            new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                        }
                    });
                }

                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>()
                    {
                        new CenterProfileEquipment<int?>() { Id = "1", Value = 1 },
                        new CenterProfileEquipment<int?>() { Id = "2", Value = 5 },
                        new CenterProfileEquipment<int?>() { Id = "3", Value = 7 },
                        new CenterProfileEquipment<int?>() { Id = "4", Value = 4 },
                        new CenterProfileEquipment<int?>() { Id = "5", Value = 0 },
                        new CenterProfileEquipment<int?>() { Id = "6", Value = null },
                    },
                    Laboratory = new List<CenterProfileEquipment<bool>>()
                    {
                        new CenterProfileEquipment<bool>() { Id = "10", Value = true },
                        new CenterProfileEquipment<bool>() { Id = "11", Value = true },
                        new CenterProfileEquipment<bool>() { Id = "12", Value = false }
                    },
                    Colleagues = colleagues,
                    BackgroundConcilium = true,
                    BackgroundInpatient = true
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(dokiNetMembers),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);

                // Assert
                Assert.Equal(expectedAccreditationStatus, accreditationStatusResult.AccreditationStatus);

                // +1 because of the leader
                if (doctorsCountAddredited + 1 < requiredDoctorsCountAccredited ||
                    doctorsCountAddredited + 1 < requiredDoctorsCountTemporarilyAccredited)
                {
                    Assert.Contains(accreditationStatusResult.PersonalConditions, x => x.Occupation == Occupation.Doctor);
                }

                if (dieticianCountAddredited < requiredDieticianCountAddredited ||
                    dieticianCountAddredited < requiredDieticianCountTemporarilyAddredited)
                {
                    Assert.Contains(accreditationStatusResult.PersonalConditions, x => x.Occupation == Occupation.Dietician);
                }

                if (diabetesNurseEducatorCount < requiredDiabetesNurseEducatorCountAccredited ||
                    diabetesNurseEducatorCount < requiredDiabetesNurseEducatorCountTemporarilyAccredited)
                {
                    Assert.Contains(accreditationStatusResult.PersonalConditions, x => x.Occupation == Occupation.DiabetesNurseEducator);
                }

                Assert.False(accreditationStatusResult.BackgroundConcilium);
                Assert.False(accreditationStatusResult.BackgroundInpatient);
                Assert.Empty(accreditationStatusResult.Laboratory);
                Assert.Empty(accreditationStatusResult.Tools);
                Assert.False(accreditationStatusResult.MdtLicence);
                Assert.Empty(accreditationStatusResult.Membership);
                Assert.Empty(accreditationStatusResult.MembershipFee);
            });
        }

        [Fact]
        public async Task CalculateAccreditationStatus_ShouldReturn_TemporarilyAccredited()
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                siteSettings.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), x =>
                {
                    x.LaboratoryList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "10", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "11", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "12", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "13", Required = false }
                    };

                    x.ToolsList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "1", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "2", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "3", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "4", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "5", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "6", Required = false }
                    };
                });

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);

                // Creating a Doctor
                var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                doctor.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 11;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(doctor);

                // Creating a Dietician
                var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                dietician.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 12;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qB.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(dietician);

                // Creating a DiabetesNurseEducator
                var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 13;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Student }
                    };
                });
                await contentManager.CreateAsync(diabetesNurseEducator);

                colleaguesAndLeader.Add(leader);
                colleaguesAndLeader.Add(doctor);
                colleaguesAndLeader.Add(dietician);
                colleaguesAndLeader.Add(diabetesNurseEducator);


                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>()
                    {
                        new CenterProfileEquipment<int?>() { Id = "1", Value = 1 },
                        new CenterProfileEquipment<int?>() { Id = "2", Value = 5 },
                        new CenterProfileEquipment<int?>() { Id = "3", Value = 7 },
                        new CenterProfileEquipment<int?>() { Id = "4", Value = 4 },

                        new CenterProfileEquipment<int?>() { Id = "5", Value = 0 },
                        new CenterProfileEquipment<int?>() { Id = "6", Value = null },
                    },
                    Laboratory = new List<CenterProfileEquipment<bool>>()
                    {
                        new CenterProfileEquipment<bool>() { Id = "10", Value = true },
                        new CenterProfileEquipment<bool>() { Id = "11", Value = true },

                        new CenterProfileEquipment<bool>() { Id = "12", Value = false }
                    },
                    Colleagues = new List<Colleague>()
                    {
                        new Colleague()
                        {
                            Occupation = Occupation.Doctor,
                            MemberRightId = 11,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.Dietician,
                            MemberRightId = 12,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            MemberRightId = 13,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                            }
                        }
                    },
                    BackgroundConcilium = true,
                    BackgroundInpatient = true
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(new[]
                    {
                        new DokiNetMember() { MemberRightId = 11, HasMemberShip = true, IsDueOk = true, DiabetLicenceNumber = "ABC" },
                        new DokiNetMember() { MemberRightId = 10, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 12, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 13, HasMemberShip = true, IsDueOk = true }
                    }),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);

                // Assert
                Assert.Equal(AccreditationStatus.TemporarilyAccredited, accreditationStatusResult.AccreditationStatus);
                Assert.Single(accreditationStatusResult.PersonalConditions);
                Assert.Contains(accreditationStatusResult.PersonalConditions, x => x.Occupation == Occupation.DiabetesNurseEducator);

                Assert.False(accreditationStatusResult.BackgroundConcilium);
                Assert.False(accreditationStatusResult.BackgroundInpatient);
                Assert.Empty(accreditationStatusResult.Laboratory);
                Assert.False(accreditationStatusResult.MdtLicence);
                Assert.Empty(accreditationStatusResult.Membership);
                Assert.Empty(accreditationStatusResult.MembershipFee);
                Assert.Empty(accreditationStatusResult.Tools);
            });
        }

        [Fact]
        public async Task CalculateAccreditationStatus_MissingQualifications_ShouldReturn_Registrated()
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                siteSettings.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), x =>
                {
                    x.LaboratoryList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "10", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "11", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "12", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "13", Required = false }
                    };

                    x.ToolsList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "1", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "2", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "3", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "4", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "5", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "6", Required = false }
                    };
                });

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);

                // Creating a Doctor
                var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                doctor.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 11;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(doctor);

                // Creating a Dietician
                var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                dietician.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 12;
                });
                await contentManager.CreateAsync(dietician);

                // Creating a DiabetesNurseEducator
                var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 13;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(diabetesNurseEducator);

                colleaguesAndLeader.Add(leader);
                colleaguesAndLeader.Add(doctor);
                colleaguesAndLeader.Add(dietician);
                colleaguesAndLeader.Add(diabetesNurseEducator);


                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>()
                    {
                        new CenterProfileEquipment<int?>() { Id = "1", Value = 1 },
                        new CenterProfileEquipment<int?>() { Id = "2", Value = 5 },
                        new CenterProfileEquipment<int?>() { Id = "3", Value = 7 },
                        new CenterProfileEquipment<int?>() { Id = "4", Value = 4 },

                        new CenterProfileEquipment<int?>() { Id = "5", Value = 0 },
                        new CenterProfileEquipment<int?>() { Id = "6", Value = null },
                    },
                    Laboratory = new List<CenterProfileEquipment<bool>>()
                    {
                        new CenterProfileEquipment<bool>() { Id = "10", Value = true },
                        new CenterProfileEquipment<bool>() { Id = "11", Value = true },

                        new CenterProfileEquipment<bool>() { Id = "12", Value = false }
                    },
                    Colleagues = new List<Colleague>()
                    {
                        new Colleague()
                        {
                            Occupation = Occupation.Doctor,
                            MemberRightId = 11,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.Dietician,
                            MemberRightId = 12,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            MemberRightId = 13,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                            }
                        }
                    },
                    BackgroundConcilium = true,
                    BackgroundInpatient = true
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(new[]
                    {
                        new DokiNetMember() { MemberRightId = 10, DiabetLicenceNumber = "A", IsDueOk = true, HasMemberShip = true },
                        new DokiNetMember() { MemberRightId = 11, DiabetLicenceNumber = "B", IsDueOk = true, HasMemberShip = true },
                        new DokiNetMember() { MemberRightId = 12, DiabetLicenceNumber = "C", IsDueOk = true, HasMemberShip = true },
                        new DokiNetMember() { MemberRightId = 13, DiabetLicenceNumber = "D", IsDueOk = true, HasMemberShip = true },
                    }),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);

                // Assert
                Assert.Equal(AccreditationStatus.Registered, accreditationStatusResult.AccreditationStatus);
                Assert.NotEmpty(accreditationStatusResult.PersonalConditions);

                Assert.False(accreditationStatusResult.BackgroundConcilium);
                Assert.False(accreditationStatusResult.BackgroundInpatient);
                Assert.Empty(accreditationStatusResult.Laboratory);
                Assert.False(accreditationStatusResult.MdtLicence);
                Assert.Empty(accreditationStatusResult.Membership);
                Assert.Empty(accreditationStatusResult.MembershipFee);
                Assert.Empty(accreditationStatusResult.Tools);
            });
        }

        [Fact]
        public async Task CalculateAccreditationStatus_ShouldThrown_ArgumentNullException()
        {
            var service = new AccreditationStatusCalculator(null, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CalculateAccreditationStatusAsync(null));
        }

        [Theory]
        [InlineData("", AccreditationStatus.Registered)]
        [InlineData("X", AccreditationStatus.Accredited)]
        public async Task CalculateAccreditationStatus_LicenseCheck_ShouldReturnAsExpected(string licence, AccreditationStatus expectedResult)
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                //siteSettings.Alter<CenterProfileEquipmentsSettings>();

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);

                // Creating a Doctor
                var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                doctor.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 11;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(doctor);

                // Creating a Dietician
                var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                dietician.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 12;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qB.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(dietician);

                // Creating a DiabetesNurseEducator
                var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 13;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(diabetesNurseEducator);

                colleaguesAndLeader.Add(leader);
                colleaguesAndLeader.Add(doctor);
                colleaguesAndLeader.Add(dietician);
                colleaguesAndLeader.Add(diabetesNurseEducator);


                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>()
                    {
                        new CenterProfileEquipment<int?>() { Id = "1", Value = 1 },
                        new CenterProfileEquipment<int?>() { Id = "2", Value = 5 },
                        new CenterProfileEquipment<int?>() { Id = "3", Value = 7 },
                        new CenterProfileEquipment<int?>() { Id = "4", Value = 4 },

                        new CenterProfileEquipment<int?>() { Id = "5", Value = 0 },
                        new CenterProfileEquipment<int?>() { Id = "6", Value = null },
                    },
                    Laboratory = new List<CenterProfileEquipment<bool>>()
                    {
                        new CenterProfileEquipment<bool>() { Id = "10", Value = true },
                        new CenterProfileEquipment<bool>() { Id = "11", Value = true },

                        new CenterProfileEquipment<bool>() { Id = "12", Value = false }
                    },
                    Colleagues = new List<Colleague>()
                    {
                        new Colleague()
                        {
                            Occupation = Occupation.Doctor,
                            MemberRightId = 11,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.Dietician,
                            MemberRightId = 12,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            MemberRightId = 13,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                            }
                        }
                    },
                    BackgroundConcilium = true,
                    BackgroundInpatient = true
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(new[]
                    {
                        // no license
                        new DokiNetMember() { MemberRightId = 10, DiabetLicenceNumber = licence, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 11, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 12, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 13, HasMemberShip = true, IsDueOk = true }
                    }),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);

                // Assert
                Assert.Equal(expectedResult, accreditationStatusResult.AccreditationStatus);
                Assert.Equal(string.IsNullOrEmpty(licence), accreditationStatusResult.MdtLicence);

                Assert.False(accreditationStatusResult.BackgroundConcilium);
                Assert.False(accreditationStatusResult.BackgroundInpatient);
                Assert.Empty(accreditationStatusResult.Laboratory);
                Assert.Empty(accreditationStatusResult.Membership);
                Assert.Empty(accreditationStatusResult.MembershipFee);
                Assert.Empty(accreditationStatusResult.PersonalConditions);
                Assert.Empty(accreditationStatusResult.Tools);

            });
        }

        [Theory]
        [InlineData(false, false, AccreditationStatus.Registered)]
        [InlineData(false, true, AccreditationStatus.Registered)]
        [InlineData(true, false, AccreditationStatus.Registered)]
        [InlineData(true, true, AccreditationStatus.Accredited)]
        public async Task CalculateAccreditationStatus_MembershipAndFee_ShouldReturn_AsExpected(
            bool membership,
            bool membershipFee,
            AccreditationStatus expectedResult)
        {
            await RequestSessionsAsync(async session =>
            {
                // Arrange
                var siteService = new SiteServiceMock();

                var siteSettings = await siteService.GetSiteSettingsAsync();

                // Equipment settings
                siteSettings.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), x =>
                {
                    x.LaboratoryList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "10", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "11", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "12", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "13", Required = false }
                    };

                    x.ToolsList = new List<CenterProfileEquipmentSetting>()
                    {
                        new CenterProfileEquipmentSetting() { Id = "1", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "2", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "3", Required = true },
                        new CenterProfileEquipmentSetting() { Id = "4", Required = true },

                        new CenterProfileEquipmentSetting() { Id = "5", Required = false },
                        new CenterProfileEquipmentSetting() { Id = "6", Required = false }
                    };
                });

                // Personal condition settings
                siteSettings.Alter<AccreditationPersonalConditionSettings>(nameof(AccreditationPersonalConditionSettings), x =>
                {
                    x.Accredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };

                    x.TemporarilyAccredited = new List<PersonalCondition>()
                    {
                        new PersonalCondition() { Occupation = Occupation.Doctor, HeadCount = 2 },
                        new PersonalCondition() { Occupation = Occupation.Dietician, HeadCount = 1 },
                        new PersonalCondition() { Occupation = Occupation.DiabetesNurseEducator, HeadCount = 1 }
                    };
                });

                await siteService.UpdateSiteSettingsAsync(siteSettings);

                // Add some qualifications
                var qualificationService = new QualificationService(siteService);
                // For doctors
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

                // For Dietician
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });

                // For DiabetesNurseEducator
                await qualificationService.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });

                var qualificationSettings = await qualificationService.GetQualificationSettingsAsync();

                var qA = qualificationSettings.Qualifications.First(x => x.Name == "A");
                var qB = qualificationSettings.Qualifications.First(x => x.Name == "B");
                var qC = qualificationSettings.Qualifications.First(x => x.Name == "C");
                var qD = qualificationSettings.Qualifications.First(x => x.Name == "D");

                // Assing qualifications to occupations
                await qualificationService.UpdateQualificationsPerOccupationsAsync(new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qA.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Doctor,
                            QualificationId = qD.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.Dietician,
                            QualificationId =  qB.Id,
                            IsSelected = true
                        },
                        new QualificationPerOccupationViewModel()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            QualificationId = qC.Id,
                            IsSelected = true
                        }
                    }
                });

                var colleaguesAndLeader = new List<ContentItem>();
                var contentManager = new ContentManagerMock(session);

                // Creating leader
                var leader = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                leader.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 10;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qA.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(leader);

                // Creating a Doctor
                var doctor = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                doctor.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 11;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qD.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(doctor);

                // Creating a Dietician
                var dietician = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                dietician.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 12;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qB.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(dietician);

                // Creating a DiabetesNurseEducator
                var diabetesNurseEducator = await contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                diabetesNurseEducator.Alter<DiabetesUserProfilePart>(p =>
                {
                    p.MemberRightId = 13;
                    p.Qualifications = new[]
                    {
                        new PersonQualification() { QualificationId = qC.Id, State = QualificationState.Obtained }
                    };
                });
                await contentManager.CreateAsync(diabetesNurseEducator);

                colleaguesAndLeader.Add(leader);
                colleaguesAndLeader.Add(doctor);
                colleaguesAndLeader.Add(dietician);
                colleaguesAndLeader.Add(diabetesNurseEducator);


                var part = new CenterProfilePart()
                {
                    MemberRightId = 10,
                    Tools = new List<CenterProfileEquipment<int?>>()
                    {
                        new CenterProfileEquipment<int?>() { Id = "1", Value = 1 },
                        new CenterProfileEquipment<int?>() { Id = "2", Value = 5 },
                        new CenterProfileEquipment<int?>() { Id = "3", Value = 7 },
                        new CenterProfileEquipment<int?>() { Id = "4", Value = 4 },

                        new CenterProfileEquipment<int?>() { Id = "5", Value = 0 },
                        new CenterProfileEquipment<int?>() { Id = "6", Value = null },
                    },
                    Laboratory = new List<CenterProfileEquipment<bool>>()
                    {
                        new CenterProfileEquipment<bool>() { Id = "10", Value = true },
                        new CenterProfileEquipment<bool>() { Id = "11", Value = true },

                        new CenterProfileEquipment<bool>() { Id = "12", Value = false }
                    },
                    Colleagues = new List<Colleague>()
                    {
                        new Colleague()
                        {
                            Occupation = Occupation.Doctor,
                            MemberRightId = 11,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.Dietician,
                            MemberRightId = 12,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.InvitationAccepted }
                            }
                        },
                        new Colleague()
                        {
                            Occupation = Occupation.DiabetesNurseEducator,
                            MemberRightId = 13,
                            StatusHistory = new List<ColleagueStatusItem>()
                            {
                                new ColleagueStatusItem() { Status = ColleagueStatus.PreExisting }
                            }
                        }
                    },
                    BackgroundConcilium = true,
                    BackgroundInpatient = true
                };

                var diabetesUserProfileService = new DiabetesUserProfileService_ForAccreditationStatusCalculator(colleaguesAndLeader);

                var service = new AccreditationStatusCalculator(
                    diabetesUserProfileService,
                    new DokiNetServiceMock_ForAccreditationStatusCalculator(new[]
                    {
                        new DokiNetMember() { MemberRightId = 10, HasMemberShip = membership, IsDueOk = membershipFee, DiabetLicenceNumber = "XXX" },
                        new DokiNetMember() { MemberRightId = 11, HasMemberShip = true, IsDueOk = true, DiabetLicenceNumber = "ABC" },
                        new DokiNetMember() { MemberRightId = 12, HasMemberShip = true, IsDueOk = true },
                        new DokiNetMember() { MemberRightId = 13, HasMemberShip = true, IsDueOk = true }
                    }),
                    siteService);

                // Act
                var accreditationStatusResult = await service.CalculateAccreditationStatusAsync(part);
                if (membership)
                {
                    Assert.Empty(accreditationStatusResult.Membership);
                }
                else
                {
                    Assert.NotEmpty(accreditationStatusResult.Membership);
                }

                // Assert
                Assert.Equal(expectedResult, accreditationStatusResult.AccreditationStatus);

                Assert.False(accreditationStatusResult.BackgroundConcilium);
                Assert.False(accreditationStatusResult.BackgroundInpatient);
                Assert.Empty(accreditationStatusResult.Laboratory);
                Assert.False(accreditationStatusResult.MdtLicence);
                Assert.Empty(accreditationStatusResult.PersonalConditions);
                Assert.Empty(accreditationStatusResult.Tools);
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
