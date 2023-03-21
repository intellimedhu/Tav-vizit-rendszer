using OrchardCore.ContentManagement;
using System;
using System.Threading.Tasks;

namespace IntelliMed.Core.Extensions
{
    public static class ContentManagerExtensions
    {
        public static async Task<ContentItem> GetAsync(this IContentManager contentManager, string contentItemId, string contentType)
        {
            ThrowIfEmpty(contentItemId, contentType);

            return ContentItemWithType(await contentManager.GetAsync(contentItemId), contentType);

        }

        public static async Task<ContentItem> GetVersionAsync(
            this IContentManager contentManager,
            string contentItemId,
            string contentItemVersionId,
            string contentType)
        {
            ThrowIfEmpty(contentItemVersionId, contentType);

            var contentItem = await contentManager.GetVersionAsync(contentItemVersionId);

            return contentItem == null || contentItem.ContentItemId != contentItemId || contentItem.ContentType != contentType
                ? null
                : contentItem;
        }

        public static async Task<ContentItem> GetNewVersionAsync(this IContentManager contentManager, string contentItemId, string contentType)
        {
            ThrowIfEmpty(contentItemId, contentType);

            return ContentItemWithType(await contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired), contentType);
        }


        private static void ThrowIfEmpty(string contentItemId, string contentType)
        {
            if (string.IsNullOrEmpty(contentItemId))
            {
                throw new ArgumentException(nameof(contentItemId));
            }

            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentException(nameof(contentType));
            }
        }

        private static ContentItem ContentItemWithType(ContentItem contentItem, string contentType)
            => contentItem == null || contentItem.ContentType != contentType ? null : contentItem;
    }
}
