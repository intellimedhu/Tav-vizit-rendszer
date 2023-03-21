using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrganiMedCore.Login.Constants;
using OrganiMedCore.Login.Settings;
using OrganiMedCore.Login.Settings.Enums;
using OrganiMedCore.Login.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Drivers
{
    public class OmcLoginSettingsDisplayDriver : SectionDisplayDriver<ISite, OmcLoginSettings>
    {
        public IStringLocalizer T { get; set; }


        public OmcLoginSettingsDisplayDriver(IStringLocalizer<OmcLoginSettingsDisplayDriver> stringLocalizer)
        {
            T = stringLocalizer;
        }


        public override IDisplayResult Edit(OmcLoginSettings section, BuildEditorContext context)
        {
            Prefix = string.Empty;

            return Initialize<OmcLoginSettingsViewModel>("OmcLoginSettings_Edit", viewModel => viewModel.UpdateViewModel(section))
                .Location("Content:1")
                .OnGroup(GroupIds.OrganiMedCoreLoginSettings);
        }

        public async override Task<IDisplayResult> UpdateAsync(OmcLoginSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupIds.OrganiMedCoreLoginSettings)
            {
                var viewModel = new OmcLoginSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(viewModel);

                if (!viewModel.UseDokiNetLogin &&
                    !viewModel.UseOrganiMedCoreLogin &&
                    !viewModel.UseLocalLogin)
                {
                    context.Updater.ModelState.AddModelError(string.Empty, T["Legalább egy módszer kiválasztása kötelező"].Value);
                }

                if (!viewModel.UseDokiNetLogin && viewModel.DefaultLoginMethod == OmcLoginMethods.DokiNet)
                {
                    context.Updater.ModelState.AddModelError(
                        string.Empty,
                        T["Nem választható ki a doki.NET bejelentkezés alapértelmezettként, ha nem lehetséges úgy a bejelentkezés"].Value);
                }

                if (!viewModel.UseOrganiMedCoreLogin && viewModel.DefaultLoginMethod == OmcLoginMethods.OrganiMedCore)
                {
                    context.Updater.ModelState.AddModelError(
                        string.Empty,
                        T["Nem választható ki az OrganiMed bejelentkezés alapértelmezettként, ha nem lehetséges úgy a bejelentkezés"].Value);
                }

                if (!viewModel.UseLocalLogin && viewModel.DefaultLoginMethod == OmcLoginMethods.Local)
                {
                    context.Updater.ModelState.AddModelError(
                        string.Empty,
                        T["Nem választható ki a lokális bejelentkezés alapértelmezettként, ha nem lehetséges úgy a bejelentkezés"].Value);
                }

                viewModel.UpdateModel(section);
            }

            return await EditAsync(section, context);
        }
    }
}
