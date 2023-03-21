using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Login.ViewModels
{
    /// <summary>
    /// This is a copy of <see cref="OrchardCore.Users.ViewModels.RegisterViewModel"/> except that the UserName was removed.
    /// </summary>
    public class EVisitRegisterViewModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
