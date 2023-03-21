namespace OrganiMedCore.DiabetesCareCenter.Core.Models.Enums
{
    public enum CenterProfileStatus
    {
        /// <summary>
        /// The center leader has not submitted the profile.
        /// </summary>
        Unsubmitted = 0,

        /// <summary>
        /// The center leader has submitted the profile but the territorial rapporteur has not reacted yet.
        /// </summary>
        UnderReviewAtTR = 1,

        /// <summary>
        /// The territorial rapporteur has accepted the profile, but none of the OMKB members have reacted yet.
        /// </summary>
        UnderReviewAtOMKB = 2,

        /// <summary>
        /// The OMKB has accepted the profile, but the MDT management has not reacted yet.
        /// </summary>
        UnderReviewAtMDT = 3,

        /// <summary>
        /// The MDT management has accepted the profile.
        /// </summary>
        MDTAccepted = 4
    }
}
