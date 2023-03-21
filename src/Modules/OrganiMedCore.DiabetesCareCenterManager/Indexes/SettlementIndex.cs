using YesSql.Indexes;

namespace OrganiMedCore.DiabetesCareCenterManager.Indexes
{
    // Check out: CenterProfileService.GetPermittedCenterProfileQueryAsync method when modifying this index
    public class SettlementIndex : MapIndex
    {
        public string ZipCode { get; set; }

        public string Name { get; set; }

        public int? TerritoryId { get; set; }
    }
}
