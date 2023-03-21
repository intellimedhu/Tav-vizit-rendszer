using OrchardCore.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Bootstrap.Models
{
    public class BootstrapStylePart : ContentPart
    {
        [Required]
        public string Style { get; set; }
    }
}
