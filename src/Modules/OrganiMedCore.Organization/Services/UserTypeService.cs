using IntelliMed.Core.Services;
using OrchardCore.Users.Models;
using OrganiMedCore.Organization.Constants;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Services
{
    public class UserTypeService : IUserTypeService
    {
        private readonly IBetterUserService _betterUserService;


        public UserTypeService(IBetterUserService betterUserService)
        {
            _betterUserService = betterUserService;
        }


        public bool IsDoctor(User user) =>
            _betterUserService.IsInRole(user, OrganizationRoleNames.Doctor);

        public async Task<bool> IsDoctorAsync(ClaimsPrincipal user) =>
            await _betterUserService.IsInRoleAsync(user, OrganizationRoleNames.Doctor);

        public async Task<bool> IsDoctorCurrentUser() =>
            await _betterUserService.IsInRoleCurrentUserAsync(OrganizationRoleNames.Doctor);

        public bool IsPatient(User user) =>
            _betterUserService.IsInRole(user, OrganizationRoleNames.Patient);

        public async Task<bool> IsPatientAsync(ClaimsPrincipal user) =>
            await _betterUserService.IsInRoleAsync(user, OrganizationRoleNames.Patient);

        public async Task<bool> IsPatientCurrentUserAsync() =>
            await _betterUserService.IsInRoleCurrentUserAsync(OrganizationRoleNames.Patient);

        public bool IsOrganizationUser(User user) =>
            _betterUserService.IsInRole(user, OrganizationRoleNames.Doctor) ||
            _betterUserService.IsInRole(user, OrganizationRoleNames.ChiefDoctor) ||
             _betterUserService.IsInRole(user, OrganizationRoleNames.Receptionist);

        public async Task<bool> IsOrganizationUserAsync(ClaimsPrincipal user) =>
            await _betterUserService.IsInRoleAsync(user, OrganizationRoleNames.Doctor) ||
            await _betterUserService.IsInRoleAsync(user, OrganizationRoleNames.ChiefDoctor) ||
            await _betterUserService.IsInRoleAsync(user, OrganizationRoleNames.Receptionist);

        public async Task<bool> IsOrganizationUserCurrentUserAsync() =>
            IsOrganizationUser(await _betterUserService.GetCurrentUserAsync());
    }
}
