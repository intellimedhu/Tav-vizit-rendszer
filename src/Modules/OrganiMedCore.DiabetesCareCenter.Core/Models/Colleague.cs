using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class Colleague
    {
        public Guid Id { get; set; }

        public int? MemberRightId { get; set; }

        public string Prefix { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string CenterProfileContentItemId { get; set; }

        public string CenterProfileContentItemVersionId { get; set; }

        public Occupation? Occupation { get; set; }

        public IList<ColleagueStatusItem> StatusHistory { get; set; } = new List<ColleagueStatusItem>();

        [JsonIgnore]
        public ColleagueStatusItem LatestStatusItem
        {
            get => StatusHistory.OrderByDescending(x => x.StatusDateUtc).FirstOrDefault();
        }

        [JsonIgnore]
        public string FullName
        {
            get => string.Join(
                " ",
                new[] { Prefix, LastName, FirstName }.Where(x => !string.IsNullOrWhiteSpace(x) && !string.IsNullOrEmpty(x)));
        }
    }
}
