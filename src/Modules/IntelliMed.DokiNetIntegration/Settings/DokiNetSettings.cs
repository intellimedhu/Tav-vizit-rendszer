namespace IntelliMed.DokiNetIntegration.Settings
{
    public class DokiNetSettings
    {
        public int? SocietyId { get; set; }

        public string PreSharedKey { get; set; }

        public string DokiNetBaseUrl { get; set; }

        public int MaxMembersCountPerRequest { get; set; }
    }
}
