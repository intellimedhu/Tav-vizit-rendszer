using IntelliMed.Core.Extensions;
using IntelliMed.Core.ViewModels;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfilePartViewModel : ShapeViewModel, IContentPartViewModel<CenterProfilePart>
    {
        public string LeaderName { get; set; }

        public bool Created { get; set; }

        public string CenterName { get; set; }

        public int CenterZipCode { get; set; }

        public string CenterSettlementName { get; set; }

        public string CenterAddress { get; set; }

        public string FullAddress => CenterZipCode + " " + CenterSettlementName + ", " + CenterAddress;

        public AccreditationStatus AccreditationStatus { get; set; }

        public DateTime AccreditationStatusDateUtc { get; set; }

        public HashSet<CenterType> CenterTypes { get; set; } = new HashSet<CenterType>();

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }


        public void UpdatePart(CenterProfilePart part)
        {
        }

        public void UpdateViewModel(CenterProfilePart part)
        {
            part.ThrowIfNull();

            Created = part.Created;
            CenterName = part.CenterName;
            CenterZipCode = part.CenterZipCode;
            CenterSettlementName = part.CenterSettlementName;
            CenterAddress = part.CenterAddress;
            AccreditationStatus = part.AccreditationStatus;
            AccreditationStatusDateUtc = part.AccreditationStatusDateUtc;
            CenterTypes = part.CenterTypes;
            Latitude = part.Latitude;
            Longitude = part.Longitude;
        }
    }
}
