using System;
using System.Collections.Generic;

namespace OrganiMedCore.Manager.ViewModels
{
    public class AccessLogViewModel
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<AccessLogEntryViewModel> Entries { get; set; }
    }
}
