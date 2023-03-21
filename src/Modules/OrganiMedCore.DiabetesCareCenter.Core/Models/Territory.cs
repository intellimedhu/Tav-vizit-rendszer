namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class Territory
    {
        /// <summary>
        /// Actually the DocumentId.
        /// </summary>
        public int Id { get; set; }

        public string Name { get; set; }

        public string Consultant { get; set; }

        public int? TerritorialRapporteurId { get; set; }
    }
}
