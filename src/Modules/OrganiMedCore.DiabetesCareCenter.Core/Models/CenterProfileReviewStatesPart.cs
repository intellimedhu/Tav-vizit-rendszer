using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class CenterProfileReviewStatesPart : ContentPart
    {
        public IList<CenterProfileReviewState> States { get; set; } = new List<CenterProfileReviewState>();
    }
}
