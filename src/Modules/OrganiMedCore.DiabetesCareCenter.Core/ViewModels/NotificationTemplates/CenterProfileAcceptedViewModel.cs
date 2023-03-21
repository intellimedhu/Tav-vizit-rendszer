using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates
{
    public class CenterProfileAcceptedViewModel
    {
        public string PersonName { get; set; }

        public string CenterName { get; set; }

        public string CurrentRole { get; set; }

        public AccreditationStatus AccreditationStatus { get; set; }
    }
}
