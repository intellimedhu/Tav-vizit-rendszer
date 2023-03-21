using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Services
{
    public interface IOrganizationUnitTypeProvider
    {
        Task<IEnumerable<string>> GetOrganizationUnitTypesAsync();
    }
}
