using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.Testing.Core
{
    public class ContentManagerMock : IContentManager
    {
        private readonly ISession _session;


        public ContentManagerMock(ISession session)
        {
            _session = session;
        }


        public Task<ContentItem> CloneAsync(ContentItem contentItem)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(ContentItem contentItem, VersionOptions options)
        {
            _session.Save(contentItem);

            return Task.CompletedTask;
        }

        public Task DiscardDraftAsync(ContentItem contentItem)
        {
            throw new NotImplementedException();
        }

        public async Task<ContentItem> GetAsync(string id)
        {
            return await _session
                .Query<ContentItem, ContentItemIndex>(x => x.ContentItemId == id)
                .LatestAndPublished()
                .FirstOrDefaultAsync();
        }

        public async Task<ContentItem> GetAsync(string id, VersionOptions options)
        {
            var query = _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId == id);

            if (options.IsDraftRequired)
            {
                var ci = await query.Where(x => x.Latest && x.Published).FirstOrDefaultAsync();
                if (ci == null)
                {
                    return null;
                }

                ci.Published = true;
                ci.Latest = false;

                var newCi = JsonConvert.DeserializeObject<ContentItem>(JsonConvert.SerializeObject(ci));
                newCi.ContentItemVersionId = Guid.NewGuid().ToString("N");
                //newCi.CreatedUtc = DateTime.UtcNow;
                newCi.Latest = true;
                newCi.ModifiedUtc = DateTime.UtcNow;
                newCi.PublishedUtc = DateTime.UtcNow;

                _session.Save(ci);
                _session.Save(newCi);

                return newCi;
            }

            if (options.IsLatest)
            {
                return await query.Where(x => x.Latest)
                    .OrderByDescending(x => x.ModifiedUtc)
                    .FirstOrDefaultAsync();
            }

            if (options.IsPublished)
            {
                return await query.Where(x => x.Published)
                    .OrderByDescending(x => x.PublishedUtc)
                    .FirstOrDefaultAsync();
            }

            if (options.IsDraft)
            {
                return await query.Where(x => x.Latest && !x.Published).FirstOrDefaultAsync();
            }

            return null;
        }

        public Task<IEnumerable<ContentItem>> GetAsync(IEnumerable<string> contentItemIds, bool latest = false)
        {
            throw new NotImplementedException();
        }

        public async Task<ContentItem> GetVersionAsync(string contentItemVersionId)
        {
            return await _session
                .Query<ContentItem, ContentItemIndex>(x => x.ContentItemVersionId == contentItemVersionId)
                .FirstOrDefaultAsync();
        }

        public Task<ContentItem> LoadAsync(ContentItem contentItem)
        {
            throw new NotImplementedException();
        }

        public Task<ContentItem> NewAsync(string contentType)
            => NewAsyncSpecifyCreateDate(contentType, DateTime.UtcNow);

        public Task<ContentItem> NewAsyncSpecifyCreateDate(string contentType, DateTime createUtc)
        {
            return Task.FromResult(new ContentItem()
            {
                Author = "author",
                ContentType = contentType,
                ContentItemId = Guid.NewGuid().ToString("N"),
                ContentItemVersionId = Guid.NewGuid().ToString("N"),
                CreatedUtc = createUtc,
                Latest = true,
                Published = true,
                ModifiedUtc = createUtc,
                Owner = "owner",
                PublishedUtc = createUtc
            });
        }

        public Task<TAspect> PopulateAspectAsync<TAspect>(IContent content, TAspect aspect)
        {
            throw new NotImplementedException();
        }

        public async Task PublishAsync(ContentItem contentItem)
        {
            var others = await _session.Query<ContentItem, ContentItemIndex>(x =>
                x.ContentItemId == contentItem.ContentItemId &&
                x.ContentItemVersionId != contentItem.ContentItemVersionId)
                .ListAsync();

            foreach (var ci in others)
            {
                ci.Latest = false;
                ci.Published = false;
                ci.ModifiedUtc = DateTime.UtcNow;

                _session.Save(ci);
            }

            contentItem.Latest = true;
            contentItem.Published = true;
            contentItem.PublishedUtc = DateTime.UtcNow;
            contentItem.ModifiedUtc = DateTime.UtcNow;

            _session.Save(contentItem);
        }

        public Task RemoveAsync(ContentItem contentItem)
        {
            _session.Delete(contentItem);

            return Task.CompletedTask;
        }

        public Task UnpublishAsync(ContentItem contentItem)
        {
            contentItem.Published = false;
            _session.Save(contentItem);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(ContentItem contentItem)
        {
            _session.Save(contentItem);

            return Task.CompletedTask;
        }
    }
}
