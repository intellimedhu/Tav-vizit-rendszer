using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Cache;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Services;
using OrganiMedCore.Testing.Core;
using OrganiMedCore.Testing.Core.SchemaBuilders;
using System;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Tests
{
    public class CenterProfileInfoServiceTests
    {
        [Theory]
        [InlineData(ContentTypes.CenterProfileEditorInfoBlockContainer)]
        [InlineData(ContentTypes.CenterProfileOverviewContainerBlock)]
        public async Task GetOrCreateNewContentItemAsync_ShouldCreateNew(string contentType)
        {
            await RequestSessionsAsync(
                async session =>
                {
                    var service = new CenterProfileInfoManagerService(
                        new ContentManagerMock(session),
                        GetCache(),
                        session,
                        GetSignal());

                    var (contentItem, isNew) = await service.GetOrCreateNewContentItemAsync(contentType);

                    Assert.True(isNew);
                    Assert.Equal(contentType, contentItem.ContentType);
                });
        }

        [Theory]
        [InlineData(ContentTypes.CenterProfileEditorInfoBlockContainer)]
        [InlineData(ContentTypes.CenterProfileOverviewContainerBlock)]
        public async Task GetOrCreateNewContentItemAsync_ShouldGetExisting(string contentType)
        {
            var contentItemId = string.Empty;

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);
                    var contentItem = await contentManager.NewAsync(contentType);
                    await contentManager.CreateAsync(contentItem, VersionOptions.Published);

                    contentItemId = contentItem.ContentItemId;
                },
                async session =>
                {
                    var service = new CenterProfileInfoManagerService(
                        new ContentManagerMock(session),
                        GetCache(),
                        session,
                        GetSignal());

                    var (contentItem, isNew) = await service.GetOrCreateNewContentItemAsync(contentType);

                    Assert.False(isNew);
                    Assert.Equal(contentType, contentItem.ContentType);
                    Assert.False(string.IsNullOrEmpty(contentItemId));
                    Assert.Equal(contentItemId, contentItem.ContentItemId);
                });
        }

        [Theory]
        [InlineData(true, ContentTypes.CenterProfileEditorInfoBlockContainer)]
        [InlineData(false, ContentTypes.CenterProfileEditorInfoBlockContainer)]
        [InlineData(true, ContentTypes.CenterProfileOverviewContainerBlock)]
        [InlineData(false, ContentTypes.CenterProfileOverviewContainerBlock)]
        public async Task SaveContentItemAsync_ShouldSave(bool shouldNotCreateBeforeSave, string contentType)
        {
            var cache = GetCache();
            var signal = GetSignal();

            var contentItemId = string.Empty;
            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);
                    var contentItem = await contentManager.NewAsync(contentType);
                    if (!shouldNotCreateBeforeSave)
                    {
                        await contentManager.CreateAsync(contentItem, VersionOptions.Published);
                    }

                    var service = new CenterProfileInfoManagerService(
                        contentManager,
                        cache,
                        session,
                        signal);

                    await service.SaveContentItemAsync(contentItem, shouldNotCreateBeforeSave);

                    contentItemId = contentItem.ContentItemId;
                },
                async session =>
                {
                    var service = new CenterProfileInfoManagerService(
                        new ContentManagerMock(session),
                        cache,
                        session,
                        signal);

                    var (contentItem, isNew) = await service.GetOrCreateNewContentItemAsync(contentType);

                    Assert.Equal(contentType, contentItem.ContentType);
                    Assert.False(string.IsNullOrEmpty(contentItem.ContentItemId));
                    Assert.Equal(contentItemId, contentItem.ContentItemId);
                });
        }

        [Theory]
        [InlineData(ContentTypes.CenterProfileEditorInfoBlockContainer, true)]
        [InlineData(ContentTypes.CenterProfileOverviewContainerBlock, true)]
        [InlineData(ContentTypes.AccreditationStatusBlock, false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void AllowedContentType_ShouldReturn_AsExpected(string contentType, bool expectedResult)
        {
            var service = new CenterProfileInfoManagerService(null, null, null, null);

            var result = service.AllowedContentType(contentType);

            Assert.Equal(expectedResult, result);
        }


        private IMemoryCache GetCache() => new MemoryCache(new MemoryCacheOptions());

        private ISignal GetSignal() => new Signal();

        private async Task RequestSessionsAsync(params Func<ISession, Task>[] sessions)
        {
            using (var sessionHandler = new YesSqlSessionHandler())
            {
                await sessionHandler.InitializeAsync();
                await sessionHandler.RegisterSchemaAndIndexes(
                    schemaBuilder =>
                    {
                        ContentItemSchemaBuilder.Build(schemaBuilder);
                    },
                    store =>
                    {
                        store.RegisterIndexes<ContentItemIndexProvider>();
                    });
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }
    }
}
