using IntelliMed.Core.Extensions;
using IntelliMed.Core.ViewModels;
using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class DiabetesUserProfilePartViewModel : IContentPartViewModel<DiabetesUserProfilePart>
    {
        [JsonProperty("qualifications")]
        public IEnumerable<PersonQualificationViewModel> Qualifications { get; set; } = new List<PersonQualificationViewModel>();

        [JsonProperty("graduationIssuedBy")]
        public string GraduationIssuedBy { get; set; }

        [JsonProperty("graduationYear")]
        public int? GraduationYear { get; set; }

        [JsonProperty("otherQualification")]
        public string OtherQualification { get; set; }

        [JsonProperty("diabetLicenceNumber")]
        public string DiabetLicenceNumber { get; set; }


        public void UpdatePart(DiabetesUserProfilePart part)
        {
            part.ThrowIfNull();

            part.Qualifications = Qualifications.Select(x =>
            {
                var model = new PersonQualification();
                x.UpdateModel(model);

                return model;
            });
            part.GraduationIssuedBy = GraduationIssuedBy;
            part.GraduationYear = GraduationYear;
            part.OtherQualification = OtherQualification;
        }

        public void UpdateViewModel(DiabetesUserProfilePart part)
        {
            part.ThrowIfNull();

            Qualifications = part.Qualifications.Select(x =>
            {
                var viewModel = new PersonQualificationViewModel();
                viewModel.UpdateViewModel(x);

                return viewModel;
            });
            GraduationIssuedBy = part.GraduationIssuedBy;
            GraduationYear = part.GraduationYear;
            OtherQualification = part.OtherQualification;
        }
    }
}
