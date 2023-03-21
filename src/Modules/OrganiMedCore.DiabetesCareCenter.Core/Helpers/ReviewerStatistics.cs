using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.Helpers
{
    public class ReviewerStatistics
    {
        public int TotalCount { get; set; }

        public List<ReviewStatisticsByRoles> ReviewStatisticsByRoles { get; set; } = new List<ReviewStatisticsByRoles>();

        public List<string> ReviewableContentItemIds { get; set; } = new List<string>();
    }
}
