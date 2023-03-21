using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class QualificationViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public string Name { get; set; }


        public void UpdateViewModel(Qualification model)
        {
            model.ThrowIfNull();

            Id = model.Id;
            Name = model.Name;
        }

        public void UpdateModel(Qualification model)
        {
            model.ThrowIfNull();

            model.Name = Name;
        }
    }
}
