using IntelliMed.Core.Extensions;
using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileAntszDataViewModel
    {
        [Required(ErrorMessage = "Helytelen az ANTSZ száma")]
        [JsonProperty("number")]
        public string Number { get; set; }

        [Required(ErrorMessage = "Helytelen az ANTSZ kelte")]
        [JsonProperty("date")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Helytelen az ANTSZ azonosító kód")]
        [JsonProperty("id")]
        public string Id { get; set; }


        public void UpdateModel(CenterProfileAntszData model)
        {
            model.ThrowIfNull();

            model.Date = Date;
            model.Number = Number;
            model.Id = Id;
        }

        public void UpdateViewModel(CenterProfileAntszData model)
        {
            model.ThrowIfNull();

            Number = model.Number;
            Date = model.Date;
            Id = model.Id;
        }
    }
}
