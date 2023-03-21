using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public class AccreditationStatusCalculator : IAccreditationStatusCalculator
    {
        private readonly IDiabetesUserProfileService _diabetesUserProfileService;
        private readonly IDokiNetService _dokiNetService;
        private readonly ISiteService _siteService;


        public AccreditationStatusCalculator(
            IDiabetesUserProfileService diabetesUserProfileService,
            IDokiNetService dokiNetService,
            ISiteService siteService)
        {
            _diabetesUserProfileService = diabetesUserProfileService;
            _dokiNetService = dokiNetService;
            _siteService = siteService;
        }


        public async Task<AccreditationStatusResult> CalculateAccreditationStatusAsync(CenterProfilePart part)
        {
            part.ThrowIfNull();

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            var equipmentsSettings = siteSettings.As<CenterProfileEquipmentsSettings>();
            var personalConditions = siteSettings.As<AccreditationPersonalConditionSettings>();
            var qualificationSettings = siteSettings.As<CenterProfileQualificationSettings>();

            // Assume that it will be 'Accredited'.
            var result = new AccreditationStatusResult()
            {
                AccreditationStatus = AccreditationStatus.Accredited
            };

            var colleagues = part.Colleagues
                .Where(colleague =>
                    colleague.MemberRightId.HasValue &&
                    colleague.Occupation.HasValue &&
                    ColleagueStatusExtensions.GreenZone.Contains(colleague.LatestStatusItem.Status));

            var memberRightIds = colleagues.Select(colleague => colleague.MemberRightId.Value).Concat(new[] { part.MemberRightId });

            var userProfileParts = (await _diabetesUserProfileService.GetProfilesByMemberRightIdsAsync(memberRightIds))
                .Select(contentItem => contentItem.As<DiabetesUserProfilePart>());

            var dokiNetMembers = await _dokiNetService.GetDokiNetMembersByIds<DokiNetMember>(
                colleagues.Select(x => x.MemberRightId.Value).Concat(new[] { part.MemberRightId }));

            var colleagueHelpers = colleagues
                .Select(colleague => new ColleagueAccreditationHelper()
                {
                    MemberRightId = colleague.MemberRightId.Value,
                    Occupation = colleague.Occupation.Value,
                    DiabetesUserProfile = userProfileParts.FirstOrDefault(x => x.MemberRightId == colleague.MemberRightId.Value)
                        ?? new DiabetesUserProfilePart(),
                    DokiNetMember = dokiNetMembers.FirstOrDefault(m => m.MemberRightId == colleague.MemberRightId)
                })
                .Concat(new[]
                {
                    // Adding leader: assume that leader is a doctor
                    new ColleagueAccreditationHelper()
                    {
                        MemberRightId = part.MemberRightId,
                        Occupation = Occupation.Doctor,
                        DiabetesUserProfile = userProfileParts.FirstOrDefault(x => x.MemberRightId == part.MemberRightId)
                            ?? new DiabetesUserProfilePart(),
                        DokiNetMember = dokiNetMembers.FirstOrDefault(m => m.MemberRightId == part.MemberRightId)
                    }
                });

            foreach (var colleagueHelper in colleagueHelpers)
            {
                if (colleagueHelper.DokiNetMember == null)
                {
                    continue;
                }

                if (!colleagueHelper.DokiNetMember.HasMemberShip)
                {
                    result.Membership.Add(colleagueHelper.DokiNetMember.FullName);
                    result.AccreditationStatus = AccreditationStatus.Registered;
                }

                if (!colleagueHelper.DokiNetMember.IsDueOk)
                {
                    result.MembershipFee.Add(colleagueHelper.DokiNetMember.FullName);
                    result.AccreditationStatus = AccreditationStatus.Registered;
                }
            }

            var dokctorMemberRightIds = colleagueHelpers
                .Where(colleague => colleague.Occupation == Occupation.Doctor)
                .Select(x => x.MemberRightId);

            var doctorDokiNetMembers = dokiNetMembers.Where(x => dokctorMemberRightIds.Contains(x.MemberRightId));

            // Checking if accredited
            if (!CheckPersonalCondition(
                colleagueHelpers,
                doctorDokiNetMembers,
                personalConditions.Accredited,
                qualificationSettings,
                AccreditationStatus.Accredited,
                out var personalConditionsForAccredited,
                out var mdtLicenceForAccredited))
            {
                // Checking if temporarily accredited
                if (!CheckPersonalCondition(
                    colleagueHelpers,
                    doctorDokiNetMembers,
                    personalConditions.TemporarilyAccredited,
                    qualificationSettings,
                    AccreditationStatus.TemporarilyAccredited,
                    out var personalConditionsForTemporarilyAccredited,
                    out var mdtLicenceForTemporarilyAccredited))
                {
                    result.AccreditationStatus = AccreditationStatus.Registered;
                    result.MdtLicence = mdtLicenceForTemporarilyAccredited;
                    result.PersonalConditions.AddRange(personalConditionsForTemporarilyAccredited);
                }
                else
                {
                    // Can not be temporarily accredited if it's already registrated
                    if (result.AccreditationStatus == AccreditationStatus.Accredited)
                    {
                        result.AccreditationStatus = AccreditationStatus.TemporarilyAccredited;
                    }

                    result.MdtLicence = mdtLicenceForAccredited;
                    result.PersonalConditions = personalConditionsForAccredited;
                }
            }

            // If the material conditions are not met the result will be 'Registrated'.
            var toolsInSetting = equipmentsSettings
                .ToolsList
                .Where(tool => tool.Required)
                .Select(tool => tool.Id);
            if (toolsInSetting.Any())
            {
                var tools = toolsInSetting.Where(toolSettingId =>
                    !part.Tools.Any(tool => tool.Id == toolSettingId && tool.Value.HasValue && tool.Value > 0));
                if (tools.Any())
                {
                    result.AccreditationStatus = AccreditationStatus.Registered;
                    result.Tools = new HashSet<string>(tools);
                }
            }

            var laboratoryInSettings = equipmentsSettings
                .LaboratoryList
                .Where(laboratory => laboratory.Required)
                .Select(laboratory => laboratory.Id);
            if (laboratoryInSettings.Any())
            {
                var laboratoryConditions = laboratoryInSettings.Where(laboratorySettingId =>
                    !part.Laboratory.Any(laboratory => laboratory.Id == laboratorySettingId && laboratory.Value));
                if (laboratoryConditions.Any())
                {
                    result.Laboratory = new HashSet<string>(laboratoryConditions);
                    result.AccreditationStatus = AccreditationStatus.Registered;
                }
            }

            if (!part.BackgroundConcilium)
            {
                result.BackgroundConcilium = true;
                result.AccreditationStatus = AccreditationStatus.Registered;
            }

            if (!part.BackgroundInpatient)
            {
                result.BackgroundInpatient = true;
                result.AccreditationStatus = AccreditationStatus.Registered;
            }

            return result;
        }


        private static bool CheckPersonalCondition(
            IEnumerable<ColleagueAccreditationHelper> colleagues,
            IEnumerable<DokiNetMember> dokiNetMembers,
            IEnumerable<PersonalCondition> accredited,
            CenterProfileQualificationSettings qualificationSettings,
            AccreditationStatus accreditationStatus,
            out List<AccreditationPersonalCondition> personalConditionsResult,
            out bool mdtLicence)
        {
            personalConditionsResult = new List<AccreditationPersonalCondition>();
            mdtLicence = false;

            var accreditationCondition = true;
            foreach (var personalCondition in accredited)
            {
                var colleaguesInOccupation = colleagues.Where(x => x.Occupation == personalCondition.Occupation);

                var requiredStates = new List<QualificationState>()
                {
                    QualificationState.Obtained
                };

                if (accreditationStatus == AccreditationStatus.TemporarilyAccredited &&
                    personalCondition.Occupation == Occupation.DiabetesNurseEducator)
                {
                    requiredStates.Add(QualificationState.Student);
                }

                var requiredQualifications = qualificationSettings[personalCondition.Occupation];

                var headCount = colleaguesInOccupation.Count();
                var qualifiedPeople = colleaguesInOccupation.Where(person =>
                    person.DiabetesUserProfile.Qualifications.Any(personQualification =>
                        // At least one of the required qualifications should match.
                        requiredQualifications.Contains(personQualification.QualificationId) &&
                        personQualification.State.HasValue &&
                        requiredStates.Contains(personQualification.State.Value)));

                // TODO: testing this block better.
                if (headCount < personalCondition.HeadCount || qualifiedPeople.Count() < personalCondition.HeadCount)
                {
                    var condition = new AccreditationPersonalCondition()
                    {
                        Occupation = personalCondition.Occupation,
                        RequiredHeadcount = personalCondition.HeadCount
                    };

                    if (headCount < personalCondition.HeadCount)
                    {
                        condition.HeadCount = headCount;
                    }

                    if (qualifiedPeople.Count() < personalCondition.HeadCount)
                    {
                        foreach (var colleague in colleaguesInOccupation)
                        {
                            if (!qualifiedPeople.Any(x => x.DokiNetMember.MemberRightId == colleague.DokiNetMember.MemberRightId))
                            {
                                condition.UnqualifiedPeople.Add(colleague.DokiNetMember.FullName);
                            }
                        }

                        condition.UnqualifiedPeople = condition.UnqualifiedPeople
                            .OrderBy(x => x)
                            .ToList();
                    }

                    personalConditionsResult.Add(condition);
                    accreditationCondition = false;
                }

                if (personalCondition.Occupation == Occupation.Doctor)
                {
                    if (!colleagues.Any(colleague =>
                        colleague.Occupation == Occupation.Doctor &&
                        dokiNetMembers.Any(dokiNetMember =>
                            dokiNetMember.MemberRightId == colleague.MemberRightId &&
                            !string.IsNullOrEmpty(dokiNetMember.DiabetLicenceNumber))))
                    {
                        mdtLicence = true;
                        accreditationCondition = false;
                    }
                }
            }

            return accreditationCondition;
        }
    }
}
