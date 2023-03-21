using YesSql.Indexes;

namespace OrganiMedCore.DiabetesCareCenterManager.Indexes
{
    // Check out: CenterProfileService.GetPermittedCenterProfileQueryAsync method when modifying this index
    public class TerritoryIndex : MapIndex
    {
        public string Name { get; set; }

        public int? TerritorialRapporteurId { get; set; }
    }
}
