namespace OrganiMedCore.DiabetesCareCenter.Core.Helpers
{
    public class ReviewStatisticsByRoles
    {
        public string RoleName { get; set; }

        public int ReviewableCount { get; set; }

        public int ReviewedCount { get; set; }

        public int NonReviewableCount { get; set; }
    }
}
