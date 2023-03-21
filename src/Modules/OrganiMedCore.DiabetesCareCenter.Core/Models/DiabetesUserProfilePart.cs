using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class DiabetesUserProfilePart : ContentPart
    {
        public int MemberRightId { get; set; }

        public IEnumerable<PersonQualification> Qualifications { get; set; } = new List<PersonQualification>();

        public string GraduationIssuedBy { get; set; }

        public int? GraduationYear { get; set; }

        public string OtherQualification { get; set; }

        [JsonIgnore]
        public bool HasGraduation
        {
            get => !string.IsNullOrEmpty(GraduationIssuedBy) && GraduationYear.HasValue;
        }
    }
}
