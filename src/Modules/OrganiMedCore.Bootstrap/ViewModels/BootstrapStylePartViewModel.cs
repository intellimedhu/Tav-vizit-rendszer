using IntelliMed.Core.Extensions;
using IntelliMed.Core.ViewModels;
using OrganiMedCore.Bootstrap.Models;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Bootstrap.ViewModels
{
    public class BootstrapStylePartViewModel : IContentPartViewModel<BootstrapStylePart>
    {
        [Required]
        public string Style { get; set; }


        public void UpdatePart(BootstrapStylePart part)
        {
            part.ThrowIfNull();

            part.Style = Style;
        }

        public void UpdateViewModel(BootstrapStylePart part)
        {
            part.ThrowIfNull();

            Style = part.Style;
        }
    }
}
