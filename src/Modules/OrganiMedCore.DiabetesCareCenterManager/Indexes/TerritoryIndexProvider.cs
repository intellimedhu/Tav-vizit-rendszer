using OrganiMedCore.DiabetesCareCenter.Core.Models;
using YesSql.Indexes;

namespace OrganiMedCore.DiabetesCareCenterManager.Indexes
{
    public class TerritoryIndexProvider : IndexProvider<Territory>
    {
        public override void Describe(DescribeContext<Territory> context)
        {
            context.For<TerritoryIndex>()
                .Map(territory => new TerritoryIndex()
                {
                    Name = territory.Name,
                    TerritorialRapporteurId = territory.TerritorialRapporteurId
                });
        }
    }
}
