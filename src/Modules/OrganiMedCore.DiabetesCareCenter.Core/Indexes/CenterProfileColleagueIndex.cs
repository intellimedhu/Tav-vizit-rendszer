using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using YesSql.Indexes;

namespace OrganiMedCore.DiabetesCareCenter.Core.Indexes
{
    public class CenterProfileColleagueIndex : MapIndex
    {
        public string CenterProfileContentItemId { get; set; }

        public string CenterProfileContentItemVersionId { get; set; }

        public Guid ColleagueId { get; set; }

        public int? MemberRightId { get; set; }

        public string Prefix { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public Occupation? Occupation { get; set; }
    }
}
