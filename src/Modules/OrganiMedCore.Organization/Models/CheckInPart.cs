using OrchardCore.ContentManagement;
using OrganiMedCore.Organization.Models.Enums;
using System;

namespace OrganiMedCore.Organization.Models
{
    public class CheckInPart : ContentPart
    {
        public DateTime? CheckInDateUtc { get; set; }

        public CheckInStatuses CheckInStatus { get; set; }
    }
}
