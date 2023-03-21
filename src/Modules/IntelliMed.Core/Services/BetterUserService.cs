using Microsoft.AspNetCore.Http;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliMed.Core.Services
{
    public class BetterUserService : IBetterUserService
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public BetterUserService(IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<User> ConvertUserAsync(ClaimsPrincipal user)
        {
            var authenticatedUser = await _userService.GetAuthenticatedUserAsync(user);

            return authenticatedUser == null ? null : (User)authenticatedUser;
        }

        public async Task<User> GetCurrentUserAsync()
            => await ConvertUserAsync(_httpContextAccessor.HttpContext.User);

        public bool IsInRole(User user, string role)
            => user == null ? false : user.RoleNames.Contains(role);

        public async Task<bool> IsInRoleCurrentUserAsync(string role)
        {
            var user = await GetCurrentUserAsync();

            return user == null ? false : IsInRole(user, role);
        }

        public async Task<bool> IsInRoleAsync(ClaimsPrincipal user, string role)
            => IsInRole(await ConvertUserAsync(user), role);
    }
}
