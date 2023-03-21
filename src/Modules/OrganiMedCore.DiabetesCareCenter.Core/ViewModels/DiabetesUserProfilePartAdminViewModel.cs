using IntelliMed.Core.Extensions;
using IntelliMed.Core.ViewModels;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class DiabetesUserProfilePartAdminViewModel : ShapeViewModel, IContentPartViewModel<DiabetesUserProfilePart>
    {
        public int MemberRightId { get; set; }

        public int QualificationsCount { get; set; }

        public string GraduationIssuedBy { get; set; }

        public int? GraduationYear { get; set; }

        public string OtherQualification { get; set; }

        public string DiabetLicenceNumber { get; set; }


        public void UpdatePart(DiabetesUserProfilePart part)
        {
        }

        public void UpdateViewModel(DiabetesUserProfilePart part)
        {
            part.ThrowIfNull();

            MemberRightId = part.MemberRightId;
            QualificationsCount = part.Qualifications.Count();
            GraduationIssuedBy = part.GraduationIssuedBy;
            GraduationYear = part.GraduationYear;
            OtherQualification = part.OtherQualification;
        }
    }
}
