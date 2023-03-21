using IntelliMed.Core.Extensions;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrganiMedCore.UriAuthentication.Models;
using OrganiMedCore.UriAuthentication.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.UriAuthentication.Services
{
    public class NonceService : INonceService
    {
        private readonly IClock _clock;
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;
        private readonly ISiteService _siteService;
        private NonceSettings _nonceSettings;


        private const string NonceCacheKey = "NonceServiceDocument";


        public NonceService(
            IClock clock,
            IMemoryCache memoryCache,
            ISession session,
            ISignal signal,
            ISiteService siteService)
        {
            _clock = clock;
            _memoryCache = memoryCache;
            _session = session;
            _signal = signal;
            _siteService = siteService;
        }


        public Task CreateAsync(Nonce nonce)
            => CreateManyAsync(new[] { nonce });

        public async Task CreateManyAsync(IEnumerable<Nonce> nonces)
        {
            nonces.ThrowIfNull();
            if (!nonces.Any())
            {
                return;
            }

            var expirationDate = default(DateTime?);
            foreach (var nonce in nonces)
            {
                nonce.ThrowIfNull();
                nonce.RedirectUrl.ThrowIfNullOrEmpty();

                nonce.Value = Guid.NewGuid();
                if (!nonce.ExpirationDate.HasValue)
                {
                    if (!expirationDate.HasValue)
                    {
                        expirationDate = _clock.UtcNow.AddDays((await GetNonceSettingsAsync()).NonceExpirationInDays);
                    }

                    nonce.ExpirationDate = expirationDate;
                }
            }

            var document = await LoadNonceDocumentFromCacheAsync() ?? new NonceDocument();
            document.Nonces.AddRange(nonces);

            _session.Save(document);

            ClearNoncesCache();
        }

        public async Task<Nonce> GetByValue(Guid value)
        {
            var document = await LoadNonceDocumentFromCacheAsync();
            var nonce = document?.Nonces.FirstOrDefault(x => x.Value == value);
            if (nonce == null || IsExpired(nonce))
            {
                return null;
            }

            return nonce;
        }

        public async Task<string> GetUriAsync(Guid value)
        {
            var baseUrl = (await _siteService.GetSiteSettingsAsync()).BaseUrl?.Trim();
            if (string.IsNullOrEmpty(baseUrl))
            {
                return null;
            }

            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            return baseUrl + $"nc/{value.ToString("N")}";
        }

        public async Task CleanupAsync()
        {
            var document = await LoadNonceDocumentFromCacheAsync();
            if (document == null)
            {
                return;
            }

            document.Nonces = document.Nonces
                .Where(nonce => !IsExpired(nonce))
                .ToList();

            _session.Save(document);

            ClearNoncesCache();
        }


        private async Task<NonceSettings> GetNonceSettingsAsync()
        {
            if (_nonceSettings == null)
            {
                _nonceSettings = (await _siteService.GetSiteSettingsAsync()).As<NonceSettings>();
            }

            return _nonceSettings;
        }

        private Task<NonceDocument> LoadNonceDocumentFromCacheAsync()
            => _memoryCache.GetOrCreateAsync("NonceService:NonceDocument", entry =>
            {
                entry.AddExpirationToken(_signal.GetToken(NonceCacheKey));

                return _session.Query<NonceDocument>().FirstOrDefaultAsync();
            });

        private void ClearNoncesCache() => _signal.SignalToken(NonceCacheKey);

        private bool IsExpired(Nonce nonce) => nonce.ExpirationDate < _clock.UtcNow;
    }
}
