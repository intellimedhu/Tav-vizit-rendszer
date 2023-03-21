namespace OrganiMedCore.DiabetesCareCenter.Core.Helpers
{
    public class CenterProfileReviewAuthorizationResult
    {
        public string CurrentRole { get; set; }

        public bool IsAuthorized { get; set; }


        public static CenterProfileReviewAuthorizationResult Default
            => new CenterProfileReviewAuthorizationResult()
            {
                CurrentRole = string.Empty,
                IsAuthorized = false
            };
    }
}
