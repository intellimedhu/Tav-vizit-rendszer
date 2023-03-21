using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.Services;
using OrganiMedCore.Organization.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Drivers
{
    public class OrganizationUnitPartDisplay : ContentPartDisplayDriver<OrganizationUnitPart>
    {
        private readonly IOrganizationService _organizationService;
        private readonly IEnumerable<IOrganizationUnitTypeProvider> _organizationUnitTypeProviders;


        public IStringLocalizer T { get; set; }


        public OrganizationUnitPartDisplay(
            IOrganizationService organizationService,
            IEnumerable<IOrganizationUnitTypeProvider> organizationUnitTypeProviders,
            IStringLocalizer<OrganizationUnitPartDisplay> stringLocalizer)
        {
            _organizationService = organizationService;
            _organizationUnitTypeProviders = organizationUnitTypeProviders;

            T = stringLocalizer;
        }


        public override IDisplayResult Display(OrganizationUnitPart part)
        {
            return Initialize<OrganizationUnitPartViewModel>("OrganizationUnitPart", viewModel => viewModel.UpdateViewModel(part))
                .Location("Detail", "Content:1")
                .Location("Summary", "Content:1");
        }

        public override async Task<IDisplayResult> EditAsync(OrganizationUnitPart part, BuildPartEditorContext context)
        {
            var organizationUnitTypes = await GetOrganizationUnitTypes();

            return Initialize<OrganizationUnitPartViewModel>("OrganizationUnitPart_Edit", viewModel =>
            {
                viewModel.UpdateViewModel(part);
                viewModel.OrganizationUnitTypes = organizationUnitTypes;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(OrganizationUnitPart part, IUpdateModel updater)
        {
            var viewModel = new OrganizationUnitPartViewModel();

            var success = await updater.TryUpdateModelAsync(viewModel, Prefix);

            viewModel.UpdatePart(part);

            if (!success)
            {
                return Edit(part);
            }

            var organizationUnits = await _organizationService.ListOrganizationUnitsAsync();
            if (organizationUnits.Any(x => x.Name == part.Name))
            {
                updater.ModelState.AddModelError("OrganizationUnitPart.Name", T["Az osztály nevének egyedinek kell lennie."]);
            }

            if (!string.IsNullOrEmpty(part.EesztId) && organizationUnits.Any(x => x.EesztId == part.EesztId))
            {
                updater.ModelState.AddModelError("OrganizationUnitPart.EesztId", T["Az EESZT névnek egyedinek kell lennie."]);
            }

            if (!string.IsNullOrEmpty(part.EesztName) && organizationUnits.Any(x => x.EesztName == part.EesztName))
            {
                updater.ModelState.AddModelError("OrganizationUnitPart.EesztName", T["Az EESZT azonosítónak egyedinek kell lennie."]);
            }

            if (!string.IsNullOrEmpty(part.OrganizationUnitType) && !(await GetOrganizationUnitTypes()).Contains(part.OrganizationUnitType))
            {
                updater.ModelState.AddModelError("OrganizationUnitPart.OrganizationUnitType", T["A megadott szervezeti egység típus nem létezik."]);
            }

            return Edit(part);
        }


        private async Task<IEnumerable<string>> GetOrganizationUnitTypes()
        {
            var organizationUnitTypes = await Task.WhenAll(_organizationUnitTypeProviders.Select(provider => provider.GetOrganizationUnitTypesAsync()));

            return organizationUnitTypes.SelectMany(x => x);
        }
    }
}
