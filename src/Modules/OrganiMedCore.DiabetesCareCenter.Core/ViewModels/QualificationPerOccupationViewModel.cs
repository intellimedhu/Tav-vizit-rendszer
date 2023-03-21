using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class QualificationPerOccupationViewModel
    {
        public Occupation Occupation { get; set; }

        public Guid QualificationId { get; set; }

        public bool IsSelected { get; set; }
    }
}
