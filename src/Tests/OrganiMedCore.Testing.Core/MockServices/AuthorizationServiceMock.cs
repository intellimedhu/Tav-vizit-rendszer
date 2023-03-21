using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrganiMedCore.Testing.Core.MockServices
{
    public class AuthorizationServiceMock : IAuthorizationService
    {
        private readonly AuthorizationResult _expectedResult;


        public AuthorizationServiceMock() : this(AuthorizationResult.Success())
        {
        }

        public AuthorizationServiceMock(AuthorizationResult expectedResult)
        {
            _expectedResult = expectedResult;
        }


        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements)
            => Task.FromResult(_expectedResult);

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
            => Task.FromResult(_expectedResult);
    }
}
