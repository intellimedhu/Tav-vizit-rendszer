using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.UriAuthentication.ViewModels
{
    public class NonceSettingsViewModel
    {
        [Required]
        public int? NonceExpirationInDays { get; set; }
    }
}
