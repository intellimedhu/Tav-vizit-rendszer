using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using System.Collections.Generic;

namespace IntelliMed.DokiNetIntegration.ViewModels
{
    public class EditDokiNetMemberViewModel
    {
        public int? MemberRightId { get; set; }

        public string FullName { get; set; }

        public IEnumerable<string> Emails { get; set; } = new List<string>();

        public string UserName { get; set; }

        /// <summary>
        /// Prefix for the Html field.
        /// </summary>
        public string Prefix { get; internal set; }


        public virtual void UpdateViewModel(DokiNetMember member)
        {
            member.ThrowIfNull();

            MemberRightId = member.MemberRightId;
            FullName = member.FullName;
            Emails = member.Emails;
            UserName = member.UserName;
        }
    }
}
