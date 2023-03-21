using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.Bootstrap.Models;
using OrganiMedCore.Bootstrap.Services;
using OrganiMedCore.Bootstrap.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Bootstrap.Drivers
{
    public class BootstrapStylePartDisplayDriver : ContentPartDisplayDriver<BootstrapStylePart>
    {
        private readonly IEnumerable<IBoostrapStyleProvider> _boostrapStyleProviders;


        public BootstrapStylePartDisplayDriver(IEnumerable<IBoostrapStyleProvider> boostrapStyleProviders)
        {
            _boostrapStyleProviders = boostrapStyleProviders;
        }


        public override IDisplayResult Display(BootstrapStylePart part)
            => Initialize<BootstrapStylePartViewModel>("BootstrapStyle", viewModel => viewModel.UpdateViewModel(part));

        public override async Task<IDisplayResult> EditAsync(BootstrapStylePart part, BuildPartEditorContext context)
        {
            var bsStyles = await CollectBoostrapStyles();

            return Initialize<BootstrapStylePartEditorViewModel>("BootstrapStyle_Edit", viewModel =>
            {
                viewModel.UpdateViewModel(part);
                viewModel.BootstrapStyles = bsStyles;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(BootstrapStylePart part, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(part, Prefix);

            if (!(await CollectBoostrapStyles()).Any(x => x.TechnicalName == part.Style))
            {
                updater.ModelState.AddModelError(nameof(BootstrapStylePartViewModel.Style), "The provided bootstrap value is invalid.");
            }

            return Edit(part);
        }


        private async Task<BootstrapStyle[]> CollectBoostrapStyles()
        {
            return (await Task.WhenAll(_boostrapStyleProviders.Select(provider => provider.GetStylesAsync())))
                .SelectMany(styles => styles)
                .ToArray();
        }
    }
}
