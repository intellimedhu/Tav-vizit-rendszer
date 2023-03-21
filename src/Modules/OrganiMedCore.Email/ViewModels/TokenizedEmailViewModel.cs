using IntelliMed.Core.Extensions;
using Newtonsoft.Json;
using OrganiMedCore.Email.Models;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Email.ViewModels
{
    public class TokenizedEmailViewModel
    {
        [Required]
        [JsonProperty("templateId")]
        public string TemplateId { get; set; }

        [Required]
        [JsonProperty("rawBody")]
        public string RawBody { get; set; }

        [Required]
        [JsonProperty("subject")]
        public string Subject { get; set; }


        public void UpdateModel(TokenizedEmail model)
        {
            model.ThrowIfNull();

            model.TemplateId = TemplateId;
            model.RawBody = RawBody;
            model.Subject = Subject;
        }

        public void UpdateViewModel(TokenizedEmail model)
        {
            model.ThrowIfNull();

            TemplateId = model.TemplateId;
            RawBody = model.RawBody;
            Subject = model.Subject;
        }
    }
}
