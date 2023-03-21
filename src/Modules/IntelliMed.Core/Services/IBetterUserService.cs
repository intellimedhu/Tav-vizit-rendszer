using OrchardCore.Users.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMed.Core.Services
{
    /// <summary>
    /// Use this service until OC has a similarly usable service like workcontextaccessor was or directly usable user on controllers.
    /// </summary>
    public interface IBetterUserService
    {
        Task<User> GetCurrentUserAsync();
        Task<User> ConvertUserAsync(ClaimsPrincipal principal);
        bool IsInRole(User user, string role);
        Task<bool> IsInRoleCurrentUserAsync(string role);
        Task<bool> IsInRoleAsync(ClaimsPrincipal user, string role);
    }
}
