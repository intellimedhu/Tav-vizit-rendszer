using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using YesSql.Indexes;

namespace OrganiMedCore.DiabetesCareCenter.Core.Indexes
{
    // Check out: CenterProfileService.GetPermittedCenterProfileQueryAsync method when modifying this index
    public class CenterProfilePartIndex : MapIndex
    {
        public int MemberRightId { get; set; }

        public bool Created { get; set; }

        public int CenterZipCode { get; set; }

        public AccreditationStatus AccreditationStatus { get; set; }

        public DateTime AccreditationStatusDateUtc { get; set; }
    }
}
