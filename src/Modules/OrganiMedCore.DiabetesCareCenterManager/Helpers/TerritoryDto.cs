using IntelliMed.Core.Helpers;

namespace OrganiMedCore.DiabetesCareCenterManager.Helpers
{
    internal class TerritoryDto
    {
        [ExcelColumn(1)]
        public string County { get; set; }

        [ExcelColumn(3)]
        public string Consultant { get; set; }
    }
}
