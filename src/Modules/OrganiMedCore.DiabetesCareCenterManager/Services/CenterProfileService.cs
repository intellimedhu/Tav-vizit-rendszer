using Dapper;
using IntelliMed.Core.Exceptions;
using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class CenterProfileService : ICenterProfileService
    {
        private readonly IAccreditationStatusCalculator _accreditationStatusCalculator;
        private readonly IBetterUserService _betterUserService;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;
        private readonly ISiteService _siteService;


        private const string CenterProfilesCacheKey = "CenterProfileService.CenterProfiles";


        public CenterProfileService(
            IAccreditationStatusCalculator accreditationStatusCalculator,
            IBetterUserService betterUserService,
            IClock clock,
            IContentManager contentManager,
            IMemoryCache memoryCache,
            ISession session,
            ISignal signal,
            ISiteService siteService)
        {
            _accreditationStatusCalculator = accreditationStatusCalculator;
            _betterUserService = betterUserService;
            _clock = clock;
            _contentManager = contentManager;
            _memoryCache = memoryCache;
            _session = session;
            _signal = signal;
            _siteService = siteService;
        }


        public async Task<bool> CacheEnabledAsync()
            => (await GetCenterManagerSettingsAsync()).CenterProfileCacheEnabled;

        public async Task<IEnumerable<ContentItem>> GetCenterProfilesAsync()
        {
            if (await CacheEnabledAsync())
            {
                return (await LoadCenterProfilesFromCacheAsync())
                    .Where(contentItem => contentItem.As<CenterProfilePart>().Created);
            }

            return await _session
                .Query<ContentItem, CenterProfilePartIndex>(index => index.Created)
                .LatestAndPublished()
                .ListAsync();
        }

        public async Task<ContentItem> GetCenterProfileToLeaderAsync(int memberRightId, string contentItemId)
        {
            if (await CacheEnabledAsync())
            {
                return (await LoadCenterProfilesFromCacheAsync())
                    .FirstOrDefault(item =>
                        item.ContentItemId == contentItemId &&
                        item.As<CenterProfilePart>().MemberRightId == memberRightId);
            }

            var contentItem = await GetCenterProfileAsync(contentItemId);
            if (contentItem == null || contentItem.As<CenterProfilePart>().MemberRightId != memberRightId)
            {
                return null;
            }

            return contentItem;
        }

        public async Task<IEnumerable<ContentItem>> GetCenterProfilesToLeaderAsync(int memberRightId)
        {
            if (await CacheEnabledAsync())
            {
                return (await LoadCenterProfilesFromCacheAsync())
                    .Where(contentItem => contentItem.As<CenterProfilePart>().MemberRightId == memberRightId);
            }

            return await _session
                .Query<ContentItem, CenterProfilePartIndex>(index => index.MemberRightId == memberRightId)
                .LatestAndPublished()
                .ListAsync();
        }

        public async Task<ContentItem> GetPermittedCenterProfileAsync(string id)
            => (await GetPermittedCenterProfileQueryAsync(id)).FirstOrDefault();

        public async Task<IEnumerable<ContentItem>> GetPermittedCenterProfilesAsync()
            => await GetPermittedCenterProfileQueryAsync();

        public async Task DeleteCenterProfileAsync(string contentItemId)
        {
            var cacheEnabled = await CacheEnabledAsync();

            var contentItem = cacheEnabled
                ? (await LoadCenterProfilesFromCacheAsync()).FirstOrDefault(x => x.ContentItemId == contentItemId)
                : await GetCenterProfileAsync(contentItemId);
            if (contentItem == null)
            {
                return;
            }

            await _contentManager.UnpublishAsync(contentItem);
            //await _contentManager.RemoveAsync(contentItem);

            if (cacheEnabled)
            {
                ClearCenterProfileCache();
            }
        }

        public async Task DeleteOwnCenterProfileAsync(string contentItemId, int memberRightId)
        {
            var contentItem = await GetCenterProfileAsync(contentItemId);
            if (contentItem == null)
            {
                throw new NotFoundException();
            }

            var part = contentItem.As<CenterProfilePart>();
            if (part.MemberRightId != memberRightId)
            {
                throw new NotFoundException();
            }

            if (part.Created)
            {
                throw new CenterProfileAreadyCreatedException();
            }

            await _contentManager.RemoveAsync(contentItem);

            if (await CacheEnabledAsync())
            {
                ClearCenterProfileCache();
            }
        }

        public async Task<ContentItem> GetCenterProfileAsync(string contentItemId)
            => await CacheEnabledAsync()
                ? (await LoadCenterProfilesFromCacheAsync()).FirstOrDefault(contentItem => contentItem.ContentItemId == contentItemId)
                : await _contentManager.GetAsync(contentItemId, ContentTypes.CenterProfile);

        // Not cached
        public async Task<IEnumerable<ContentItem>> GetCenterProfileVersionsAsync(string contentItemId)
            => await _session
                .Query<ContentItem, ContentItemIndex>(index =>
                    index.ContentType == ContentTypes.CenterProfile &&
                    index.ContentItemId == contentItemId &&
                    !index.Latest)
                .ListAsync();

        public async Task ReviewAsync(CenterProfileReviewCheckResult reviewCheckResult, bool accepted, string comment)
        {
            reviewCheckResult.ThrowIfNull();
            reviewCheckResult.ContentItem.ThrowIfNull();
            if (!accepted && string.IsNullOrEmpty(comment))
            {
                throw new ArgumentNullException(nameof(comment));
            }

            AddReviewState(reviewCheckResult.ContentItem, new CenterProfileReviewState()
            {
                Accepted = accepted,
                Comment = accepted ? null : comment,
                Post = reviewCheckResult.CurrentRole
            });

            AccreditationStatus? accreditationStatusAfterMdtDecision = null;
            var toUpdate = false;
            reviewCheckResult.ContentItem.Alter<CenterProfileManagerExtensionsPart>(part =>
            {
                if (!part.RenewalCenterProfileStatus.HasValue ||
                    part.RenewalCenterProfileStatus == CenterProfileStatus.Unsubmitted ||
                    part.RenewalCenterProfileStatus == CenterProfileStatus.MDTAccepted)
                {
                    return;
                }

                if (!accepted)
                {
                    part.RenewalCenterProfileStatus = CenterProfileStatus.Unsubmitted;
                    toUpdate = true;

                    return;
                }

                switch (part.RenewalCenterProfileStatus.Value)
                {
                    case CenterProfileStatus.UnderReviewAtTR:
                        part.RenewalCenterProfileStatus = CenterProfileStatus.UnderReviewAtOMKB;
                        break;
                    case CenterProfileStatus.UnderReviewAtOMKB:
                        part.RenewalCenterProfileStatus = CenterProfileStatus.UnderReviewAtMDT;
                        break;
                    case CenterProfileStatus.UnderReviewAtMDT:
                        part.RenewalCenterProfileStatus = CenterProfileStatus.MDTAccepted;
                        accreditationStatusAfterMdtDecision = part.RenewalAccreditationStatus;
                        break;
                    default:
                        return;
                }

                toUpdate = true;
            });

            if (toUpdate)
            {
                if (accreditationStatusAfterMdtDecision.HasValue)
                {
                    reviewCheckResult.ContentItem.Alter<CenterProfilePart>(part => ManagementDecision(part, accreditationStatusAfterMdtDecision.Value));
                }

                await _contentManager.UpdateAsync(reviewCheckResult.ContentItem);

                if (await CacheEnabledAsync())
                {
                    ClearCenterProfileCache();
                }
            }
        }

        public async Task<IEnumerable<string>> AcceptManyAsync(IEnumerable<CenterProfileDecisionStateViewModel> viewModelStates)
        {
            viewModelStates.ThrowIfNull();

            if (!viewModelStates.Any())
            {
                throw new ArgumentException();
            }

            var calculatedStatusOverridable = (await GetCenterManagerSettingsAsync()).CalculatedStatusOverridable;
            if (calculatedStatusOverridable)
            {
                var allowedStatuses = new[]
                {
                    default(AccreditationStatus?),
                    AccreditationStatus.Accredited,
                    AccreditationStatus.TemporarilyAccredited,
                    AccreditationStatus.Registered
                };

                if (viewModelStates.Any(state => !allowedStatuses.Contains(state.AccreditationStatus)))
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            var contentItemIds = viewModelStates.Select(x => x.ContentItemId);

            IEnumerable<ContentItem> contentItems;
            if (await CacheEnabledAsync())
            {
                contentItems = (await LoadCenterProfilesFromCacheAsync())
                    .Where(contentItem =>
                        contentItemIds.Contains(contentItem.ContentItemId) &&
                        contentItem.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus == CenterProfileStatus.UnderReviewAtMDT);
            }
            else
            {
                contentItems = await _session
                    .Query<ContentItem, CenterProfileManagerExtensionsPartIndex>(index => index.RenewalCenterProfileStatus == CenterProfileStatus.UnderReviewAtMDT)
                    .LatestAndPublished()
                    .Where(index => index.ContentItemId.IsIn(contentItemIds))
                    .ListAsync();
            }

            var idsOfAcceptedCenterProfiles = new List<string>();
            var updateCache = false;
            foreach (var contentItem in contentItems)
            {
                AddReviewState(contentItem, new CenterProfileReviewState()
                {
                    Accepted = true,
                    Post = CenterPosts.MDTManagement
                });

                var finalAccreditationStatus = default(AccreditationStatus?);
                if (calculatedStatusOverridable)
                {
                    finalAccreditationStatus = viewModelStates
                        .FirstOrDefault(x => x.ContentItemId == contentItem.ContentItemId)
                        ?.AccreditationStatus;
                }

                if (!finalAccreditationStatus.HasValue)
                {
                    finalAccreditationStatus = contentItem.As<CenterProfileManagerExtensionsPart>().RenewalAccreditationStatus;
                }

                // There is a theoretical possibility that this value is still null, however in practice can not be.
                if (!finalAccreditationStatus.HasValue)
                {
                    throw new ArgumentNullException($"{nameof(finalAccreditationStatus)} is still null. ContentItemId:{contentItem.ContentItemId}");
                }

                contentItem.Alter<CenterProfilePart>(part => ManagementDecision(part, finalAccreditationStatus.Value));
                contentItem.Alter<CenterProfileManagerExtensionsPart>(part =>
                {
                    part.RenewalCenterProfileStatus = CenterProfileStatus.MDTAccepted;
                });

                await _contentManager.UpdateAsync(contentItem);

                idsOfAcceptedCenterProfiles.Add(contentItem.ContentItemId);

                updateCache = true;
            }

            if (updateCache && await CacheEnabledAsync())
            {
                ClearCenterProfileCache();
            }

            return idsOfAcceptedCenterProfiles;
        }

        public async Task<CenterProfileEquipmentsSettings> GetCenterProfileEquipmentSettingsAsync()
            => (await _siteService.GetSiteSettingsAsync()).As<CenterProfileEquipmentsSettings>();

        public async Task SetCenterProfileEquipmentSettingsAsync(CenterProfileEquipmentsSettings settings)
        {
            var iSite = await _siteService.GetSiteSettingsAsync();
            iSite.Alter<CenterProfileEquipmentsSettings>(nameof(CenterProfileEquipmentsSettings), equipments =>
            {
                equipments.ToolsList = settings.ToolsList;
                equipments.LaboratoryList = settings.LaboratoryList;
            });

            await _siteService.UpdateSiteSettingsAsync(iSite);
        }

        public async Task SaveCenterProfileAsync(ContentItem contentItem, ICenterProfileViewModel viewModel)
        {
            contentItem.ThrowIfNull();
            viewModel.ThrowIfNull();

            if (contentItem.As<CenterProfileManagerExtensionsPart>().Submitted())
            {
                return;
            }

            contentItem.Alter<CenterProfilePart>(part => viewModel.UpdatePart(part));

            await _contentManager.UpdateAsync(contentItem);

            if (await CacheEnabledAsync())
            {
                ClearCenterProfileCache();
            }
        }

        public async Task<ContentItem> NewCenterProfileAsync(int leadersMemberRightId, bool shouldCreate)
        {
            var contentItem = await _contentManager.NewAsync(ContentTypes.CenterProfile);
            contentItem.Alter<CenterProfilePart>(part =>
            {
                part.MemberRightId = leadersMemberRightId;
                part.AccreditationStatus = AccreditationStatus.New;
                part.AccreditationStatusDateUtc = _clock.UtcNow;
            });
            contentItem.Alter<CenterProfileManagerExtensionsPart>(part => part.RenewalAccreditationStatus = AccreditationStatus.New);

            if (shouldCreate)
            {
                await _contentManager.CreateAsync(contentItem);

                if (await CacheEnabledAsync())
                {
                    ClearCenterProfileCache();
                }
            }

            return contentItem;
        }

        public async Task SaveCenterProfileAsync(ContentItem contentItem, bool submitted)
        {
            var created = contentItem.As<CenterProfilePart>().Created;
            await CalculateAccreditationStatusAsync(contentItem, part =>
            {
                part.RenewalCenterProfileStatus = created && submitted
                    ? CenterProfileStatus.UnderReviewAtTR
                    : CenterProfileStatus.Unsubmitted;
            });

            if (submitted && !created)
            {
                contentItem.Alter<CenterProfilePart>(part => part.Created = true);
            }
        }

        public async Task CalculateAccreditationStatusAsync(ContentItem contentItem, Action<CenterProfileManagerExtensionsPart> alteration = null)
        {
            var accreditationStatusResult = await _accreditationStatusCalculator.CalculateAccreditationStatusAsync(
                contentItem.As<CenterProfilePart>());

            contentItem.Alter<CenterProfileManagerExtensionsPart>(part =>
            {
                part.RenewalAccreditationStatus = accreditationStatusResult.AccreditationStatus;
                part.AccreditationStatusResult = accreditationStatusResult;

                alteration?.Invoke(part);
            });

            await _contentManager.UpdateAsync(contentItem);

            if (await CacheEnabledAsync())
            {
                ClearCenterProfileCache();
            }
        }

        public async Task<IEnumerable<ContentItem>> GetUnsubmittedCenterProfilesAsync(DateTime renewalStartDate)
        {
            if (await CacheEnabledAsync())
            {
                return (await LoadCenterProfilesFromCacheAsync())
                    .Where(contentItem =>
                    {
                        return contentItem.As<CenterProfilePart>().Created &&
                        (
                            contentItem.PublishedUtc < renewalStartDate ||
                            (
                                contentItem.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus == CenterProfileStatus.Unsubmitted &&
                                contentItem.PublishedUtc >= renewalStartDate
                            )
                        );
                    });
            }

            // Versions created earlier, and no alteration in the renewal period.
            var pristine = await _session
                .Query<ContentItem, CenterProfilePartIndex>(index => index.Created)
                .LatestAndPublished()
                .Where(index => index.PublishedUtc < renewalStartDate)
                .ListAsync();

            // Version created in the renewal period (ateration) that haven't submitted.
            var unsubmitted = await _session
                .Query<ContentItem, CenterProfilePartIndex>(index => index.Created)
                .With<CenterProfileManagerExtensionsPartIndex>(index => index.RenewalCenterProfileStatus == CenterProfileStatus.Unsubmitted)
                .LatestAndPublished()
                .Where(index => index.PublishedUtc >= renewalStartDate)
                .ListAsync();

            return pristine.Concat(unsubmitted);
        }

        public async Task ChangeCenterProfileLeaderAsync(ContentItem contentItem, int memberRightId)
        {
            contentItem.Alter<CenterProfilePart>(part =>
            {
                part.MemberRightId = memberRightId;
            });

            await _contentManager.UpdateAsync(contentItem);

            if (await CacheEnabledAsync())
            {
                ClearCenterProfileCache();
            }
        }

        public async Task OnUserProfilesUpdatedAsync(IEnumerable<int> memberRightIds)
        {
            memberRightIds.ThrowIfNull();
            if (!memberRightIds.Any())
            {
                return;
            }

            var contentItems = new List<ContentItem>();
            if (await CacheEnabledAsync())
            {
                contentItems.AddRange(
                    (await LoadCenterProfilesFromCacheAsync())
                    .Where(contentItem =>
                    {
                        var part = contentItem.As<CenterProfilePart>();

                        return memberRightIds.Contains(part.MemberRightId) ||
                            memberRightIds.Intersect(part.Colleagues
                                .Where(colleague => colleague.MemberRightId.HasValue)
                                .Select(x => x.MemberRightId.Value))
                            .Any();
                    }));
            }
            else
            {
                contentItems.AddRange(
                    await _session
                        .Query<ContentItem, CenterProfileColleagueIndex>(index => index.MemberRightId.IsIn(memberRightIds))
                        .LatestAndPublished()
                        .ListAsync());

                contentItems.AddRange(
                    await _session
                        .Query<ContentItem, CenterProfilePartIndex>(index => index.MemberRightId.IsIn(memberRightIds))
                        .LatestAndPublished()
                        .ListAsync());
            }

            if (contentItems.Any())
            {
                await Task.WhenAll(contentItems.Select(centerProfile => CalculateAccreditationStatusAsync(centerProfile)));
            }
        }

        public Task<IEnumerable<ContentItem>> LoadCenterProfilesFromCacheAsync()
            => _memoryCache.GetOrCreateAsync("CenterProfileService:AllCenterProfiles", entry =>
            {
                entry.AddExpirationToken(_signal.GetToken(CenterProfilesCacheKey));

                return _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == ContentTypes.CenterProfile)
                    .LatestAndPublished()
                    .ListAsync();
            });

        public void ClearCenterProfileCache()
            => _signal.SignalToken(CenterProfilesCacheKey);


        private async Task<IEnumerable<ContentItem>> GetPermittedCenterProfileQueryAsync(string id = null)
        {
            // TODO: Currently there is no method to join index tables without the Document table.
            // Do this better if yessql makes it possible.
            var sql =
                $"SELECT DISTINCT ci.{nameof(ContentItemIndex.ContentItemId)} " +
                $"FROM {nameof(TerritoryIndex)} t " +
                $"JOIN {nameof(SettlementIndex)} s on s.{nameof(SettlementIndex.TerritoryId)} = t.DocumentId " +
                $"JOIN {nameof(CenterProfilePartIndex)} c on c.{nameof(CenterProfilePartIndex.CenterZipCode)} = s.{nameof(SettlementIndex.ZipCode)} " +
                $"JOIN {nameof(ContentItemIndex)} ci on ci.{nameof(ContentItemIndex.DocumentId)} = c.DocumentId " +
                $"WHERE t.{nameof(TerritoryIndex.TerritorialRapporteurId)} = @territorialRapporteur";

            var user = await _betterUserService.GetCurrentUserAsync();

            var transaction = await _session.DemandAsync();
            var centerProfileContentItemIds = await transaction.Connection.QueryAsync<string>(sql, new { territorialRapporteur = user.Id }, transaction);

            if (id == null)
            {
                if (await CacheEnabledAsync())
                {
                    return (await GetCenterProfilesAsync()).Where(contentItem => centerProfileContentItemIds.Contains(contentItem.ContentItemId));
                }

                return await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentItemId.IsIn(centerProfileContentItemIds))
                    .LatestAndPublished()
                    .ListAsync();
            }

            if (!centerProfileContentItemIds.Contains(id))
            {
                throw new UnauthorizedException();
            }

            if (await CacheEnabledAsync())
            {
                return (await GetCenterProfilesAsync()).Where(contentItem => contentItem.ContentItemId == id);
            }

            return await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentItemId == id)
                .LatestAndPublished()
                .Slice(limit: 1)
                .ListAsync();

            //var zipCodes = await GetZipCodesOfTerritorialRapporteurAsync();

            //var query = _session.Query<ContentItem, ContentItemIndex>(index => index.ContentType == ContentTypes.CenterProfile);
            //if (!string.IsNullOrEmpty(id))
            //{
            //    query = query.Where(index => index.ContentItemId == id);
            //}

            //return query
            //    .LatestAndPublished()
            //    .With<CenterProfilePartIndex>(index =>
            //        index.Created &&
            //        index.CenterZipCode.IsIn(zipCodes));
        }

        //private async Task<IEnumerable<int>> GetZipCodesOfTerritorialRapporteurAsync()
        //{
        //    var user = await _betterUserService.GetCurrentUserAsync();
        //    user.ThrowIfNull();

        //    var territoryIds = (await _session
        //        .Query<Territory, TerritoryIndex>(t => t.TerritorialRapporteurId == user.Id)
        //        .ListAsync())
        //        .Select(x => x.Id);

        //    return (await _session
        //        .QueryIndex<SettlementIndex>(index => index.TerritoryId.IsIn(territoryIds))
        //        .ListAsync())
        //        .Select(index => int.Parse(index.ZipCode))
        //        .Distinct();
        //}

        private void ManagementDecision(CenterProfilePart part, AccreditationStatus finalAccreditationStatus)
        {
            part.AccreditationStatus = finalAccreditationStatus;
            part.AccreditationStatusDateUtc = _clock.UtcNow;
        }

        private void AddReviewState(ContentItem contentItem, CenterProfileReviewState centerProfileReviewState)
            => contentItem.Alter<CenterProfileReviewStatesPart>(part =>
            {
                foreach (var state in part.States)
                {
                    state.Current = false;
                }

                centerProfileReviewState.Current = true;
                centerProfileReviewState.Date = _clock.UtcNow;

                part.States.Add(centerProfileReviewState);
            });

        private async Task<CenterManagerSettings> GetCenterManagerSettingsAsync()
            => (await _siteService.GetSiteSettingsAsync()).As<CenterManagerSettings>();
    }
}
