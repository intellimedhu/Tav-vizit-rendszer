using IntelliMed.DokiNetIntegration.Settings;

namespace IntelliMed.DokiNetIntegration.Extensions
{
    public static class DokiNetSettingsExtensions
    {
        public static string GetAuthorization(this DokiNetSettings settings)
            => $"{settings.PreSharedKey}:{settings.SocietyId}";
    }
}
