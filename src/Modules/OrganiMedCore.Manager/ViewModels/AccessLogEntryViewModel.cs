using System;

namespace OrganiMedCore.Manager.ViewModels
{
    public class AccessLogEntryViewModel
    {
        public DateTime Date { get; set; }
        public string OrganizationId { get; set; }
        public string Message { get; set; }
    }
}
