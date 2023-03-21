using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.Settings
{
    public class AccreditationPersonalConditionSettings
    {
        public IEnumerable<PersonalCondition> Accredited { get; set; } = new List<PersonalCondition>();

        public IEnumerable<PersonalCondition> TemporarilyAccredited { get; set; } = new List<PersonalCondition>();
    }
}
