using Microsoft.AspNetCore.Mvc.Localization;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.Extensions
{
    public static class ColleagueStatusExtensions
    {
        public static readonly ColleagueStatus[] AllowedStatusesToApply =
        {
            ColleagueStatus.Invited,
            ColleagueStatus.InvitationRejected,
            ColleagueStatus.DeletedByColleague,
            ColleagueStatus.InvitationCancelled,
            ColleagueStatus.ApplicationCancelled
        };

        public static readonly ColleagueStatus[] AllowedStatusesToCancel =
        {
            ColleagueStatus.ApplicationAccepted,
            ColleagueStatus.PreExisting,
            ColleagueStatus.InvitationAccepted,
            ColleagueStatus.ApplicationSubmitted,
            ColleagueStatus.Invited
        };

        public static readonly ColleagueStatus[] GreenZone =
        {
            ColleagueStatus.ApplicationAccepted,
            ColleagueStatus.PreExisting,
            ColleagueStatus.InvitationAccepted
        };

        public static readonly ColleagueStatus[] PendingZone =
        {
            ColleagueStatus.ApplicationSubmitted,
            ColleagueStatus.Invited
        };

        public static readonly ColleagueStatus[] RemovedZone =
        {
            ColleagueStatus.DeletedByLeader,
            ColleagueStatus.InvitationRejected,
            ColleagueStatus.DeletedByColleague,
            ColleagueStatus.ApplicationRejected,
            ColleagueStatus.InvitationCancelled,
            ColleagueStatus.ApplicationCancelled
        };

        public static IDictionary<ColleagueStatus, string> GetLocalizedValues(IHtmlLocalizer t)
            => ((ColleagueStatus[])Enum.GetValues(typeof(ColleagueStatus)))
                .ToDictionary(
                    status => status,
                    status =>
                    {
                        switch (status)
                        {
                            case ColleagueStatus.ApplicationAccepted:
                                return t["Jelentkezés elfogadva"].Value;

                            case ColleagueStatus.PreExisting:
                                return t["Meglévő"].Value;

                            case ColleagueStatus.ApplicationSubmitted:
                                return t["Jelentkezés"].Value;

                            case ColleagueStatus.Invited:
                                return t["Meghívva"].Value;

                            case ColleagueStatus.DeletedByLeader:
                                return t["Törölve vezető által"].Value;

                            case ColleagueStatus.InvitationRejected:
                                return t["Meghívó elutasítva"].Value;

                            case ColleagueStatus.DeletedByColleague:
                                return t["Törölve munkatárs által"].Value;

                            case ColleagueStatus.ApplicationRejected:
                                return t["Jelentkezés elutasítva"].Value;

                            case ColleagueStatus.InvitationCancelled:
                                return t["Meghívó visszavonva"].Value;

                            case ColleagueStatus.InvitationAccepted:
                                return t["Meghívás elfogadva"].Value;

                            case ColleagueStatus.ApplicationCancelled:
                                return t["Jelentkezés visszavonva"].Value;

                            default:
                                return status.ToString();
                        }
                    }
                );
    }
}
