using IntelliMed.Core.Extensions;
using OrganiMedCore.Login.Settings;
using OrganiMedCore.Login.Settings.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Login.ViewModels
{
    public class OmcLoginSettingsViewModel
    {
        public bool UseDokiNetLogin { get; set; }

        public string DokiNetLoginTitle { get; set; }

        public bool UseOrganiMedCoreLogin { get; set; }

        public bool UseLocalLogin { get; set; }

        [Required]
        public OmcLoginMethods? DefaultLoginMethod { get; set; }


        public void UpdateViewModel(OmcLoginSettings model)
        {
            model.ThrowIfNull();

            UseDokiNetLogin = model.UseDokiNetLogin;
            DokiNetLoginTitle = model.DokiNetLoginTitle;
            UseOrganiMedCoreLogin = model.UseOrganiMedCoreLogin;
            UseLocalLogin = model.UseLocalLogin;
            DefaultLoginMethod = model.DefaultLoginMethod;
        }

        public void UpdateModel(OmcLoginSettings model)
        {
            model.ThrowIfNull();

            model.UseDokiNetLogin = UseDokiNetLogin;
            model.DokiNetLoginTitle = DokiNetLoginTitle;
            model.UseOrganiMedCoreLogin = UseOrganiMedCoreLogin;
            model.UseLocalLogin = UseLocalLogin;
            model.DefaultLoginMethod = DefaultLoginMethod ?? OmcLoginMethods.Local;
        }
    }
}
