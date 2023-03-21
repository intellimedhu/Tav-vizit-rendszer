using OrchardCore.Users.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Services
{
    /// <summary>
    /// A helper service to determine whether a user is in an organization role.
    /// </summary>
    public interface IUserTypeService
    {
        bool IsDoctor(User user);

        Task<bool> IsDoctorAsync(ClaimsPrincipal user);

        Task<bool> IsDoctorCurrentUser();

        bool IsPatient(User user);

        Task<bool> IsPatientAsync(ClaimsPrincipal user);

        Task<bool> IsPatientCurrentUserAsync();

        bool IsOrganizationUser(User user);

        Task<bool> IsOrganizationUserAsync(ClaimsPrincipal user);

        Task<bool> IsOrganizationUserCurrentUserAsync();
    }
}
