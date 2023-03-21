using OrganiMedCore.DiabetesCareCenter.Core.Models;
using YesSql.Indexes;

namespace OrganiMedCore.DiabetesCareCenterManager.Indexes
{
    public class SettlementIndexProvider : IndexProvider<Settlement>
    {
        public override void Describe(DescribeContext<Settlement> context)
            => context.For<SettlementIndex>()
                .Map(settlement => new SettlementIndex()
                {
                    Name = settlement.Name,
                    ZipCode = settlement.ZipCode.ToString(),
                    TerritoryId = settlement.TerritoryId
                });
    }
}
