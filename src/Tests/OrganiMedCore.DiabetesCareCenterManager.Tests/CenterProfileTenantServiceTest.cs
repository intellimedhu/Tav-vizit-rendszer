using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Cache;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
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
    public class CenterProfileTenantServiceTest
    {
        [Fact]
        public async Task GetCenterProfilesForTenantAsync()
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var cp1 = await centerProfileService.NewCenterProfileAsync(2000, true);
                var cp2 = await centerProfileService.NewCenterProfileAsync(2005, true);
                var cp3 = await centerProfileService.NewCenterProfileAsync(2010, true);

                var tenantName = "DCC1";

                // Create the first two ones
                await centerProfileService.SaveCenterProfileAsync(cp1, true);
                await centerProfileService.SaveCenterProfileAsync(cp2, true);
                await centerProfileService.SaveCenterProfileAsync(cp3, false);

                await service.SetCenterProfileAssignmentAsync(cp1.ContentItemId, tenantName);

                var centerProfiles = await service.GetCenterProfilesForTenantAsync(tenantName);

                Assert.True(centerProfiles.All(x => x.Latest && x.Published));
                Assert.True(centerProfiles.All(x => x.As<CenterProfilePart>().Created));
                Assert.True(centerProfiles.All(x =>
                {
                    var assignedTenantName = x.As<CenterProfileManagerExtensionsPart>().AssignedTenantName;

                    return string.IsNullOrEmpty(assignedTenantName) || assignedTenantName == tenantName;
                }));
            });
        }

        [Fact]
        public async Task SetCenterProfileAssignmentAsync()
        {
            const string tenantName = "DCC1";

            await RequestSessionsAsync(
                async session =>
                {
                    var clock = new ClockMock();
                    var contentManager = new ContentManagerMock(session);

                    var centerProfileService = new CenterProfileService(
                        new AccreditationStatusCalculatorMock(),
                        new BetterUserServiceMock(99),
                        clock,
                        contentManager,
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        new SiteServiceMock());

                    var service = new CenterProfileTenantService(
                        null,
                        centerProfileService,
                        clock,
                        contentManager,
                        new DokiNetServiceMock(),
                        session);

                    var cp1 = await centerProfileService.NewCenterProfileAsync(2000, true);
                    var cp2 = await centerProfileService.NewCenterProfileAsync(2005, true);
                    var cp3 = await centerProfileService.NewCenterProfileAsync(2010, true);

                    // Create the first two ones
                    await centerProfileService.SaveCenterProfileAsync(cp1, true);
                    await centerProfileService.SaveCenterProfileAsync(cp2, true);
                    await centerProfileService.SaveCenterProfileAsync(cp3, false);

                    // Set up relation
                    await service.SetCenterProfileAssignmentAsync(cp2.ContentItemId, tenantName);

                    var centerProfiles = await service.GetCenterProfilesForTenantAsync(tenantName);
                    Assert.Contains(centerProfiles, x => x.As<CenterProfileManagerExtensionsPart>().AssignedTenantName == tenantName);

                    // Remove relation
                    await service.SetCenterProfileAssignmentAsync(cp2.ContentItemId, null);
                },
                async session =>
                {
                    var clock = new ClockMock();
                    var contentManager = new ContentManagerMock(session);

                    var centerProfileService = new CenterProfileService(
                        new AccreditationStatusCalculatorMock(),
                        new BetterUserServiceMock(99),
                        clock,
                        contentManager,
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        new SiteServiceMock());

                    var service = new CenterProfileTenantService(
                        null,
                        centerProfileService,
                        clock,
                        contentManager,
                        new DokiNetServiceMock(),
                        session);

                    var centerProfiles = await service.GetCenterProfilesForTenantAsync(tenantName);
                    Assert.DoesNotContain(centerProfiles, x => x.As<CenterProfileManagerExtensionsPart>().AssignedTenantName == tenantName);
                });
        }

        [Fact]
        public async Task GetCenterProfileAssignedToTenantAsync()
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var cp1 = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(cp1, true);

                var tenantName = "DCC1";

                // Set up relation
                await service.SetCenterProfileAssignmentAsync(cp1.ContentItemId, tenantName);

                var cp = await service.GetCenterProfileAssignedToTenantAsync(tenantName);

                Assert.Equal(cp1.ContentItemId, cp1.ContentItemId);
                await Assert.ThrowsAsync<CenterProfileNotAssignedException>(() => service.GetCenterProfileAssignedToTenantAsync("Non-existng"));
                await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetCenterProfileAssignedToTenantAsync(null));
            });
        }

        [Fact]
        public async Task UpdateCenterProfileAsync()
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                // Set up relation
                var tenantName = "DCC2";
                await service.SetCenterProfileAssignmentAsync(contentItem.ContentItemId, tenantName);

                // Init view model
                var centerName = "DCC3 Center name";
                var basicData = new CenterProfileBasicDataViewModel() { CenterName = centerName };
                await service.UpdateCenterProfileAsync(tenantName, basicData);

                var centerProfile = await service.GetCenterProfileAssignedToTenantAsync(tenantName);

                Assert.Equal(centerName, centerProfile.As<CenterProfilePart>().CenterName);
            });
        }

        [Theory]
        [InlineData(null)]
        [InlineData("this is an invalid email")]
        public async Task InviteColleagueAsync_ShouldThrowColleagueException(string email)
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                await Assert.ThrowsAsync<ColleagueException>(() => service.InviteColleagueAsync(
                   contentItem,
                   new CenterProfileColleagueViewModel() { Email = email }));
            });
        }

        [Fact]
        public async Task InviteColleagueAsync_InviteNew_ShouldThrowException()
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                var gkovi = await new DokiNetServiceMock().GetDokiNetMemberById<DokiNetMember>(500);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);
                await service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                {
                    MemberRightId = gkovi.MemberRightId,
                    Email = gkovi.Emails.First()
                });

                // Invite a non-member with the same email
                await Assert.ThrowsAsync<ColleagueEmailAlreadyTakenException>(() =>
                    service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                    {
                        Email = gkovi.Emails.First()
                    }));

                // Invite a member again
                await Assert.ThrowsAsync<ColleagueAlreadyExistsException>(() =>
                    service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                    {
                        MemberRightId = gkovi.MemberRightId,
                        Email = gkovi.Emails.First() + "c"
                    }));
            });
        }

        [Fact]
        public async Task InviteColleagueAsync_InviteNew()
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                // Invite a member (member's email is: "gkovi@nodomain.wq")
                var colleague = await service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                {
                    MemberRightId = 500,
                    Email = "gkovi@nodomain.wq",
                    Occupation = Occupation.Dietician
                });

                Assert.NotEqual(default(Guid), colleague.Id);
                Assert.Equal(contentItem.ContentItemId, colleague.CenterProfileContentItemId);
                Assert.Equal(contentItem.ContentItemVersionId, colleague.CenterProfileContentItemVersionId);
                Assert.Equal(Occupation.Dietician, colleague.Occupation);
                Assert.Equal(ColleagueStatus.Invited, colleague.LatestStatusItem.Status);
                Assert.Contains(contentItem.As<CenterProfilePart>().Colleagues, x => x.Id == colleague.Id);
            });
        }

        [Fact]
        public async Task InviteColleagueAsync_InviteExistingAgain_ShouldThrowColleagueNotFoundException()
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                // Invite a member (member's email is: "gkovi@nodomain.wq")
                var colleague1 = await service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                {
                    //MemberRightId = 500,
                    Email = "gkovi@nodomain.wq",
                    Occupation = Occupation.Dietician
                });

                var colleague2 = await service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                {
                    Email = "apple@lemon.cc",
                    Occupation = Occupation.Nurse
                });

                await Assert.ThrowsAsync<ColleagueNotFoundException>(() =>
                    service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                    {
                        Id = Guid.Empty.ToString(),
                        //MemberRightId = 500,
                        Email = "gkovi@nodomain.wq",
                        Occupation = Occupation.Dietician
                    }));

                await Assert.ThrowsAsync<ColleagueEmailAlreadyTakenException>(() =>
                    service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                    {
                        Id = colleague2.Id.ToString(),
                        //MemberRightId = 500,
                        Email = "gkovi@nodomain.wq",
                        Occupation = Occupation.Doctor
                    }));
            });
        }

        [Fact]
        public async Task InviteColleagueAsync_InviteExistingAgain()
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                var colleague = await service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                {
                    Email = "gkovi@nodomain.wq",
                    Occupation = Occupation.Dietician
                });

                colleague = await service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                {
                    Id = colleague.Id.ToString(),
                    Email = "gkovi2@gmail.com",
                    Occupation = Occupation.Nurse
                });

                contentItem = await centerProfileService.GetCenterProfileAsync(contentItem.ContentItemId);

                Assert.Equal(1, contentItem.As<CenterProfilePart>().Colleagues.Count);
                Assert.Equal(Occupation.Nurse, contentItem.As<CenterProfilePart>().Colleagues.First().Occupation);
                Assert.Equal("gkovi2@gmail.com", contentItem.As<CenterProfilePart>().Colleagues.First().Email);
            });
        }

        [Theory]
        [InlineData(ColleagueStatus.ApplicationAccepted)]
        [InlineData(ColleagueStatus.PreExisting)]
        [InlineData(ColleagueStatus.ApplicationSubmitted)]
        [InlineData(ColleagueStatus.InvitationAccepted)]
        public async Task InviteColleagueAsync_InviteExistingAgain_ShouldThrowColleagueException(ColleagueStatus latestStatus)
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                var colleague = await service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                {
                    Email = "gkovi@nodomain.wq",
                    Occupation = Occupation.Dietician
                });

                colleague.StatusHistory.Add(new ColleagueStatusItem()
                {
                    Status = latestStatus,
                    StatusDateUtc = DateTime.Now
                });
                contentItem = await centerProfileService.GetCenterProfileAsync(contentItem.ContentItemId);
                contentItem.Alter<CenterProfilePart>(part =>
                {
                    part.Colleagues.Clear();
                    part.Colleagues.Add(colleague);
                });

                await contentManager.UpdateAsync(contentItem);

                await Assert.ThrowsAsync<ColleagueException>(() =>
                    service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                    {
                        Id = colleague.Id.ToString(),
                        Email = "gkovi2@gmail.com",
                        Occupation = Occupation.DiabetesNurseEducator
                    }));
            });
        }

        [Fact]
        public async Task RequireCenterProfileContentItemInNewRenewalProcessAsync()
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var expectedStatus = AccreditationStatus.TemporarilyAccredited;

                var service = new CenterProfileTenantService(
                    new AccreditationStatusCalculatorMock(expectedStatus),
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                // Submit
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                var c1 = await service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                {
                    Email = "no.email@ksle.se"
                });

                var c2 = await service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                {
                    Email = "no.email2@ksle.se"
                });

                var c3 = await service.InviteColleagueAsync(contentItem, new CenterProfileColleagueViewModel()
                {
                    Email = "no.email3@ksle.se"
                });

                contentItem.Alter<CenterProfilePart>(part =>
                {
                    foreach (var colleague in part.Colleagues)
                    {
                        colleague.StatusHistory.Add(new ColleagueStatusItem()
                        {
                            Status = colleague.Id == c1.Id
                                ? ColleagueStatus.InvitationAccepted
                                : ColleagueStatus.InvitationRejected,
                            StatusDateUtc = DateTime.UtcNow
                        });
                    }
                });

                await contentManager.UpdateAsync(contentItem);

                contentItem = await centerProfileService.GetCenterProfileAsync(contentItem.ContentItemId);

                var contentItemNewVersion = await service.RequireCenterProfileContentItemInNewRenewalProcessAsync(contentItem, false);
                var partMg = contentItemNewVersion.As<CenterProfileManagerExtensionsPart>();

                Assert.All(
                    contentItem.As<CenterProfilePart>().Colleagues,
                    c => ColleagueStatusExtensions.GreenZone.Contains(c.LatestStatusItem.Status));

                Assert.Equal(CenterProfileStatus.Unsubmitted, partMg.RenewalCenterProfileStatus);
                Assert.Equal(expectedStatus, partMg.RenewalAccreditationStatus);
            });
        }

        [Fact]
        public async Task ExecuteColleagueActionAsync_ShouldThrownColleagueNotFoundException1()
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                // Submit
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                await Assert.ThrowsAsync<ColleagueNotFoundException>(() =>
                    service.ExecuteColleagueActionAsync(contentItem, new CenterProfileColleagueActionViewModel()
                    {
                        ColleagueId = "this is not Guid"
                    }));
            });
        }

        [Fact]
        public async Task ExecuteColleagueActionAsync_ShouldThrownColleagueNotFoundException2()
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                // Submit
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                await Assert.ThrowsAsync<ColleagueNotFoundException>(() =>
                    service.ExecuteColleagueActionAsync(contentItem, new CenterProfileColleagueActionViewModel()
                    {
                        ColleagueId = Guid.Empty.ToString()
                    }));
            });
        }

        [Theory]
        [InlineData(ColleagueAction.RemoveActive, ColleagueStatus.DeletedByLeader, ColleagueStatus.ApplicationAccepted)]
        [InlineData(ColleagueAction.RemoveActive, ColleagueStatus.DeletedByLeader, ColleagueStatus.PreExisting)]
        [InlineData(ColleagueAction.RemoveActive, ColleagueStatus.DeletedByLeader, ColleagueStatus.InvitationAccepted)]
        [InlineData(ColleagueAction.AcceptApplication, ColleagueStatus.ApplicationAccepted, ColleagueStatus.ApplicationSubmitted)]
        [InlineData(ColleagueAction.RejectApplication, ColleagueStatus.ApplicationRejected, ColleagueStatus.ApplicationSubmitted)]
        [InlineData(ColleagueAction.ResendInvitation, ColleagueStatus.Invited, ColleagueStatus.Invited)]
        [InlineData(ColleagueAction.ResendInvitation, ColleagueStatus.Invited, ColleagueStatus.InvitationRejected)]
        [InlineData(ColleagueAction.ResendInvitation, ColleagueStatus.Invited, ColleagueStatus.ApplicationCancelled)]
        [InlineData(ColleagueAction.CancelInvitation, ColleagueStatus.InvitationCancelled, ColleagueStatus.Invited)]
        public async Task ExecuteColleagueActionAsync_ShouldBeDone(
            ColleagueAction colleagueAction,
            ColleagueStatus colleagueStatusExpected,
            ColleagueStatus latestStatus)
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                // Submit
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                var colleagueIdAsString = "abc12300-0000-0000-0000-000000000000";
                var colleagueId = Guid.Parse(colleagueIdAsString);

                contentItem = await centerProfileService.GetCenterProfileAsync(contentItem.ContentItemId);
                contentItem.Alter<CenterProfilePart>(part =>
                {
                    part.Colleagues.Add(new Colleague()
                    {
                        Id = colleagueId,
                        CenterProfileContentItemId = contentItem.ContentItemId,
                        CenterProfileContentItemVersionId = contentItem.ContentItemVersionId,
                        Email = "test1@im.hu",
                        Occupation = Occupation.CommunityNurse,
                        StatusHistory = new List<ColleagueStatusItem>()
                        {
                            new ColleagueStatusItem() { Status = latestStatus, StatusDateUtc = clock.UtcNow }
                        }
                    });
                });

                await contentManager.UpdateAsync(contentItem);
                contentItem = await centerProfileService.GetCenterProfileAsync(contentItem.ContentItemId);

                await service.ExecuteColleagueActionAsync(
                    contentItem,
                    new CenterProfileColleagueActionViewModel()
                    {
                        ColleagueAction = colleagueAction,
                        ColleagueId = colleagueIdAsString
                    });

                contentItem = await centerProfileService.GetCenterProfileAsync(contentItem.ContentItemId);
                var colleague = contentItem.As<CenterProfilePart>().Colleagues.First(x => x.Id == colleagueId);

                Assert.Equal(colleagueStatusExpected, colleague.LatestStatusItem.Status);
            });
        }

        [Theory]
        [InlineData(ColleagueAction.RemoveActive, ColleagueStatus.ApplicationSubmitted)]
        [InlineData(ColleagueAction.RemoveActive, ColleagueStatus.Invited)]
        [InlineData(ColleagueAction.RemoveActive, ColleagueStatus.DeletedByLeader)]
        [InlineData(ColleagueAction.RemoveActive, ColleagueStatus.InvitationRejected)]
        [InlineData(ColleagueAction.RemoveActive, ColleagueStatus.DeletedByColleague)]
        [InlineData(ColleagueAction.RemoveActive, ColleagueStatus.ApplicationRejected)]
        [InlineData(ColleagueAction.RemoveActive, ColleagueStatus.InvitationCancelled)]
        [InlineData(ColleagueAction.RemoveActive, ColleagueStatus.ApplicationCancelled)]

        [InlineData(ColleagueAction.AcceptApplication, ColleagueStatus.ApplicationAccepted)]
        [InlineData(ColleagueAction.AcceptApplication, ColleagueStatus.PreExisting)]
        [InlineData(ColleagueAction.AcceptApplication, ColleagueStatus.InvitationAccepted)]
        [InlineData(ColleagueAction.AcceptApplication, ColleagueStatus.Invited)]
        [InlineData(ColleagueAction.AcceptApplication, ColleagueStatus.DeletedByLeader)]
        [InlineData(ColleagueAction.AcceptApplication, ColleagueStatus.InvitationRejected)]
        [InlineData(ColleagueAction.AcceptApplication, ColleagueStatus.DeletedByColleague)]
        [InlineData(ColleagueAction.AcceptApplication, ColleagueStatus.ApplicationRejected)]
        [InlineData(ColleagueAction.AcceptApplication, ColleagueStatus.InvitationCancelled)]
        [InlineData(ColleagueAction.AcceptApplication, ColleagueStatus.ApplicationCancelled)]

        [InlineData(ColleagueAction.RejectApplication, ColleagueStatus.ApplicationAccepted)]
        [InlineData(ColleagueAction.RejectApplication, ColleagueStatus.PreExisting)]
        [InlineData(ColleagueAction.RejectApplication, ColleagueStatus.InvitationAccepted)]
        [InlineData(ColleagueAction.RejectApplication, ColleagueStatus.Invited)]
        [InlineData(ColleagueAction.RejectApplication, ColleagueStatus.DeletedByLeader)]
        [InlineData(ColleagueAction.RejectApplication, ColleagueStatus.InvitationRejected)]
        [InlineData(ColleagueAction.RejectApplication, ColleagueStatus.DeletedByColleague)]
        [InlineData(ColleagueAction.RejectApplication, ColleagueStatus.ApplicationRejected)]
        [InlineData(ColleagueAction.RejectApplication, ColleagueStatus.InvitationCancelled)]
        [InlineData(ColleagueAction.RejectApplication, ColleagueStatus.ApplicationCancelled)]

        [InlineData(ColleagueAction.ResendInvitation, ColleagueStatus.ApplicationAccepted)]
        [InlineData(ColleagueAction.ResendInvitation, ColleagueStatus.PreExisting)]
        [InlineData(ColleagueAction.ResendInvitation, ColleagueStatus.InvitationAccepted)]
        [InlineData(ColleagueAction.ResendInvitation, ColleagueStatus.ApplicationSubmitted)]
        [InlineData(ColleagueAction.ResendInvitation, ColleagueStatus.DeletedByLeader)]
        [InlineData(ColleagueAction.ResendInvitation, ColleagueStatus.DeletedByColleague)]
        [InlineData(ColleagueAction.ResendInvitation, ColleagueStatus.ApplicationRejected)]
        [InlineData(ColleagueAction.ResendInvitation, ColleagueStatus.InvitationCancelled)]

        [InlineData(ColleagueAction.CancelInvitation, ColleagueStatus.ApplicationAccepted)]
        [InlineData(ColleagueAction.CancelInvitation, ColleagueStatus.PreExisting)]
        [InlineData(ColleagueAction.CancelInvitation, ColleagueStatus.InvitationAccepted)]
        [InlineData(ColleagueAction.CancelInvitation, ColleagueStatus.ApplicationSubmitted)]
        [InlineData(ColleagueAction.CancelInvitation, ColleagueStatus.DeletedByLeader)]
        [InlineData(ColleagueAction.CancelInvitation, ColleagueStatus.InvitationRejected)]
        [InlineData(ColleagueAction.CancelInvitation, ColleagueStatus.DeletedByColleague)]
        [InlineData(ColleagueAction.CancelInvitation, ColleagueStatus.ApplicationRejected)]
        [InlineData(ColleagueAction.CancelInvitation, ColleagueStatus.InvitationCancelled)]
        [InlineData(ColleagueAction.CancelInvitation, ColleagueStatus.ApplicationCancelled)]

        public async Task ExecuteColleagueActionAsync_ShouldThrownColleagueException(ColleagueAction colleagueAction, ColleagueStatus latestStatus)
        {
            await RequestSessionsAsync(async session =>
            {
                var clock = new ClockMock();
                var contentManager = new ContentManagerMock(session);

                var centerProfileService = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(99),
                    clock,
                    contentManager,
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    new SiteServiceMock());

                var service = new CenterProfileTenantService(
                    null,
                    centerProfileService,
                    clock,
                    contentManager,
                    new DokiNetServiceMock(),
                    session);

                var contentItem = await centerProfileService.NewCenterProfileAsync(2000, true);

                // Create
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                // Submit
                await centerProfileService.SaveCenterProfileAsync(contentItem, true);

                var colleagueIdAsString = "abc12300-0000-0000-0000-000000000000";
                var colleagueId = Guid.Parse(colleagueIdAsString);

                contentItem = await centerProfileService.GetCenterProfileAsync(contentItem.ContentItemId);
                contentItem.Alter<CenterProfilePart>(part =>
                {
                    part.Colleagues.Add(new Colleague()
                    {
                        Id = colleagueId,
                        CenterProfileContentItemId = contentItem.ContentItemId,
                        CenterProfileContentItemVersionId = contentItem.ContentItemVersionId,
                        Email = "test1@im.hu",
                        Occupation = Occupation.CommunityNurse,
                        StatusHistory = new List<ColleagueStatusItem>()
                        {
                            new ColleagueStatusItem() { Status = latestStatus, StatusDateUtc = clock.UtcNow }
                        }
                    });
                });

                await contentManager.UpdateAsync(contentItem);
                contentItem = await centerProfileService.GetCenterProfileAsync(contentItem.ContentItemId);

                await Assert.ThrowsAsync<ColleagueException>(() => service.ExecuteColleagueActionAsync(
                    contentItem,
                    new CenterProfileColleagueActionViewModel()
                    {
                        ColleagueAction = colleagueAction,
                        ColleagueId = colleagueIdAsString
                    }));
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
                        CenterProfileSchemaBuilder.Build(schemaBuilder);
                        CenterProfileManagerExtensionSchemaBuilder.Build(schemaBuilder);
                        TerritorySchemaBuilder.Build(schemaBuilder);
                        SettlementSchemaBuilder.Build(schemaBuilder);
                    },
                    store =>
                    {
                        store.RegisterIndexes<ContentItemIndexProvider>();
                        store.RegisterIndexes<CenterProfilePartIndexProvider>();
                        store.RegisterIndexes<CenterProfileManagerExtensionsPartIndexProvider>();
                        store.RegisterIndexes<TerritoryIndexProvider>();
                        store.RegisterIndexes<SettlementIndexProvider>();
                    });
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }

        private IMemoryCache GetMemoryCache() => new MemoryCache(new MemoryCacheOptions());

        private ISignal GetSignal() => new Signal();
    }
}
