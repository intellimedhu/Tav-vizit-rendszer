using OrganiMedCore.Core.Services;
using OrganiMedCore.DiabetesCareCenterTenant.Constants;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenterTenant.Services
{
    public class CenterRoleNameProvider : IRoleNameProvider
    {
        public IEnumerable<string> GetRoleNames()
            => new[]
            {
                DiabetesCareCenterRoleNames.DiabetesCareCenterLeader
            };
    }
}
