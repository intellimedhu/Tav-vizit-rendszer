using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Models.Enums;
using OrganiMedCore.Core.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Drivers
{
    public class EVisitOrganizationUserProfilePartDisplayDriver : ContentPartDisplayDriver<EVisitOrganizationUserProfilePart>
    {
        public IStringLocalizer T { get; set; }


        public EVisitOrganizationUserProfilePartDisplayDriver(IStringLocalizer<EVisitOrganizationUserProfilePartDisplayDriver> stringLocalizer)
        {
            T = stringLocalizer;
        }


        public override IDisplayResult Edit(EVisitOrganizationUserProfilePart part)
        {
            return Initialize<EVisitOrganizationUserProfilePartViewModel>("EVisitOrganizationUserProfilePart_Edit", model =>
                {
                    model.OrganizationUserProfileType = part.OrganizationUserProfileType;
                    model.Email = part.Email;
                    model.Name = part.Name;
                    model.Phone = part.Phone;

                    // Conditionally set profile type related part properties.
                    if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor)
                    {
                        model.StampNumber = part.StampNumber;
                        model.EesztId = part.EesztId;
                        model.EesztName = part.EesztName;
                        model.MedicalExams = part.MedicalExams;
                    }
                    if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor ||
                        part.OrganizationUserProfileType == OrganizationUserProfileTypes.SpecialAssistant)
                    {
                        model.QualificationLicenceExams = part.QualificationLicenceExams;
                    }
                    if (part.OrganizationUserProfileType != OrganizationUserProfileTypes.Doctor)
                    {
                        model.QualificationNameNumberDate = part.QualificationNameNumberDate;
                    }
                    if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor ||
                    part.OrganizationUserProfileType == OrganizationUserProfileTypes.SpecialAssistant ||
                    part.OrganizationUserProfileType == OrganizationUserProfileTypes.Assistant ||
                    part.OrganizationUserProfileType == OrganizationUserProfileTypes.Receptionist)
                    {
                        model.HighestRankOrEducation = part.HighestRankOrEducation;
                    }
                });
        }

        public override async Task<IDisplayResult> UpdateAsync(EVisitOrganizationUserProfilePart part, IUpdateModel updater)
        {
            var previousOrganizationUserProfileType = part.OrganizationUserProfileType;

            var model = new EVisitOrganizationUserProfilePartViewModel();

            await updater.TryUpdateModelAsync(model, Prefix);

            if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor &&
                string.IsNullOrEmpty(model.StampNumber))
            {
                updater.ModelState.AddModelError("EVisitOrganizationUserProfilePart.StampNumber", T["Doktor esetén a pecsétszám megadása kötelező."]);
            }

            if (!previousOrganizationUserProfileType.HasValue)
            {
                part.OrganizationUserProfileType = model.OrganizationUserProfileType;
            }

            part.Name = model.Name;
            part.Email = model.Email;
            part.Phone = model.Phone;
            // Conditionally update profile type related part properties.
            if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor)
            {
                part.StampNumber = model.StampNumber;
                part.EesztId = model.EesztId;
                part.EesztName = model.EesztName;
                part.MedicalExams = model.MedicalExams;
            }
            if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor ||
                part.OrganizationUserProfileType == OrganizationUserProfileTypes.SpecialAssistant)
            {
                part.QualificationLicenceExams = model.QualificationLicenceExams;
            }
            if (part.OrganizationUserProfileType != OrganizationUserProfileTypes.Doctor)
            {
                part.QualificationNameNumberDate = model.QualificationNameNumberDate;
            }
            if (part.OrganizationUserProfileType == OrganizationUserProfileTypes.Doctor ||
                    part.OrganizationUserProfileType == OrganizationUserProfileTypes.SpecialAssistant ||
                    part.OrganizationUserProfileType == OrganizationUserProfileTypes.Assistant ||
                    part.OrganizationUserProfileType == OrganizationUserProfileTypes.Receptionist)
            {
                part.HighestRankOrEducation = model.HighestRankOrEducation;
            }

            return Edit(part);
        }
    }
}
