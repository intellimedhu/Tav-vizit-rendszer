namespace OrganiMedCore.DiabetesCareCenter.Core.Settings
{
    public class CenterManagerSettings
    {
        public string GoogleMapsApiKey { get; set; }

        public bool CenterProfileCacheEnabled { get; set; }

        public bool CalculatedStatusOverridable { get; set; }

        public bool TerritoryCacheEnabled { get; set; }
    }
}
