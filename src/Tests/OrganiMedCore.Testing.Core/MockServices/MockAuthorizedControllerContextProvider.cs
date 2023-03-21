using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace OrganiMedCore.Testing.Core.MockServices
{
    public static class MockAuthorizedControllerContextProvider
    {
        public static ControllerContext GetAuthorizedControllerContext(params string[] roles)
            => new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        roles
                            .Select(role => new Claim(ClaimTypes.Role, role))
                            .ToArray()))
                }
            };
    }
}
