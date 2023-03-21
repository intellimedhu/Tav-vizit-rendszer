using IntelliMed.Core.Extensions;
using IntelliMed.Core.ViewModels;
using OrganiMedCore.Organization.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.Organization.ViewModels
{
    public class OrganizationUnitPartViewModel : IContentPartViewModel<OrganizationUnitPart>
    {
        [Required]
        public string Name { get; set; }

        public string EesztId { get; set; }

        public string EesztName { get; set; }

        public string OrganizationUnitType { get; set; }

        public IEnumerable<string> OrganizationUnitTypes { get; set; } = new List<string>();


        public void UpdatePart(OrganizationUnitPart part)
        {
            part.ThrowIfNull();

            part.Name = Name;
            part.EesztId = EesztId;
            part.EesztName = EesztName;
            part.OrganizationUnitType = OrganizationUnitType;
        }

        public void UpdateViewModel(OrganizationUnitPart part)
        {
            part.ThrowIfNull();

            Name = part.Name;
            EesztId = part.EesztId;
            EesztName = part.EesztName;
            OrganizationUnitType = part.OrganizationUnitType;
        }
    }
}
