namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class Settlement
    {
        /// <summary>
        /// Actually the DocumentId.
        /// </summary>
        public int Id { get; set; }

        public int ZipCode { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int? TerritoryId { get; set; }
    }
}
