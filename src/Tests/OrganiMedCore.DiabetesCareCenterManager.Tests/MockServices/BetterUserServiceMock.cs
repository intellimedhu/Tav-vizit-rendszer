using IntelliMed.Core.Services;
using OrchardCore.Users.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests.MockServices
{
    public class BetterUserServiceMock : IBetterUserService
    {
        private readonly int _territorialRapporteurId;


        public BetterUserServiceMock(int territorialRapporteurId)
        {
            _territorialRapporteurId = territorialRapporteurId;
        }


        [ExcludeFromCodeCoverage]
        public Task<User> ConvertUserAsync(ClaimsPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetCurrentUserAsync()
        {
            var user = new User() { Id = _territorialRapporteurId };

            return Task.FromResult(user);
        }

        [ExcludeFromCodeCoverage]
        public bool IsInRole(User user, string role)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<bool> IsInRoleAsync(ClaimsPrincipal user, string role)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<bool> IsInRoleCurrentUserAsync(string role)
        {
            throw new NotImplementedException();
        }
    }
}
