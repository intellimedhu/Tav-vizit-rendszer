using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Validators;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class DiabetesUserProfileService : IDiabetesUserProfileService
    {
        private readonly DiabetesUserProfileValidator _diabetesUserProfileValidator;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger _logger;
        private readonly IQualificationService _qualificationService;
        private readonly ISession _session;


        public IHtmlLocalizer T { get; set; }


        public DiabetesUserProfileService(
            DiabetesUserProfileValidator diabetesUserProfileValidator,
            IClock clock,
            IContentManager contentManager,
            IDokiNetService dokiNetService,
            IHtmlLocalizer<DiabetesUserProfileService> htmlLocalizer,
            ILogger<DiabetesUserProfileService> logger,
            IQualificationService qualificationService,
            ISession session)
        {
            _diabetesUserProfileValidator = diabetesUserProfileValidator;
            _clock = clock;
            _contentManager = contentManager;
            _dokiNetService = dokiNetService;
            _logger = logger;
            _qualificationService = qualificationService;
            _session = session;

            T = htmlLocalizer;
        }


        public async Task<ContentItem> GetProfileByMemberRightIdAsync(int memberRightId)
        {
            return (await GetProfilesByMemberRightIdsAsync(new[] { memberRightId })).FirstOrDefault();
        }

        public async Task<IEnumerable<ContentItem>> GetProfilesByMemberRightIdsAsync(IEnumerable<int> memberRightIds)
        {
            memberRightIds.ThrowIfNull();

            return await _session
                .Query<ContentItem, DiabetesUserProfilePartIndex>(index => index.MemberRightId.IsIn(memberRightIds))
                .LatestAndPublished()
                .ListAsync();
        }

        public async Task<object> InitializeProfileEditorAsync(int memberRightId)
        {
            var dokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(memberRightId);

            var viewModel = new DiabetesUserProfilePartViewModel()
            {
                DiabetLicenceNumber = dokiNetMember.DiabetLicenceNumber
            };

            var contentItem = await GetProfileByMemberRightIdAsync(dokiNetMember.MemberRightId);
            if (contentItem != null)
            {
                var part = contentItem.As<DiabetesUserProfilePart>();
                viewModel.UpdateViewModel(part);
            }

            var settings = await _qualificationService.GetQualificationSettingsAsync();
            var viewModelSettings = new CenterProfileQualificationSettingsViewModel();
            viewModelSettings.UpdateViewModel(settings);

            var viewModelQualifications = settings.Qualifications.Select(x =>
            {
                var viewModelQualification = new QualificationViewModel();
                viewModelQualification.UpdateViewModel(x);

                return viewModelQualification;
            });

            return new
            {
                dokiNetMember,
                viewModel,
                qualifications = viewModelQualifications,
                qualificationStates = QualificationStateCaptions.GetLocalizedValues(T)
                        .Select(x => new KeyValuePair<int, string>((int)x.Key, x.Value)),
                occupations = OccupationExtensions.GetLocalizedValues(T)
                        .Select(x => new KeyValuePair<int, string>((int)x.Key, x.Value)),
                settings = viewModelSettings
            };
        }

        public async Task SetProfileByMemberRightIdAsync(int memberRightId, DiabetesUserProfilePartViewModel viewModel)
        {
            viewModel.ThrowIfNull();
            await CreateOrUpdateProfileByMemberRightIdAsync(memberRightId, part =>
            {
                viewModel.UpdatePart(part);
            });
        }

        public async Task SetPartialProfileByMemberRightIdAsync(Occupation occupation, int memberRightId, DiabetesUserProfilePartViewModel viewModel)
        {
            viewModel.ThrowIfNull();

            var qualificationSettings = await _qualificationService.GetQualificationSettingsAsync();
            await CreateOrUpdateProfileByMemberRightIdAsync(memberRightId, part =>
            {
                if (qualificationSettings[occupation].Any())
                {
                    var qualificationsToUpdate = viewModel.Qualifications.Select(x =>
                    {
                        var qualification = new PersonQualification();
                        x.UpdateModel(qualification);

                        return qualification;
                    });

                    var ids = qualificationsToUpdate.Select(x => x.QualificationId);
                    part.Qualifications = part.Qualifications
                        .Where(x => !ids.Contains(x.QualificationId))
                        .Concat(qualificationsToUpdate);
                }

                if (OccupationExtensions.GetOccupationsRequiredForGraduation().Contains(occupation))
                {
                    part.GraduationIssuedBy = viewModel.GraduationIssuedBy;
                    part.GraduationYear = viewModel.GraduationYear;
                }

                if (OccupationExtensions.GetOccupationsRequiredForOtherQualification().Contains(occupation))
                {
                    part.OtherQualification = viewModel.OtherQualification;
                }
            });
        }

        public async Task<PersonDataCompactViewModel> GetPersonDataCompactViewModel(int memberRightId)
        {
            var viewModel = new PersonDataCompactViewModel();

            var contentItem = GetProfileByMemberRightIdAsync(memberRightId);
            var dokiNetMember = _dokiNetService.GetDokiNetMemberById<DokiNetMember>(memberRightId);

            await Task.WhenAll(contentItem, dokiNetMember);

            var part = contentItem.Result?.As<DiabetesUserProfilePart>() ?? new DiabetesUserProfilePart();

            viewModel.PersonQualifications = new DiabetesUserProfilePartViewModel();
            viewModel.PersonQualifications.UpdateViewModel(part);

            viewModel.PrivatePhone = dokiNetMember.Result?.PrivatePhone;
            viewModel.HasMembership = dokiNetMember.Result?.HasMemberShip == true;
            viewModel.IsMembershipFeePaid = dokiNetMember.Result?.IsDueOk == true;

            if (viewModel.PersonQualifications.Qualifications.Any())
            {
                var settings = await _qualificationService.GetQualificationSettingsAsync();

                viewModel.Qualifications = settings.Qualifications
                    .Where(x => viewModel.PersonQualifications.Qualifications.Any(y => y.QualificationId == x.Id))
                    .Select(x =>
                    {
                        var qualificationViewModel = new QualificationViewModel();
                        qualificationViewModel.UpdateViewModel(x);

                        return qualificationViewModel;
                    });
            }



            return viewModel;
        }

        public async Task<bool> HasMissingQualificationsForOccupation<T>(Occupation occupation, T dokiNetMember) where T : DokiNetMember
        {
            dokiNetMember.ThrowIfNull();

            var contentItem = await GetProfileByMemberRightIdAsync(dokiNetMember.MemberRightId);
            var part = contentItem?.As<DiabetesUserProfilePart>() ?? new DiabetesUserProfilePart();

            var qualificationSettings = await _qualificationService.GetQualificationSettingsAsync();
            var requiredQualifications = qualificationSettings[occupation];
            if (requiredQualifications.Any() && !requiredQualifications.Any(x => part.Qualifications.Any(q => q.QualificationId == x)))
            {
                return true;
            }

            if (OccupationExtensions.GetOccupationsRequiredForGraduation().Contains(occupation) &&
                !part.HasGraduation)
            {
                return true;
            }

            if (OccupationExtensions.GetOccupationsRequiredForOtherQualification().Contains(occupation) &&
                string.IsNullOrEmpty(part.OtherQualification))
            {
                return true;
            }

            return false;
        }

        public async Task UpdateProfileAsync(int memberRightId, DiabetesUserProfilePartViewModel viewModel, IUpdateModel updater)
        {
            viewModel.ThrowIfNull();
            updater.ThrowIfNull();

            await _diabetesUserProfileValidator.ValidateAsync(
                _qualificationService,
                T,
                viewModel,
                _clock.UtcNow.Year,
                updater.ModelState.AddModelError);

            if (!updater.ModelState.IsValid)
            {
                return;
            }

            await SetProfileByMemberRightIdAsync(memberRightId, viewModel);
        }

        public async Task UpdatePartialProfileAsync(Occupation occupation, DokiNetMember dokiNetMember, DiabetesUserProfilePartViewModel viewModel, IUpdateModel updater)
        {
            viewModel.ThrowIfNull();
            updater.ThrowIfNull();

            await _diabetesUserProfileValidator.ValidateAsync(
                occupation,
                _qualificationService,
                T,
                viewModel,
                _clock.UtcNow.Year,
                updater.ModelState.AddModelError);
            if (!updater.ModelState.IsValid)
            {
                return;
            }

            await SetPartialProfileByMemberRightIdAsync(occupation, dokiNetMember.MemberRightId, viewModel);

            if (occupation == Occupation.Doctor && dokiNetMember.DiabetLicenceNumber != viewModel.DiabetLicenceNumber)
            {
                try
                {
                    await _dokiNetService.SaveMemberDataAnsyc(
                        dokiNetMember.MemberId,
                        new[]
                        {
                            new MemberData()
                            {
                                DataCode = nameof(DokiNetMember.DiabetLicenceNumber),
                                DataValue = viewModel.DiabetLicenceNumber
                            }
                        });
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(
                        ex,
                        "DokiNetService.SaveMemberDataAnsyc failed",
                        dokiNetMember.MemberId,
                        nameof(DokiNetMember.DiabetLicenceNumber), viewModel.DiabetLicenceNumber);
                }
            }
        }


        private async Task CreateOrUpdateProfileByMemberRightIdAsync(int memberRightId, Action<DiabetesUserProfilePart> action)
        {
            action.ThrowIfNull();

            var contentItem = await GetProfileByMemberRightIdAsync(memberRightId);
            var isNew = contentItem == null;
            if (isNew)
            {
                contentItem = await _contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
            }
            else
            {
                contentItem = await _contentManager.GetNewVersionAsync(contentItem.ContentItemId, ContentTypes.DiabetesUserProfile);
            }

            contentItem.Alter<DiabetesUserProfilePart>(part =>
            {
                part.MemberRightId = memberRightId;
                action(part);
            });

            if (isNew)
            {
                await _contentManager.CreateAsync(contentItem);
            }
            else
            {
                await _contentManager.PublishAsync(contentItem);
            }
        }
    }
}
