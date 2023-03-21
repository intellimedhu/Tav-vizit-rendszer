using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class CenterProfilePart : ContentPart
    {
        public int MemberRightId { get; set; }

        public bool Created { get; set; }

        public string CenterName { get; set; }

        public string CenterLeaderEmail { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public string Web { get; set; }

        public int CenterZipCode { get; set; }

        public string CenterSettlementName { get; set; }

        public string CenterAddress { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        public AccreditationStatus AccreditationStatus { get; set; }

        public DateTime AccreditationStatusDateUtc { get; set; }

        public HashSet<CenterType> CenterTypes { get; set; } = new HashSet<CenterType>();

        #region Additional data

        public bool VocationalClinic { get; set; }

        public bool PartOfOtherVocationalClinic { get; set; }

        public string OtherVocationalClinic { get; set; }

        /// <summary>
        /// There is no NEAK contract if this property is empty.
        /// </summary>
        public IEnumerable<CenterProfileNeakData> Neak { get; set; } = new List<CenterProfileNeakData>();

        public CenterProfileAntszData Antsz { get; set; }

        public IEnumerable<DailyOfficeHours> OfficeHours { get; set; } = new List<DailyOfficeHours>();

        #endregion

        #region Equipments

        public IEnumerable<CenterProfileEquipment<int?>> Tools { get; set; } = new List<CenterProfileEquipment<int?>>();

        public IEnumerable<CenterProfileEquipment<bool>> Laboratory { get; set; } = new List<CenterProfileEquipment<bool>>();

        public bool BackgroundConcilium { get; set; }

        public bool BackgroundInpatient { get; set; }

        #endregion

        #region Colleagues

        public IList<Colleague> Colleagues { get; set; } = new List<Colleague>();

        #endregion
    }
}