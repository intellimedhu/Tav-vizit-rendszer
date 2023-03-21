using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class CenterProfileTenantService : ICenterProfileTenantService
    {
        private readonly IAccreditationStatusCalculator _accreditationStatusCalculator;
        private readonly ICenterProfileService _centerProfileService;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IDokiNetService _dokiNetService;
        private readonly ISession _session;


        public CenterProfileTenantService(
            IAccreditationStatusCalculator accreditationStatusCalculator,
            ICenterProfileService centerProfileService,
            IClock clock,
            IContentManager contentManager,
            IDokiNetService dokiNetService,
            ISession session)
        {
            _accreditationStatusCalculator = accreditationStatusCalculator;
            _centerProfileService = centerProfileService;
            _clock = clock;
            _dokiNetService = dokiNetService;
            _contentManager = contentManager;
            _session = session;
        }


        public async Task<IEnumerable<ContentItem>> GetCenterProfilesForTenantAsync(string tenantName)
        {
            if (await _centerProfileService.CacheEnabledAsync())
            {
                return (await _centerProfileService.LoadCenterProfilesFromCacheAsync())
                    .Where(contentItem =>
                        contentItem.As<CenterProfilePart>().Created &&
                        new[] { default(string), string.Empty, tenantName }.Contains(contentItem.As<CenterProfileManagerExtensionsPart>().AssignedTenantName));
            }

            return await _session
                .Query<ContentItem, CenterProfilePartIndex>(index => index.Created)
                .With<CenterProfileManagerExtensionsPartIndex>(index =>
                    index.AssignedTenantName == null ||
                    index.AssignedTenantName == string.Empty ||
                    index.AssignedTenantName == tenantName)
                .LatestAndPublished()
                .ListAsync();
        }

        public async Task<ContentItem> GetCenterProfileAssignedToTenantAsync(string tenantName)
        {
            tenantName.ThrowIfNull();

            var contentItem = await _centerProfileService.CacheEnabledAsync()
                ? (await _centerProfileService.LoadCenterProfilesFromCacheAsync())
                    .FirstOrDefault(x => x.As<CenterProfileManagerExtensionsPart>().AssignedTenantName == tenantName)
                : await _session
                    .Query<ContentItem, CenterProfileManagerExtensionsPartIndex>(index => index.AssignedTenantName == tenantName)
                    .LatestAndPublished()
                    .FirstOrDefaultAsync();

            if (contentItem == null)
            {
                throw new CenterProfileNotAssignedException();
            }

            return contentItem;
        }

        public async Task SetCenterProfileAssignmentAsync(string contentItemId, string tenantName)
        {
            if (string.IsNullOrEmpty(contentItemId))
            {
                return;
            }

            var cacheEnabled = await _centerProfileService.CacheEnabledAsync();
            var updateCache = false;

            if (!string.IsNullOrEmpty(tenantName))
            {
                var contentItemCurrentlyAssigned = cacheEnabled
                    ? (await _centerProfileService.LoadCenterProfilesFromCacheAsync())
                        .FirstOrDefault(x => x.As<CenterProfileManagerExtensionsPart>().AssignedTenantName == tenantName)
                    : await _session
                        .Query<ContentItem, CenterProfileManagerExtensionsPartIndex>(x => x.AssignedTenantName == tenantName)
                        .FirstOrDefaultAsync();

                // Remove connection first
                if (contentItemCurrentlyAssigned != null)
                {
                    contentItemCurrentlyAssigned.Alter<CenterProfileManagerExtensionsPart>(part => part.AssignedTenantName = null);
                    await _contentManager.UpdateAsync(contentItemCurrentlyAssigned);

                    updateCache = true;
                }
            }

            var contentItem = await _centerProfileService.GetCenterProfileAsync(contentItemId);
            if (contentItem != null && contentItem.As<CenterProfilePart>().Created)
            {
                contentItem.Alter<CenterProfileManagerExtensionsPart>(part =>
                {
                    part.AssignedTenantName = tenantName;
                });

                await _contentManager.UpdateAsync(contentItem);
                updateCache = true;
            }

            if (cacheEnabled && updateCache)
            {
                _centerProfileService.ClearCenterProfileCache();
            }
        }

        public async Task<ContentItem> UpdateCenterProfileAsync(string tenantName, ICenterProfileViewModel viewModel)
        {
            var contentItem = await GetCenterProfileAssignedToTenantAsync(tenantName);
            await _centerProfileService.SaveCenterProfileAsync(contentItem, viewModel);

            return contentItem;
        }

        public async Task SaveCenterProfileAsync(ContentItem contentItem, bool submitted)
            => await _centerProfileService.SaveCenterProfileAsync(contentItem, submitted);

        public async Task CalculateAccreditationStatusAsync(ContentItem contentItem)
            => await _centerProfileService.CalculateAccreditationStatusAsync(contentItem);

        public async Task<Colleague> ExecuteColleagueActionAsync(ContentItem contentItem, CenterProfileColleagueActionViewModel viewModel)
        {
            contentItem.ThrowIfNull();
            viewModel.ThrowIfNull();

            if (!Guid.TryParse(viewModel.ColleagueId, out var colleagueId))
            {
                throw new ColleagueNotFoundException();
            }

            var part = contentItem.As<CenterProfilePart>();
            var colleague = part.Colleagues.FirstOrDefault(x => x.Id == colleagueId);
            if (colleague == null)
            {
                throw new ColleagueNotFoundException();
            }

            var allowedStatuses = new List<ColleagueStatus>();
            ColleagueStatus newStatus;
            switch (viewModel.ColleagueAction)
            {
                case ColleagueAction.RemoveActive:
                    allowedStatuses.Add(ColleagueStatus.ApplicationAccepted);
                    allowedStatuses.Add(ColleagueStatus.PreExisting);
                    allowedStatuses.Add(ColleagueStatus.InvitationAccepted);
                    newStatus = ColleagueStatus.DeletedByLeader;
                    break;

                case ColleagueAction.AcceptApplication:
                    allowedStatuses.Add(ColleagueStatus.ApplicationSubmitted);
                    newStatus = ColleagueStatus.ApplicationAccepted;
                    break;

                case ColleagueAction.RejectApplication:
                    allowedStatuses.Add(ColleagueStatus.ApplicationSubmitted);
                    newStatus = ColleagueStatus.ApplicationRejected;
                    break;

                case ColleagueAction.ResendInvitation:
                    allowedStatuses.Add(ColleagueStatus.Invited);
                    allowedStatuses.Add(ColleagueStatus.InvitationRejected);
                    allowedStatuses.Add(ColleagueStatus.ApplicationCancelled);
                    newStatus = ColleagueStatus.Invited;
                    break;

                case ColleagueAction.CancelInvitation:
                    allowedStatuses.Add(ColleagueStatus.Invited);
                    newStatus = ColleagueStatus.InvitationCancelled;
                    break;

                default:
                    throw new ColleagueException("Nem létezik ilyen munkatárshoz kapcsolódó művelet.");
            }

            if (!allowedStatuses.Contains(colleague.LatestStatusItem.Status))
            {
                throw new ColleagueException("A munkatárs státusza a kérésnek megfelelően nem változtatható meg.");
            }

            contentItem.Alter<CenterProfilePart>(partAlter =>
            {
                var colleagueAlter = partAlter.Colleagues.First(x => x.Id == colleagueId);
                colleagueAlter.StatusHistory.Add(new ColleagueStatusItem()
                {
                    Status = newStatus,
                    StatusDateUtc = _clock.UtcNow
                });
            });

            await CalculateAccreditationStatusAsync(contentItem);

            return contentItem.As<CenterProfilePart>().Colleagues.First(x => x.Id == colleagueId);
        }

        public async Task<Colleague> InviteColleagueAsync(ContentItem contentItem, CenterProfileColleagueViewModel viewModel)
        {
            contentItem.ThrowIfNull();
            viewModel.ThrowIfNull();

            if (!viewModel.Email.IsEmail())
            {
                throw new ColleagueException("A megadott email cím formailag helytelen.");
            }

            viewModel.Email = viewModel.Email.ToLowerInvariant().Trim();

            var isMember = viewModel.MemberRightId.HasValue;
            DokiNetMember dokiNetMember = null;
            if (isMember)
            {
                dokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(viewModel.MemberRightId.Value);
            }

            var part = contentItem.As<CenterProfilePart>();

            Colleague colleague;
            var isNew = string.IsNullOrEmpty(viewModel.Id) || string.IsNullOrWhiteSpace(viewModel.Id);
            if (isNew)
            {
                if (part.Colleagues.Any(x => string.Equals(x.Email, viewModel.Email, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new ColleagueEmailAlreadyTakenException();
                }

                if (isMember && part.Colleagues.Any(x => x.MemberRightId == viewModel.MemberRightId.Value))
                {
                    throw new ColleagueAlreadyExistsException();
                }

                colleague = new Colleague()
                {
                    Id = Guid.NewGuid()
                };
            }
            else
            {
                var colleagueId = Guid.Parse(viewModel.Id);
                colleague = part.Colleagues.FirstOrDefault(x => x.Id == colleagueId);
                if (colleague == null)
                {
                    throw new ColleagueNotFoundException();
                }

                if (part.Colleagues.Any(x => x.Id != colleagueId && string.Equals(x.Email, viewModel.Email, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new ColleagueEmailAlreadyTakenException();
                }

                if (!new[]
                {
                    ColleagueStatus.Invited,
                    ColleagueStatus.DeletedByLeader,
                    ColleagueStatus.InvitationRejected,
                    ColleagueStatus.DeletedByColleague,
                    ColleagueStatus.ApplicationRejected,
                    ColleagueStatus.InvitationCancelled,
                    ColleagueStatus.ApplicationCancelled
                }.Contains(colleague.LatestStatusItem.Status))
                {
                    throw new ColleagueException("A munkatárs státusza a kérésnek megfelelően nem változtatható meg.");
                }
            }

            // The following properties can change even if the colleague have been recorded earlier.
            if (!isMember)
            {
                colleague.Prefix = viewModel.Prefix;
                colleague.FirstName = viewModel.FirstName;
                colleague.LastName = viewModel.LastName;
                colleague.MemberRightId = null;
                colleague.Email = viewModel.Email;
            }
            else
            {
                colleague.Prefix = dokiNetMember.Prefix;
                colleague.FirstName = dokiNetMember.FirstName;
                colleague.LastName = dokiNetMember.LastName;
                colleague.MemberRightId = dokiNetMember.MemberRightId;
                colleague.Email = dokiNetMember.Emails.Contains(viewModel.Email)
                    ? viewModel.Email
                    : dokiNetMember.Emails.FirstOrDefault();
            }

            colleague.CenterProfileContentItemId = contentItem.ContentItemId;
            colleague.CenterProfileContentItemVersionId = contentItem.ContentItemVersionId;
            colleague.Occupation = viewModel.Occupation;
            colleague.StatusHistory.Add(new ColleagueStatusItem()
            {
                Status = ColleagueStatus.Invited,
                StatusDateUtc = _clock.UtcNow
            });

            contentItem.Alter<CenterProfilePart>(partAlter =>
            {
                if (isNew)
                {
                    partAlter.Colleagues.Add(colleague);
                }
                else
                {
                    var colleagueAlter = partAlter.Colleagues.First(x => x.Id == colleague.Id);
                    partAlter.Colleagues.Remove(colleagueAlter);
                    partAlter.Colleagues.Add(colleague);
                }
            });

            await _contentManager.UpdateAsync(contentItem);
            if (await _centerProfileService.CacheEnabledAsync())
            {
                _centerProfileService.ClearCenterProfileCache();
            }

            return colleague;
        }

        public async Task<ContentItem> RequireCenterProfileContentItemInNewRenewalProcessAsync(ContentItem contentItem, bool shouldEmptyCache)
        {
            var contentItemNewVersion = await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.DraftRequired);
            contentItemNewVersion.Alter<CenterProfilePart>(part =>
            {
                // Removing those colleagues from the new version of the CP whose are not in the 'GreenZone'.
                part.Colleagues = part.Colleagues
                    .Where(colleague => ColleagueStatusExtensions.GreenZone.Contains(colleague.LatestStatusItem.Status))
                    .ToList();

                // Changing the latest status to 'PreExisting'
                foreach (var colleague in part.Colleagues)
                {
                    if (colleague.LatestStatusItem.Status != ColleagueStatus.PreExisting)
                    {
                        colleague.StatusHistory.Add(new ColleagueStatusItem()
                        {
                            StatusDateUtc = _clock.UtcNow,
                            Status = ColleagueStatus.PreExisting
                        });
                    }
                }
            });

            var accreditationStatusResult =
                await _accreditationStatusCalculator.CalculateAccreditationStatusAsync(contentItemNewVersion.As<CenterProfilePart>());

            contentItemNewVersion.Alter<CenterProfileManagerExtensionsPart>(part =>
            {
                part.RenewalCenterProfileStatus = CenterProfileStatus.Unsubmitted;

                part.RenewalAccreditationStatus = accreditationStatusResult.AccreditationStatus;
                part.AccreditationStatusResult = accreditationStatusResult;
            });

            contentItemNewVersion.Alter<CenterProfileReviewStatesPart>(part => part.States.Clear());

            await _contentManager.PublishAsync(contentItemNewVersion);

            if (shouldEmptyCache && await _centerProfileService.CacheEnabledAsync())
            {
                _centerProfileService.ClearCenterProfileCache();
            }

            return contentItemNewVersion;
        }
    }
}