namespace OrganiMedCore.DiabetesCareCenter.Core.Models.Enums
{
    public enum ColleagueAction
    {
        // Actions in green zone:
        RemoveActive,

        // Actions in pending zone:
        AcceptApplication,
        RejectApplication,
        ResendInvitation,

        // Actions in deleted zone:
        CancelInvitation,
    }
}
