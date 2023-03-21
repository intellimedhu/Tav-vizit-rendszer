using OrganiMedCore.Organization.Models.Enums;
using System;
using YesSql.Indexes;

namespace OrganiMedCore.Organization.Indexes
{
    public class CheckInPartIndex : MapIndex
    {
        public DateTime? CheckInDateUtc { get; set; }

        public CheckInStatuses CheckInStatus { get; set; }
    }
}