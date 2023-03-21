using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using OrchardCore.Entities;
using OrchardCore.Users.Models;

namespace IntelliMed.DokiNetIntegration.Extensions
{
    public static class UserExtensions
    {
        public static void UpdateUsersDokiNetMemberData(this User user, DokiNetMember member)
        {
            user.ThrowIfNull();

            if (member == null)
            {
                user.Properties.Remove(nameof(DokiNetMember));

                return;
            }

            user.Alter<DokiNetMember>(nameof(DokiNetMember), entity => entity.UpdateDokiNetMemberData(member));
        }

        public static void UpdateDokiNetMemberData(this DokiNetMember toUpdate, DokiNetMember updater)
        {
            toUpdate.BornDate = updater.BornDate;
            toUpdate.BornPlace = updater.BornPlace;
            toUpdate.DiabetLicenceNumber = updater.DiabetLicenceNumber;
            toUpdate.Emails = updater.Emails;
            toUpdate.FirstName = updater.FirstName;
            toUpdate.HasMemberShip = updater.HasMemberShip;
            toUpdate.IsDueOk = updater.IsDueOk;
            toUpdate.LastName = updater.LastName;
            toUpdate.MaidenName = updater.MaidenName;
            toUpdate.MemberId = updater.MemberId;
            toUpdate.MemberRightId = updater.MemberRightId;
            toUpdate.Prefix = updater.Prefix;
            toUpdate.PrivatePhone = updater.PrivatePhone;
            toUpdate.ScientificDegree = updater.ScientificDegree;
            toUpdate.StampNumber = updater.StampNumber;
            toUpdate.UserName = updater.UserName;
            if (!string.IsNullOrEmpty(updater.WebId))
            {
                toUpdate.WebId = updater.WebId;
            }
        }
    }
}
