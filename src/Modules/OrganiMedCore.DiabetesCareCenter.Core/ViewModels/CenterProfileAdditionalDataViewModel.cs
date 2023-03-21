using IntelliMed.Core.Validators;
using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileAdditionalDataViewModel : ICenterProfileViewModel
    {
        [JsonProperty("vocationalClinic")]
        public bool? VocationalClinic { get; set; }

        [JsonProperty("partOfOtherVocationalClinic")]
        public bool? PartOfOtherVocationalClinic { get; set; }

        [JsonProperty("otherVocationalClinic")]
        public string OtherVocationalClinic { get; set; }

        [JsonProperty("neak")]
        public IEnumerable<CenterProfileNeakDataViewModel> Neak { get; set; } = new List<CenterProfileNeakDataViewModel>();

        [Required(ErrorMessage = "Helytelen ANTSZ adatok")]
        [JsonProperty("antsz")]
        public CenterProfileAntszDataViewModel Antsz { get; set; }

        [NotEmpty(ErrorMessage = "Legalább egy Rendelési idő megadása kötelező")]
        [JsonProperty("officeHours")]
        public IEnumerable<DailyOfficeHours> OfficeHours { get; set; } = new List<DailyOfficeHours>();


        public void UpdatePart(CenterProfilePart part)
        {
            part.VocationalClinic = VocationalClinic == true;
            part.PartOfOtherVocationalClinic = PartOfOtherVocationalClinic == true;
            part.OtherVocationalClinic = OtherVocationalClinic;
            part.OfficeHours = OfficeHours;
            part.Neak = Neak.Select(neak =>
            {
                var model = new CenterProfileNeakData();
                neak.UpdateModel(model);

                return model;
            });

            part.Antsz = new CenterProfileAntszData();
            if (Antsz != null)
            {
                Antsz.UpdateModel(part.Antsz);
            }
        }

        public void UpdateViewModel(CenterProfilePart part)
        {
            VocationalClinic = part.VocationalClinic;
            PartOfOtherVocationalClinic = part.PartOfOtherVocationalClinic;
            OtherVocationalClinic = part.OtherVocationalClinic;
            Neak = part.Neak.Select(neak =>
            {
                var viewModel = new CenterProfileNeakDataViewModel();
                viewModel.UpdateViewModel(neak);

                return viewModel;
            });
            OfficeHours = part.OfficeHours;

            Antsz = new CenterProfileAntszDataViewModel();
            if (part.Antsz != null)
            {
                Antsz.UpdateViewModel(part.Antsz);
            }
        }
    }
}
