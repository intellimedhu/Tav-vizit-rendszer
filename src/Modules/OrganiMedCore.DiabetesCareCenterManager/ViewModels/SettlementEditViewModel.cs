using OrganiMedCore.DiabetesCareCenterManager.Constants;
using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class SettlementEditViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        [MaxLength(DataFieldsProperties.SettlementNameMaxLength)]
        [Required]
        public string Name { get; set; }

        [JsonProperty("zipCode")]
        [Range(1000, 9999)]
        public int ZipCode { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }


        public static SettlementEditViewModel ToViewModel(Settlement model)
        {
            if (model == null)
            {
                return null;
            }

            return new SettlementEditViewModel()
            {
                Id = model.Id,
                Description = model.Description,
                Name = model.Name,
                ZipCode = model.ZipCode
            };
        }

        public static Settlement ToModel(SettlementEditViewModel viewModel)
        {
            if (viewModel == null)
            {
                return null;
            }

            return new Settlement()
            {
                Id = viewModel.Id,
                Description = viewModel.Description,
                Name = viewModel.Name,
                ZipCode = viewModel.ZipCode
            };
        }
    }
}
