using OrganiMedCore.Login.Settings.Enums;

namespace OrganiMedCore.Login.Settings
{
    public class OmcLoginSettings
    {
        public bool UseDokiNetLogin { get; set; }

        public string DokiNetLoginTitle { get; set; }

        public bool UseOrganiMedCoreLogin { get; set; }

        /// <summary>
        /// Local login cannot be disabled only the local login button can be hidden.
        /// </summary>
        public bool UseLocalLogin { get; set; }

        public OmcLoginMethods DefaultLoginMethod { get; set; }
    }
}
