using IntelliMed.Core.Helpers;

namespace OrganiMedCore.DiabetesCareCenterManager.Helpers
{
    /// <summary>
    /// DTO (Data transfer object) for zip codes in Magyar Posta's excel. This DTO can be used to importing the cities beginning from 3rd sheet.
    /// </summary>
    internal class CityDto
    {
        [ExcelColumn(1)]
        public string ZipCode { get; set; }

        [ExcelColumn(2)]
        public string Street { get; set; }

        [ExcelColumn(3)]
        public string PublicLand { get; set; }

        [ExcelColumn(8)]
        public string District { get; set; }
    }
}
