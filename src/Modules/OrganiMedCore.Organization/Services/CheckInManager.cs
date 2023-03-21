using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrganiMedCore.Core.Exceptions;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Services;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Exceptions;
using OrganiMedCore.Organization.Indexes;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.Models.Enums;
using OrganiMedCore.Organization.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.Organization.Services
{
    public class CheckInManager : ICheckInManager
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly IEVisitPatientProfileService _eVisitPatientProfileService;
        private readonly IOrganizationService _organizationService;
        private readonly IClock _clock;
        private readonly ShellSettings _shellSettings;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly IEVisitOrganizationUserProfileService _eVisitOrganizationUserProfileService;
        private readonly IOrganizationUserProfileService _organizationUserProfileService;

        public IStringLocalizer T { get; set; }


        public CheckInManager(
            ISession session,
            IContentManager contentManager,
            IEVisitPatientProfileService eVisitPatientProfileService,
            IStringLocalizer<CheckInManager> stringLocalizer,
            IOrganizationService organizationService,
            IClock clock,
            ShellSettings shellSettings,
            ISharedDataAccessorService sharedDataAccessorService,
            IEVisitOrganizationUserProfileService eVisitOrganizationUserProfileService,
            IOrganizationUserProfileService organizationUserProfileService)
        {
            _session = session;
            _contentManager = contentManager;
            _eVisitPatientProfileService = eVisitPatientProfileService;
            _organizationService = organizationService;
            _clock = clock;
            _shellSettings = shellSettings;
            _sharedDataAccessorService = sharedDataAccessorService;
            _eVisitOrganizationUserProfileService = eVisitOrganizationUserProfileService;
            _organizationUserProfileService = organizationUserProfileService;

            T = stringLocalizer;
        }


        public async Task CheckInPatient(IServiceScope managersServiceScope, string eVisitPatientProfileId, string organizationUnitId, string eVisitOrganizationUserProfileId)
        {
            eVisitPatientProfileId.ThrowIfNullOrEmpty();
            organizationUnitId.ThrowIfNullOrEmpty();

            var utcNow = _clock.UtcNow;

            var checkIns = await GetCheckInsAsync(eVisitPatientProfileId, organizationUnitId, utcNow);
            if (checkIns.Any(x => x.As<CheckInPart>().CheckInStatus != CheckInStatuses.TreatementFinished))
            {
                throw new PatientAlreadyCheckedInException("A páciens már hozzá van adva az osztályos napi listához.");
            }

            var patient = await _eVisitPatientProfileService.GetAsync(managersServiceScope, eVisitPatientProfileId);
            if (patient == null)
            {
                throw new PatientNotFoundException(T["Nem található páciens ilyen azonosítóval."]);
            }

            var organizationUnit = await _organizationService.GetOrganizationUnitAsync(organizationUnitId);
            if (organizationUnit == null)
            {
                throw new OrganizationUnitNotFoundException(T["Nem található osztály ilyen azonosítóval."]);
            }

            var eVisitOrganizationUserProfile = await _eVisitOrganizationUserProfileService.GetAsync(managersServiceScope, eVisitOrganizationUserProfileId);
            if (eVisitOrganizationUserProfile == null)
            {
                throw new EVisitOrganizationUserProfileNotFoundException(T["Nem található intézményi dolgozó ilyen azonosítóval."]);
            }

            var organizationUserProfile = await _session
                .Query<ContentItem, OrganizationUserProfilePartIndex>(x => x.EVisitOrganizationUserProfileId == eVisitOrganizationUserProfileId)
                .FirstOrDefaultAsync();
            if (organizationUserProfile == null)
            {
                throw new EVisitOrganizationUserProfileNotFoundException(T["Nem található intézményi dolgozó ilyen azonosítóval."]);
            }

            var checkIn = await _contentManager.NewAsync(ContentTypes.CheckIn);
            checkIn.Alter<CheckInPart>(x =>
            {
                x.CheckInDateUtc = utcNow;
                x.CheckInStatus = CheckInStatuses.Waiting;
            });
            checkIn.Alter<MetaDataPart>(x =>
            {
                x.EVisitPatientProfileId = eVisitPatientProfileId;
                x.OrganizationUnitId = organizationUnitId;
                x.EVisitOrganizationUserProfileId = eVisitOrganizationUserProfileId;
            });

            var tenantName = _shellSettings.Name;
            if (!patient.As<EVisitPatientProfilePart>().AttendedOrganizationNames.Any(x => x == tenantName))
            {
                patient = await _eVisitPatientProfileService.GetAsync(managersServiceScope, eVisitPatientProfileId);
                patient.Alter<EVisitPatientProfilePart>(x => x.AttendedOrganizationNames.Add(tenantName));

                await managersServiceScope.ServiceProvider.GetRequiredService<IContentManager>()
                    .PublishAsync(patient);
            }

            await _contentManager.CreateAsync(checkIn);
        }

        public async Task<IEnumerable<ContentItem>> GetCheckInsAsync(string eVisitPatientProfileId, string organizationUnitId, DateTime checkInDateUtc)
        {
            eVisitPatientProfileId.ThrowIfNullOrEmpty();
            organizationUnitId.ThrowIfNullOrEmpty();
            checkInDateUtc.ThrowIfNull();

            var min = checkInDateUtc.ToLocalTime().Date.ToUniversalTime();
            var max = min.AddDays(1);

            return await _session
                 .Query<ContentItem, ContentItemIndex>(x => x.ContentType == ContentTypes.CheckIn)
                 .LatestAndPublished()
                 .With<CheckInPartIndex>(x => x.CheckInDateUtc >= min && x.CheckInDateUtc < max)
                 .With<MetaDataPartIndex>(x =>
                     x.EVisitPatientProfileId == eVisitPatientProfileId &&
                     x.OrganizationUnitId == organizationUnitId)
                 .ListAsync();
        }

        public async Task<IEnumerable<ContentItem>> GetCheckInsAsync(DateTime checkInDateUtc, string organizationUnitId = "")
        {
            checkInDateUtc.ThrowIfNull();

            var min = checkInDateUtc.ToLocalTime().Date.ToUniversalTime();
            var max = min.AddDays(1);

            var query = _session
                .Query<ContentItem, ContentItemIndex>(x => x.ContentType == ContentTypes.CheckIn)
                .LatestAndPublished()
                .With<CheckInPartIndex>(x => x.CheckInDateUtc >= min && x.CheckInDateUtc < max);

            if (!string.IsNullOrEmpty(organizationUnitId))
            {
                query.With<MetaDataPartIndex>(x => x.OrganizationUnitId == organizationUnitId);
            }

            return await query.ListAsync();
        }

        public async Task<DailyListViewModel> GetCheckedInPatientsAsync(IServiceScope managersServiceScope, DateTime checkInDateUtc, string organizationUnitId = "")
        {
            checkInDateUtc.ThrowIfNull();

            var viewModel = new DailyListViewModel { };

            var checkIns = await GetCheckInsAsync(checkInDateUtc, organizationUnitId);
            if (checkIns.Any())
            {
                var patientIds = checkIns.Select(x => x.As<MetaDataPart>().EVisitPatientProfileId).Distinct();
                var patients = await _eVisitPatientProfileService.GetByIds(managersServiceScope, patientIds);

                var organizationUnits = await _organizationService.ListOrganizationUnitsAsync();

                foreach (var checkIn in checkIns)
                {
                    var patient = patients.FirstOrDefault(x => x.ContentItemId == checkIn.As<MetaDataPart>().EVisitPatientProfileId);
                    var organizationUnit = organizationUnits.FirstOrDefault(x => x.ContentItem.ContentItemId == checkIn.As<MetaDataPart>().OrganizationUnitId);
                    if (patient != null && organizationUnit != null)
                    {
                        var doctors = await _organizationUserProfileService.GetDoctors(managersServiceScope, organizationUnit.ContentItem.ContentItemId);
                        var doctor = doctors.Where(x =>
                            x.EVisitOrganizationUserProfilePart.ContentItem.ContentItemId == checkIn.As<MetaDataPart>().EVisitOrganizationUserProfileId)
                            .FirstOrDefault();
                        if (doctor != null)
                        {
                            viewModel.DailyListItems.Add(new DailyListItemViewModel
                            {
                                CheckIn = checkIn,
                                EVisitPatientProfile = patient,
                                OrganizationUnit = organizationUnit.ContentItem,
                                Doctor = doctor.EVisitOrganizationUserProfilePart
                            });
                        }
                    }
                }
            }

            return viewModel;
        }

        public async Task<bool> SetCheckInStatus(string checkInId, CheckInStatuses checkInStatus)
        {
            checkInId.ThrowIfNullOrEmpty();

            var checkIn = await _contentManager.GetAsync(checkInId, ContentTypes.CheckIn);
            if (checkIn == null) return false;
            var checkInPart = checkIn.As<CheckInPart>();
            switch (checkInPart.CheckInStatus)
            {
                case CheckInStatuses.Waiting:
                    if (checkInStatus != CheckInStatuses.TreatmentInProgress) return false;
                    break;
                case CheckInStatuses.TreatmentInProgress:
                    if (checkInStatus != CheckInStatuses.TreatementFinished) return false;
                    break;
                case CheckInStatuses.TreatementFinished:
                default:
                    return false;
            }

            checkIn = await _contentManager.GetNewVersionAsync(checkInId, ContentTypes.CheckIn);
            checkIn.Alter<CheckInPart>(x =>
            {
                x.CheckInStatus = checkInStatus;
            });

            await _contentManager.PublishAsync(checkIn);

            return true;
        }
    }
}
