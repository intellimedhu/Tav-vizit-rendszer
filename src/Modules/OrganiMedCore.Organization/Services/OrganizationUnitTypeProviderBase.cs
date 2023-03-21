using OrganiMedCore.Organization.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Services
{
    public class OrganizationUnitTypeProviderBase : IOrganizationUnitTypeProvider
    {
        public Task<IEnumerable<string>> GetOrganizationUnitTypesAsync()
            => Task.FromResult(new[]
            {
                Misc.GeneralOrganizationUnitType
            }.AsEnumerable());
    }
}
