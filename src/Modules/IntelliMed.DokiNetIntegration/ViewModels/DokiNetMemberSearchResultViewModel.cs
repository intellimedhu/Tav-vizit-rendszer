using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using System.Collections.Generic;
using System.Linq;

namespace IntelliMed.DokiNetIntegration.ViewModels
{
    public class DokiNetMemberSearchResultViewModel
    {
        public int MemberId { get; set; }

        public int MemberRightId { get; set; }

        public string Prefix { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string StampNumber { get; set; }

        public string DiabetLicenceNumber { get; set; }

        public string FullName
        {
            get => string.Join(" ", new[] { Prefix, LastName, FirstName }.Where(x => !string.IsNullOrEmpty(x)));
        }

        public IEnumerable<string> Emails { get; set; } = new List<string>();

        public bool HasMemberShip { get; set; }

        public bool IsMembershipFeePaid { get; set; }


        public void UpdateViewModel(DokiNetMember model)
        {
            model.ThrowIfNull();

            MemberId = model.MemberId;
            MemberRightId = model.MemberRightId;
            Prefix = model.Prefix;
            FirstName = model.FirstName;
            LastName = model.LastName;
            StampNumber = model.StampNumber;
            DiabetLicenceNumber = model.DiabetLicenceNumber;
            HasMemberShip = model.HasMemberShip;
            IsMembershipFeePaid = model.IsDueOk;
            Emails = new List<string>(model.Emails);
        }
    }
}
