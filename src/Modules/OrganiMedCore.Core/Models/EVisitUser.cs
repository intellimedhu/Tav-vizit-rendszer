using System.Collections.Generic;

namespace OrganiMedCore.Core.Models
{
    public class EVisitUser
    {
        public bool IsEVisitUser { get; set; }

        public bool EVisitLoginEnabled { get; set; }

        public HashSet<string> PermittedTenans { get; set; } = new HashSet<string>();
    }
}
