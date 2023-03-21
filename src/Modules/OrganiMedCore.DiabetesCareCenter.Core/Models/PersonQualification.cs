using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class PersonQualification
    {
        public Guid QualificationId { get; set; }

        public string QualificationNumber { get; set; }

        public int? QualificationYear { get; set; }

        public QualificationState? State { get; set; }
    }
}
