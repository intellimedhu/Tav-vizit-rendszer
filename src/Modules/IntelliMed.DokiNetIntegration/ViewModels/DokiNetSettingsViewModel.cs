using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Settings;
using System.ComponentModel.DataAnnotations;

namespace IntelliMed.DokiNetIntegration.ViewModels
{
    public class DokiNetSettingsViewModel
    {
        [Required]
        public int? SocietyId { get; set; }

        public string PreSharedKey { get; set; }

        public string DokiNetBaseUrl { get; set; }

        public int MaxMembersCountPerRequest { get; set; }


        public void UpdateViewModel(DokiNetSettings model)
        {
            model.ThrowIfNull();

            SocietyId = model.SocietyId;
            PreSharedKey = model.PreSharedKey;
            DokiNetBaseUrl = model.DokiNetBaseUrl;
            MaxMembersCountPerRequest = model.MaxMembersCountPerRequest;
        }

        public void UpdateModel(DokiNetSettings model)
        {
            model.ThrowIfNull();

            model.SocietyId = SocietyId;
            model.PreSharedKey = PreSharedKey;
            model.DokiNetBaseUrl = DokiNetBaseUrl;
            model.MaxMembersCountPerRequest = MaxMembersCountPerRequest;
        }
    }
}
