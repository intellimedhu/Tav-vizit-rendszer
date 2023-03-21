namespace OrganiMedCore.Core.Models
{
    /// <summary>
    /// Local record to indicate that the organization profile is now added to the organization.
    /// TODO: better naming?
    /// </summary>
    public class AssignedEVisitOrganizationProfile
    {
        public int Id { get; set; }

        public string EVisitOrganizationUserProfileId { get; set; }
    }
}
