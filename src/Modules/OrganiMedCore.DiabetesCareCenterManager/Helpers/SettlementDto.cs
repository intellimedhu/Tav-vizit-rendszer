using IntelliMed.Core.Helpers;

namespace OrganiMedCore.DiabetesCareCenterManager.Helpers
{
    /// <summary>
    /// DTO (Data transfer object) for zip codes in Magyar Posta's excel. This DTO can be used only for the first sheet.
    /// </summary>
    internal class SettlementDto
    {
        [ExcelColumn(1)]
        public string ZipCode { get; set; }

        [ExcelColumn(2)]
        public string Name { get; set; }

        [ExcelColumn(3)]
        public string Description { get; set; }
    }
}
