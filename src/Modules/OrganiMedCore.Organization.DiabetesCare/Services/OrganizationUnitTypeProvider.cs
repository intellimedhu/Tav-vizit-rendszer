using OrganiMedCore.Organization.DiabetesCare.Constants;
using OrganiMedCore.Organization.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.DiabetesCare.Services
{
    public class OrganizationUnitTypeProvider : IOrganizationUnitTypeProvider
    {
        public Task<IEnumerable<string>> GetOrganizationUnitTypesAsync()
            => Task.FromResult(new[]
            {
                OrganizationUnitTypes.DiabetesCare
            }.AsEnumerable());
    }
}
