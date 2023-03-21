using Newtonsoft.Json;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class PersonDataCompactViewModel
    {
        [JsonProperty("personQualifications")]
        public DiabetesUserProfilePartViewModel PersonQualifications { get; set; }

        [JsonProperty("qualifications")]
        public IEnumerable<QualificationViewModel> Qualifications { get; set; } = new List<QualificationViewModel>();

        public string PrivatePhone { get; set; }

        public bool HasMembership { get; set; }

        public bool IsMembershipFeePaid { get; set; }
    }
}
