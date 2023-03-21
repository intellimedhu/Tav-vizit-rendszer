using IntelliMed.Core.Extensions;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using System;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Services
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class CenterProfileInfoManagerService : ICenterProfileInfoService
    {
        private readonly IContentManager _contentManager;
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;


        private const string CacheKeyPattern = "CenterProfileInfoManagerService:{0}";
        private const string ContentItemCacheKeyPattern = "CenterProfileInfoManagerService:ContentItem:{0}";


        public CenterProfileInfoManagerService(
            IContentManager contentManager,
            IMemoryCache memoryCache,
            ISession session,
            ISignal signal)
        {
            _contentManager = contentManager;
            _memoryCache = memoryCache;
            _session = session;
            _signal = signal;
        }


        public bool AllowedContentType(string contentType)
            => contentType == ContentTypes.CenterProfileEditorInfoBlockContainer ||
            contentType == ContentTypes.CenterProfileOverviewContainerBlock;

        public Task<(ContentItem contentItem, bool isNew)> GetOrCreateNewContentItemAsync(string contentType)
        {
            contentType.ThrowIfNull();

            if (!AllowedContentType(contentType))
            {
                throw new ArgumentException($"Wrong content type: '{contentType}'");
            }

            return _memoryCache.GetOrCreateAsync(GetCacheKey(contentType), async entry =>
            {
                entry.AddExpirationToken(_signal.GetToken(GetContentItemCacheKey(contentType)));

                var contentItem = await _session
                   .Query<ContentItem>()
                   .LatestAndPublished()
                   .Where(index => index.ContentType == contentType)
                   .FirstOrDefaultAsync();

                var isNew = contentItem == null;
                if (isNew)
                {
                    contentItem = await _contentManager.NewAsync(contentType);
                }

                return (contentItem, isNew);
            });
        }

        public async Task SaveContentItemAsync(ContentItem contentItem, bool isNew)
        {
            contentItem.ThrowIfNull();

            if (!AllowedContentType(contentItem.ContentType))
            {
                throw new ArgumentException($"Wrong content type: '{contentItem.ContentType}'");
            }

            if (isNew)
            {
                await _contentManager.CreateAsync(contentItem);
            }
            else
            {
                await _contentManager.UpdateAsync(contentItem);
            }

            _signal.SignalToken(GetContentItemCacheKey(contentItem.ContentType));
        }


        private string GetCacheKey(string contentType)
            => $"{nameof(CenterProfileInfoManagerService)}.{contentType}";

        private string GetContentItemCacheKey(string contentType)
            => $"{nameof(CenterProfileInfoManagerService)}.{nameof(ContentItem.ContentType)}.{contentType}";
    }
}
