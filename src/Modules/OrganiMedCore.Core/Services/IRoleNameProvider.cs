using System.Collections.Generic;

namespace OrganiMedCore.Core.Services
{
    public interface IRoleNameProvider
    {
        IEnumerable<string> GetRoleNames();
    }
}
