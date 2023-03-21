using IntelliMed.DokiNetIntegration.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers
{
    [Admin]
    [Authorize]
    public class DataMigrationController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly ILogger _logger;
        private readonly IContentManager _contentManager;
        private readonly YesSql.ISession _session;

        private static int fakeEmailsCount = 0;


        public IHtmlLocalizer T { get; set; }


        public DataMigrationController(
            IAuthorizationService authorizationService,
            INotifier notifier,
            IHtmlLocalizer<DataMigrationController> htmlLocalizer,
            ILogger<DataMigrationController> logger,
            IContentManager contentManager,
            YesSql.ISession session)
        {
            _authorizationService = authorizationService;
            _notifier = notifier;
            _logger = logger;
            _contentManager = contentManager;
            _session = session;

            T = htmlLocalizer;
        }


        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Admin.Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            return View();
        }

        [HttpPost, ActionName(nameof(Index))]
        public async Task<IActionResult> IndexPost(ImportPostViewModel postModel, IFormFile file)
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Admin.Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            if (file == null || file.Length == 0)
            {
                _notifier.Warning(T["Nem lett fájl kiválasztva"]);

                return RedirectToAction(nameof(Index));
            }

            if (file.ContentType.ToLower() != "application/json")
            {
                _notifier.Warning(T["Csak JSON formátum megengedett"]);

                return Redirect(nameof(Index));
            }

            var buffer = new byte[file.Length];
            using (var stream = file.OpenReadStream())
            {
                await stream.ReadAsync(buffer, 0, buffer.Length);
            }

            var viewModel = JsonConvert.DeserializeObject<ImportViewModel>(Encoding.UTF8.GetString(buffer));

            try
            {
                await ImportDiabetesUserProfilesAsync(viewModel);
                await ImportCenterProfilesAsync(viewModel, postModel.FakeData);

                _notifier.Success(T["A feltöltés sikeres volt, az adatok mentésre kerültek."]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                _notifier.Error(T["A feltöltés nem sikerült."]);
                _notifier.Error(T[ex.Message]);
                _notifier.Error(T[ex.StackTrace]);

                _session.Cancel();
            }

            return RedirectToAction(nameof(Index));
        }


        private async Task ImportCenterProfilesAsync(ImportViewModel viewModel, bool fakeData)
        {
            var occupationCaptions = OccupationExtensions.GetLocalizedValues(T);

            var random = new Random();
            foreach (var profile in viewModel.CenterProfilePrts)
            {
                var centerTypes = new HashSet<CenterType>(profile.CenterTypes);
                if (profile.DiabeticPregnancyCare)
                {
                    centerTypes.Add(CenterType.Gestational);
                }

                if (profile.InsulinPumpTreatment)
                {
                    centerTypes.Add(CenterType.ContinuousInsulinDelivery);
                }

                var createdAt = DateTime.SpecifyKind(DateTime.Parse("2018-04-18"), DateTimeKind.Utc);

                var contentItem = await _contentManager.NewAsync(ContentTypes.CenterProfile);
                await _contentManager.CreateAsync(contentItem);

                if (fakeData)
                {
                    contentItem.Alter<CenterProfileManagerExtensionsPart>(part =>
                    {
                        part.RenewalCenterProfileStatus = (CenterProfileStatus)random.Next(0, Enum.GetNames(typeof(CenterProfileStatus)).Length);
                        part.RenewalAccreditationStatus = (AccreditationStatus)random.Next(0, Enum.GetNames(typeof(AccreditationStatus)).Length - 1);
                    });
                }

                contentItem.Alter<CenterProfilePart>(part =>
                {
                    part.AccreditationStatus = profile.AccreditationStatus;
                    part.AccreditationStatusDateUtc = createdAt;
                    part.Antsz = profile.Antsz;
                    part.BackgroundConcilium = profile.BackgroundConcilium;
                    part.BackgroundInpatient = profile.BackgroundInpatient;
                    part.CenterAddress = profile.CenterAddress;
                    part.CenterLeaderEmail = profile.CenterLeaderEmail;
                    part.CenterName = profile.CenterName;
                    part.CenterSettlementName = profile.CenterSettlementName;
                    part.CenterTypes = centerTypes;
                    part.CenterZipCode = profile.CenterZipCode;
                    part.Created = profile.Created;
                    part.Email = fakeData ? GetFakeEmail() : profile.Email;
                    part.Fax = profile.Fax;
                    part.Laboratory = profile.Laboratory;
                    part.Latitude = profile.Latitude;
                    part.Longitude = profile.Longitude;
                    part.MemberRightId = profile.MemberRightId;
                    part.Neak = profile.Neak;
                    part.OfficeHours = profile.OfficeHours;
                    part.OtherVocationalClinic = profile.OtherVocationalClinic;
                    part.PartOfOtherVocationalClinic = profile.PartOfOtherVocationalClinic;
                    part.Phone = profile.Phone;
                    part.Tools = profile.Tools;
                    part.VocationalClinic = profile.VocationalClinic;
                    part.Web = profile.Web;

                    var colleaguesGroupedByMemberRightId = profile.Colleagues.GroupBy(x => x.MemberRightId);
                    foreach (var group in colleaguesGroupedByMemberRightId)
                    {
                        if (group.Count() == 1)
                        {
                            part.Colleagues.Add(
                                MapColleague(group.First(), contentItem.ContentItemId, contentItem.ContentItemVersionId, fakeData));
                        }
                        else
                        {
                            part.Colleagues.Add(GetRankedCollague(group, contentItem.ContentItemId, contentItem.ContentItemVersionId, fakeData));
                        }
                    }
                });

                await _contentManager.UpdateAsync(contentItem);

                contentItem.CreatedUtc = createdAt;
                contentItem.ModifiedUtc = createdAt;
                contentItem.PublishedUtc = createdAt;

                _session.Save(contentItem);
            }
        }

        private Colleague GetRankedCollague(IGrouping<int?, Colleague> group, string contentItemId, string contentItemVersionId, bool fakeEmails)
        {
            var occupations = ((Occupation[])Enum.GetValues(typeof(Occupation))).OrderBy(occupation => (int)occupation);

            foreach (var occupation in occupations)
            {
                var colleague = group.FirstOrDefault(x => x.Occupation == occupation);
                if (colleague != null)
                {
                    return MapColleague(colleague, contentItemId, contentItemVersionId, fakeEmails);
                }
            }

            return null;
        }

        private Colleague MapColleague(Colleague colleague, string contentItemId, string contentItemVersionId, bool fakeEmails)
            => new Colleague()
            {
                Id = Guid.NewGuid(),
                CenterProfileContentItemId = contentItemId,
                CenterProfileContentItemVersionId = contentItemVersionId,
                Email = fakeEmails ? GetFakeEmail() : colleague.Email,
                FirstName = colleague.FirstName,
                LastName = colleague.LastName,
                MemberRightId = colleague.MemberRightId,
                Occupation = colleague.Occupation,
                Prefix = colleague.Prefix,
                StatusHistory = new List<ColleagueStatusItem>(colleague.StatusHistory)
            };

        private string GetFakeEmail()
        {
            return $"nonexisting.email.{(fakeEmailsCount++).ToString().PadLeft(4, '0')}@nodmain.cx";
        }

        private async Task ImportDiabetesUserProfilesAsync(ImportViewModel viewModel)
        {
            foreach (var profile in viewModel.DiabetesUserProfileParts)
            {
                // Continue if model is empty.
                if (string.IsNullOrEmpty(profile.GraduationIssuedBy) &&
                    !profile.GraduationYear.HasValue &&
                    string.IsNullOrEmpty(profile.OtherQualification) &&
                    !profile.Qualifications.Any())
                {
                    continue;
                }

                var contentItem = await _contentManager.NewAsync(ContentTypes.DiabetesUserProfile);
                contentItem.Alter<DiabetesUserProfilePart>(part =>
                {
                    part.MemberRightId = profile.MemberRightId;
                    part.GraduationIssuedBy = string.IsNullOrEmpty(profile.GraduationIssuedBy) ? null : profile.GraduationIssuedBy;
                    part.GraduationYear = profile.GraduationYear;
                    part.Qualifications = new List<PersonQualification>(profile.Qualifications);
                });
                await _contentManager.CreateAsync(contentItem);
            }
        }
    }

    public class ImportViewModel
    {
        /// <summary>
        /// Includes the ids.
        /// </summary>
        [JsonProperty("qualifications")]
        public IEnumerable<Qualification> Qualifications { get; set; } = new List<Qualification>();

        [JsonProperty("diabetesUserProfiles")]
        public IEnumerable<DiabetesUserProfilePart> DiabetesUserProfileParts { get; set; } = new List<DiabetesUserProfilePart>();

        [JsonProperty("centerProfiles")]
        public IEnumerable<CenterProfilePartExt> CenterProfilePrts { get; set; } = new List<CenterProfilePartExt>();

        [JsonProperty("omkb")]
        public IEnumerable<EditDokiNetMemberViewModel> Omkb { get; set; } = new List<EditDokiNetMemberViewModel>();

        [JsonProperty("rapporteurs")]
        public IEnumerable<ImportRapporteurViewModel> Rapporteurs { get; set; } = new List<ImportRapporteurViewModel>();
    }

    public class CenterProfilePartExt : CenterProfilePart
    {
        public bool DiabeticPregnancyCare { get; set; }

        public bool InsulinPumpTreatment { get; set; }
    }

    public class ImportRapporteurViewModel
    {
        public int? MemberRightId { get; set; }

        public string FullName { get; set; }

        public IEnumerable<string> Emails { get; set; } = new List<string>();

        public string UserName { get; set; }

        public IEnumerable<int> ZipCodes { get; set; } = new List<int>();
    }

    public class ImportPostViewModel
    {
        public bool FakeData { get; set; }
    }
}
