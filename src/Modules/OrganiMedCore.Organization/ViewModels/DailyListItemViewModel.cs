using OrchardCore.ContentManagement;
using OrganiMedCore.Core.Models;

namespace OrganiMedCore.Organization.ViewModels
{
    public class DailyListItemViewModel
    {
        public ContentItem CheckIn { get; set; }

        public ContentItem EVisitPatientProfile { get; set; }

        public ContentItem OrganizationUnit { get; set; }

        public EVisitOrganizationUserProfilePart Doctor { get; set; }
    }
}
