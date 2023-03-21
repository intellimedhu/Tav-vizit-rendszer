using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class AccreditationPersonalCondition
    {
        public Occupation Occupation { get; set; }

        public int RequiredHeadcount { get; set; }

        public int? HeadCount { get; set; }

        public IList<string> UnqualifiedPeople { get; set; } = new List<string>();
    }
}
