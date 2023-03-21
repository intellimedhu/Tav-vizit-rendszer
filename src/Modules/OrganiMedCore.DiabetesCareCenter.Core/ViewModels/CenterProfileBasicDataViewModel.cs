using IntelliMed.Core.Constants;
using IntelliMed.Core.Extensions;
using IntelliMed.Core.Validators;
using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileBasicDataViewModel : ICenterProfileViewModel
    {
        [Required(ErrorMessage = "A szakellátóhely nevének megadása kötelező.")]
        [JsonProperty("centerName")]
        public string CenterName { get; set; }

        [Required(ErrorMessage = "Az irányítószám megadása kötelező.")]
        [Range(1000, 9999, ErrorMessage = "Az irányítószám 4 db számjegy.")]
        [JsonProperty("centerZipCode")]
        public int? CenterZipCode { get; set; }

        [JsonIgnore]
        public bool Created { get; set; }

        [Required(ErrorMessage = "A település megadása kötelező.")]
        [JsonProperty("centerSettlement")]
        public string CenterSettlementName { get; set; }

        [Required(ErrorMessage = "A cím megadása kötelező.")]
        [JsonProperty("centerAddress")]
        public string CenterAddress { get; set; }

        [JsonProperty("fullAddress")]
        public string FullAddress => CenterZipCode + " " + CenterSettlementName + ", " + CenterAddress;

        [JsonProperty("centerLatitude")]
        public float? Latitude { get; set; }

        [JsonProperty("centerLongitude")]
        public float? Longitude { get; set; }

        [JsonProperty("accreditationStatus")]
        public AccreditationStatus AccreditationStatus { get; set; }

        [JsonProperty("accreditationStatusDateUtc")]
        public DateTime AccreditationStatusDateUtc { get; set; }

        [NotEmpty(ErrorMessage = "Legalább egy profil típust kötelező választani.")]
        [JsonProperty("centerTypes")]
        public HashSet<CenterType> CenterTypes { get; set; } = new HashSet<CenterType>();

        [Required(ErrorMessage = "A telefonszám megadása kötelező.")]
        [JsonProperty("phone")]
        [RegularExpression(RegexPatterns.Phone, ErrorMessage = "A telefonszám formátuma nem megfelelő.")]
        public string Phone { get; set; }

        [JsonProperty("fax")]
        [RegularExpression(RegexPatterns.Phone, ErrorMessage = "A fax formátuma nem megfelelő.")]
        public string Fax { get; set; }

        [Required(ErrorMessage = "Az email megadása kötelező.")]
        [JsonProperty("email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Az email cím formátuma nem megfelelő.")]
        public string Email { get; set; }

        [JsonProperty("web")]
        [DataType(DataType.Url, ErrorMessage = "Helytelen weboldal.")]
        public string Web { get; set; }


        public void UpdatePart(CenterProfilePart part)
        {
            part.ThrowIfNull();

            part.CenterAddress = CenterAddress;
            part.CenterName = CenterName;
            part.CenterSettlementName = CenterSettlementName;
            if (CenterZipCode.HasValue)
            {
                part.CenterZipCode = CenterZipCode.Value;
            }

            part.Latitude = Latitude;
            part.Longitude = Longitude;
            part.CenterTypes = CenterTypes;
            part.Phone = Phone;
            part.Fax = Fax;
            part.Email = Email;
            part.Web = Web;
        }

        public void UpdateViewModel(CenterProfilePart part)
        {
            part.ThrowIfNull();

            Created = part.Created;
            CenterAddress = part.CenterAddress;
            CenterName = part.CenterName;
            CenterSettlementName = part.CenterSettlementName;
            if (part.CenterZipCode > 0)
            {
                CenterZipCode = part.CenterZipCode;
            }
            Latitude = part.Latitude;
            Longitude = part.Longitude;
            AccreditationStatus = part.AccreditationStatus;
            AccreditationStatusDateUtc = part.AccreditationStatusDateUtc;
            CenterTypes = part.CenterTypes;
            Phone = part.Phone;
            Fax = part.Fax;
            Email = part.Email;
            Web = part.Web;
        }
    }
}
