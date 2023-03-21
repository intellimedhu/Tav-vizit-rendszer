using IntelliMed.DokiNetIntegration.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Widgets.Services;
using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;
using OrganiMedCore.Testing.Core;
using OrganiMedCore.Testing.Core.SchemaBuilders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Tests
{
    public class ColleagueWorkplaceZoneServiceTests
    {
        [Fact]
        public void Default()
        {
            var expectedResult = new JObject()
            {
                ["GreenZone"] = false,
                ["PendingZone"] = false,
                ["RemovedZone"] = false
            };

            var service = new ColleagueWorkplaceZoneService();

            Assert.Equal(expectedResult, service.Default);
        }

        [Theory]
        // Green zone
        [InlineData(ColleagueStatus.ApplicationAccepted, true, false, false)]
        [InlineData(ColleagueStatus.InvitationAccepted, true, false, false)]
        [InlineData(ColleagueStatus.PreExisting, true, false, false)]

        // Pending zone
        [InlineData(ColleagueStatus.ApplicationSubmitted, false, true, false)]
        [InlineData(ColleagueStatus.Invited, false, true, false)]

        // Removed zone
        [InlineData(ColleagueStatus.ApplicationCancelled, false, false, true)]
        [InlineData(ColleagueStatus.ApplicationRejected, false, false, true)]
        [InlineData(ColleagueStatus.DeletedByColleague, false, false, true)]
        [InlineData(ColleagueStatus.DeletedByLeader, false, false, true)]
        [InlineData(ColleagueStatus.InvitationCancelled, false, false, true)]
        [InlineData(ColleagueStatus.InvitationRejected, false, false, true)]
        public async Task GetZones_ShouldReturn_AsExpected(ColleagueStatus colleagueStatus, bool greenZone, bool pendingZone, bool removedZone)
        {
            var contentItems = new ContentItem[1];
            const int memberRightId = 99;

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);
                    var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
                    contentItem.Alter<CenterProfilePart>(part => part.Colleagues.Add(new Colleague()
                    {
                        MemberRightId = memberRightId,
                        StatusHistory = new List<ColleagueStatusItem>()
                        {
                            new ColleagueStatusItem() { Status = colleagueStatus }
                        }
                    }));
                    await contentManager.CreateAsync(contentItem);

                    contentItems[0] = contentItem;
                });

            var service = new ColleagueWorkplaceZoneService();

            var expectedResult = new JObject()
            {
                ["GreenZone"] = greenZone,
                ["PendingZone"] = pendingZone,
                ["RemovedZone"] = removedZone
            };

            var result = service.GetZones(contentItems, new DokiNetMember() { MemberRightId = memberRightId });

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetZones_NoZone()
        {
            var contentItems = new ContentItem[1];
            const int memberRightId = 99;

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);
                    var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
                    contentItem.Alter<CenterProfilePart>(part => part.Colleagues.Add(new Colleague()
                    {
                        MemberRightId = memberRightId,
                        StatusHistory = new List<ColleagueStatusItem>()
                        {
                            new ColleagueStatusItem() { Status = ColleagueStatus.ApplicationAccepted }
                        }
                    }));
                    await contentManager.CreateAsync(contentItem);

                    contentItems[0] = contentItem;
                });

            var service = new ColleagueWorkplaceZoneService();

            var expectedResult = new JObject()
            {
                ["GreenZone"] = false,
                ["PendingZone"] = false,
                ["RemovedZone"] = false
            };

            var result = service.GetZones(contentItems, new DokiNetMember() { MemberRightId = memberRightId + 1 });

            Assert.Equal(expectedResult, result);
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
                    },
                    store =>
                    {
                        store.RegisterIndexes<ContentItemIndexProvider>();
                        store.RegisterIndexes<CenterProfilePartIndexProvider>();
                        store.RegisterIndexes<CenterProfileManagerExtensionsPartIndexProvider>();
                    });
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }
    }
}
