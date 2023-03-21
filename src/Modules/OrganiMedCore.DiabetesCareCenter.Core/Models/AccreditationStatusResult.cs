using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class AccreditationStatusResult
    {
        public AccreditationStatus AccreditationStatus { get; set; }

        public List<AccreditationPersonalCondition> PersonalConditions { get; set; } = new List<AccreditationPersonalCondition>();

        public bool MdtLicence { get; set; }

        public HashSet<string> Membership { get; set; } = new HashSet<string>();

        public HashSet<string> MembershipFee { get; set; } = new HashSet<string>();

        public HashSet<string> Tools { get; set; } = new HashSet<string>();

        public HashSet<string> Laboratory { get; set; } = new HashSet<string>();

        public bool BackgroundConcilium { get; set; }

        public bool BackgroundInpatient { get; set; }
    }
}
