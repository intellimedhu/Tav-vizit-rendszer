namespace OrganiMedCore.DiabetesCareCenter.Core.Models.Enums
{
    public enum ColleagueStatus
    {
        // Green zone
        ApplicationAccepted = 0,
        PreExisting = 1,
        InvitationAccepted = 9,

        // Pending zone
        ApplicationSubmitted = 2,
        Invited = 3,

        // Removed zone
        DeletedByLeader = 4,
        InvitationRejected = 5,
        DeletedByColleague = 6,
        ApplicationRejected = 7,
        InvitationCancelled = 8,
        ApplicationCancelled = 10
    }
}
