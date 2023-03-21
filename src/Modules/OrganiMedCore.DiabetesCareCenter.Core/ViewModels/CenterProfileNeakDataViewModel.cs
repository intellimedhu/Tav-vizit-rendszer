using IntelliMed.Core.Extensions;
using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileNeakDataViewModel
    {
        [JsonProperty("primary")]
        public bool Primary { get; set; }

        [Required(ErrorMessage = "Kérjük, adja meg a heti óraszámot")]
        [JsonProperty("numberOfHours")]
        public int? NumberOfHours { get; set; }

        [Required(ErrorMessage = "Kérjük, adja meg a cukorbetegekre fordított heti óraszámot")]
        [JsonProperty("numberOfHoursDiabetes")]
        public int? NumberOfHoursDiabetes { get; set; }

        [Required(ErrorMessage = "Kérjük, adja meg a munkahelyi kódot")]
        [JsonProperty("workplaceCode")]
        public string WorkplaceCode { get; set; }


        public void UpdateModel(CenterProfileNeakData model)
        {
            model.ThrowIfNull();

            model.Primary = Primary;
            model.WorkplaceCode = WorkplaceCode;

            if (NumberOfHours.HasValue)
            {
                model.NumberOfHours = NumberOfHours.Value;
            }

            if (NumberOfHoursDiabetes.HasValue)
            {
                model.NumberOfHoursDiabetes = NumberOfHoursDiabetes.Value;
            }
        }

        public void UpdateViewModel(CenterProfileNeakData model)
        {
            model.ThrowIfNull();

            Primary = model.Primary;
            NumberOfHours = model.NumberOfHours;
            NumberOfHoursDiabetes = model.NumberOfHoursDiabetes;
            WorkplaceCode = model.WorkplaceCode;
        }
    }
}
