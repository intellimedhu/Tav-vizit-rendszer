using IntelliMed.Core.Extensions;
using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class PersonQualificationViewModel
    {
        [JsonProperty("id")]
        public Guid? QualificationId { get; set; }

        [JsonProperty("number")]
        public string QualificationNumber { get; set; }

        [JsonProperty("year")]
        public int? QualificationYear { get; set; }

        [JsonProperty("state")]
        public QualificationState? State { get; set; }


        public void UpdateModel(PersonQualification model)
        {
            model.ThrowIfNull();

            if (QualificationId.HasValue)
            {
                model.QualificationId = QualificationId.Value;
            }
            model.QualificationNumber = QualificationNumber;
            model.QualificationYear = QualificationYear;
            model.State = State;
        }

        public void UpdateViewModel(PersonQualification model)
        {
            model.ThrowIfNull();

            QualificationId = model.QualificationId;
            QualificationNumber = model.QualificationNumber;
            QualificationYear = model.QualificationYear;
            State = model.State;
        }
    }
}
