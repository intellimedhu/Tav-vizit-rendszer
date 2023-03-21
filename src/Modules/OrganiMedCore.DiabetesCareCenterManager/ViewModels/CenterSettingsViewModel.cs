using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class CenterSettingsViewModel
    {
        public string GoogleMapsApiKey { get; set; }

        public bool CenterProfileCacheEnabled { get; set; }

        public bool ForceEmptyCache { get; set; }

        public bool CalculatedStatusOverridable { get; set; }

        public bool TerritoryCacheEnabled { get; set; }


        public void UpdateViewModel(CenterManagerSettings settings)
        {
            settings.ThrowIfNull();

            GoogleMapsApiKey = settings.GoogleMapsApiKey;
            CenterProfileCacheEnabled = settings.CenterProfileCacheEnabled;
            CalculatedStatusOverridable = settings.CalculatedStatusOverridable;
            TerritoryCacheEnabled = settings.TerritoryCacheEnabled;
        }

        public void UpdateModel(CenterManagerSettings settings)
        {
            settings.ThrowIfNull();

            settings.GoogleMapsApiKey = GoogleMapsApiKey;
            settings.CenterProfileCacheEnabled = CenterProfileCacheEnabled;
            settings.CalculatedStatusOverridable = CalculatedStatusOverridable;
            settings.TerritoryCacheEnabled = TerritoryCacheEnabled;
        }
    }
}
