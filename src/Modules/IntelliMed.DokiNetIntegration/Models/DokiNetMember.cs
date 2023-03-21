using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliMed.DokiNetIntegration.Models
{
    public class DokiNetMember
    {
        public int MemberId { get; set; }

        public int MemberRightId { get; set; }

        public string Prefix { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string StampNumber { get; set; }

        public string DiabetLicenceNumber { get; set; }

        public string MaidenName { get; set; }

        public string BornPlace { get; set; }

        public DateTime? BornDate { get; set; }

        public string ScientificDegree { get; set; }

        [JsonIgnore]
        public string FullName
        {
            get => string.Join(" ", new[] { Prefix, LastName, FirstName }.Where(x => !string.IsNullOrEmpty(x)));
        }

        public IEnumerable<string> Emails { get; set; } = new List<string>();

        public string PrivatePhone { get; set; }

        public string WebId { get; set; }

        public bool HasMemberShip { get; set; }

        public bool IsDueOk { get; set; }
    }
}
