using IntelliMed.Core.Extensions;
using OrchardCore.ContentManagement;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class CenterProfileReviewService : ICenterProfileReviewService
    {
        private readonly ITerritoryService _territoryService;
        private readonly ISession _session;


        public CenterProfileReviewService(
            ITerritoryService territoryService,
            ISession session)
        {
            _territoryService = territoryService;
            _session = session;
        }


        public async Task<CenterProfileReviewAuthorizationResult> GetAuthorizationResultAsync(User user, ContentItem contentItem)
        {
            user.ThrowIfNull();
            contentItem.ThrowIfNull();
            if (contentItem.ContentType != ContentTypes.CenterProfile)
            {
                throw new ArgumentException($"{ContentTypes.CenterProfile} expected but got {contentItem.ContentType}");
            }

            var partMg = contentItem.As<CenterProfileManagerExtensionsPart>();

            if (!partMg.RenewalCenterProfileStatus.HasValue ||
                !new[]
                {
                    CenterProfileStatus.UnderReviewAtMDT,
                    CenterProfileStatus.UnderReviewAtOMKB,
                    CenterProfileStatus.UnderReviewAtTR
                }.Contains(partMg.RenewalCenterProfileStatus.Value))
            {
                return CenterProfileReviewAuthorizationResult.Default;
            }

            if (partMg.RenewalCenterProfileStatus == CenterProfileStatus.UnderReviewAtMDT)
            {
                return new CenterProfileReviewAuthorizationResult()
                {
                    CurrentRole = CenterPosts.MDTManagement,
                    IsAuthorized = user.RoleNames.Contains(CenterPosts.MDTManagement)
                };
            }

            if (partMg.RenewalCenterProfileStatus == CenterProfileStatus.UnderReviewAtOMKB)
            {
                return new CenterProfileReviewAuthorizationResult()
                {
                    CurrentRole = CenterPosts.OMKB,
                    IsAuthorized = user.RoleNames.Contains(CenterPosts.OMKB)
                };
            }

            // From here: UnderReviewAtTR
            var isSecretary = user.RoleNames.Contains(CenterPosts.MDTSecretary);
            var isRapporteur = user.RoleNames.Contains(CenterPosts.TerritorialRapporteur);
            if (!isSecretary && !isRapporteur)
            {
                return CenterProfileReviewAuthorizationResult.Default;
            }

            var partCp = contentItem.As<CenterProfilePart>();

            try
            {
                if ((await _territoryService.GetRapporteurToSettlementAsync(partCp.CenterZipCode, partCp.CenterSettlementName)).Id == user.Id)
                {
                    return new CenterProfileReviewAuthorizationResult()
                    {
                        CurrentRole = CenterPosts.TerritorialRapporteur,
                        IsAuthorized = true
                    };
                }
            }
            catch (TerritoryException)
            {
                // If we got here there can be two reason: the given settlement has no territory or
                // the territory has no rapporteur, so the secretary will substitute the rapporteur.
                if (user.RoleNames.Contains(CenterPosts.MDTSecretary))
                {
                    return new CenterProfileReviewAuthorizationResult()
                    {
                        CurrentRole = CenterPosts.MDTSecretary,
                        IsAuthorized = true
                    };
                }
            }

            return CenterProfileReviewAuthorizationResult.Default;
        }

        public async Task<ReviewerStatistics> GetReviewerStatisticsAsync(User user, IEnumerable<ContentItem> contentItems)
        {
            contentItems.ThrowIfNull();

            var result = new ReviewerStatistics()
            {
                TotalCount = contentItems.Count()
            };

            if (!contentItems.Any() || user == null)
            {
                return result;
            }

            var reviewableContentItemIds = new List<string>();
            var nonReviewableContentItemIds = new List<string>();

            var managerExtensionsParts = contentItems.Select(contentItem => contentItem.As<CenterProfileManagerExtensionsPart>());

            if (user.RoleNames.Contains(CenterPosts.MDTManagement))
            {
                var reviewable = managerExtensionsParts.Where(part => part.RenewalCenterProfileStatus == CenterProfileStatus.UnderReviewAtMDT);
                reviewableContentItemIds.AddRange(reviewable.Select(part => part.ContentItem.ContentItemId));
                result.ReviewStatisticsByRoles.Add(new ReviewStatisticsByRoles()
                {
                    RoleName = CenterPosts.MDTManagement,
                    ReviewableCount = reviewable.Count(),
                    ReviewedCount = managerExtensionsParts.Count(part =>
                        part.RenewalCenterProfileStatus == CenterProfileStatus.MDTAccepted),
                    NonReviewableCount = managerExtensionsParts.Count(part =>
                        part.RenewalCenterProfileStatus != CenterProfileStatus.UnderReviewAtMDT &&
                        part.RenewalCenterProfileStatus != CenterProfileStatus.MDTAccepted),
                });
            }

            if (user.RoleNames.Contains(CenterPosts.OMKB))
            {
                var reviewable = managerExtensionsParts.Where(part => part.RenewalCenterProfileStatus == CenterProfileStatus.UnderReviewAtOMKB);
                reviewableContentItemIds.AddRange(reviewable.Select(part => part.ContentItem.ContentItemId));
                result.ReviewStatisticsByRoles.Add(new ReviewStatisticsByRoles()
                {
                    RoleName = CenterPosts.OMKB,
                    ReviewableCount = reviewable.Count(),
                    ReviewedCount = managerExtensionsParts.Count(part =>
                        part.RenewalCenterProfileStatus == CenterProfileStatus.UnderReviewAtMDT ||
                        part.RenewalCenterProfileStatus == CenterProfileStatus.MDTAccepted),
                    NonReviewableCount = managerExtensionsParts.Count(part =>
                        part.RenewalCenterProfileStatus != CenterProfileStatus.UnderReviewAtOMKB &&
                        part.RenewalCenterProfileStatus != CenterProfileStatus.UnderReviewAtMDT &&
                        part.RenewalCenterProfileStatus != CenterProfileStatus.MDTAccepted),
                });
            }

            var isSecretary = user.RoleNames.Contains(CenterPosts.MDTSecretary);
            var isRapporteur = user.RoleNames.Contains(CenterPosts.TerritorialRapporteur);
            if (isSecretary || isRapporteur)
            {
                var authorizedTerritoryIds = new List<int>();

                var territories = await _territoryService.GetTerritoriesAsync();
                if (isSecretary)
                {
                    // Territories where no territorial rapporteur has been selected:
                    authorizedTerritoryIds.AddRange(territories
                        .Where(territory => !territory.TerritorialRapporteurId.HasValue)
                        .Select(territory => territory.Id));
                }

                if (isRapporteur)
                {
                    // Territories where the current user is the territorial rapporteur:
                    authorizedTerritoryIds.AddRange(territories
                        .Where(territory => territory.TerritorialRapporteurId == user.Id)
                        .Select(territory => territory.Id));
                }

                authorizedTerritoryIds = authorizedTerritoryIds.Distinct().ToList();
                if (authorizedTerritoryIds.Any())
                {
                    var query = _session.Query<Settlement, SettlementIndex>();
                    if (isSecretary)
                    {
                        // If the user has secretary role he/she is authorized to review all the center profiles that have no territories
                        // Ideally there is no such a settlement.
                        query = query.Where(index => index.TerritoryId.IsIn(authorizedTerritoryIds) || index.TerritoryId == null);
                    }
                    else
                    {
                        query = query.Where(index => index.TerritoryId.IsIn(authorizedTerritoryIds));
                    }

                    var statisticsForTr = new ReviewStatisticsByRoles()
                    {
                        RoleName = CenterPosts.TerritorialRapporteur
                    };

                    var settlements = await query.ListAsync();
                    Parallel.ForEach(managerExtensionsParts, part =>
                    {
                        var centerProfilePart = part.ContentItem.As<CenterProfilePart>();
                        if (settlements.Any(x => x.ZipCode == centerProfilePart.CenterZipCode && x.Name == centerProfilePart.CenterSettlementName))
                        {
                            lock (statisticsForTr)
                            {
                                if (part.RenewalCenterProfileStatus == null ||
                                    part.RenewalCenterProfileStatus == CenterProfileStatus.Unsubmitted)
                                {
                                    statisticsForTr.NonReviewableCount++;
                                }
                                else if (part.RenewalCenterProfileStatus == CenterProfileStatus.UnderReviewAtTR)
                                {
                                    reviewableContentItemIds.Add(part.ContentItem.ContentItemId);
                                    statisticsForTr.ReviewableCount++;
                                }
                                else
                                {
                                    statisticsForTr.ReviewedCount++;
                                }
                            }
                        }
                    });

                    result.ReviewStatisticsByRoles.Add(statisticsForTr);
                }
            }

            result.ReviewableContentItemIds = new HashSet<string>(reviewableContentItemIds).ToList();

            return result;
        }
    }
}
