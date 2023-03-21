using IntelliMed.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenterManager.Controllers;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.DiabetesCareCenterManager.ViewModels;
using OrganiMedCore.Testing.Core.MockServices;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class TerritoriesControllerTests
    {
        [Fact]
        public async Task Index_ShouldReturn_UnauthorizedResult()
        {
            using (var controller = new TerritoriesController(new AuthorizationServiceMock(), null, null, null, null))
            {
                var result = await controller.Index();
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Theory]
        [InlineData(nameof(ManagerPermissions.ViewTerritorialRapporteurs))]
        [InlineData(nameof(ManagerPermissions.ManageTerritorialRapporteurs))]
        public async Task Index_ShouldReturn_ViewResult(string permission)
        {
            using (var controller = new TerritoriesController(
                new AuthorizationServiceMock(),
                new CenterUserServiceMock(),
                null,
                new UserManagerMock(null),
                new TerritoryServiceMock())
            {
                ControllerContext = MockAuthorizedControllerContextProvider.GetAuthorizedControllerContext(permission)
            })
            {
                var result = await controller.Index();
                Assert.IsType<ViewResult>(result);
            }
        }

        [Fact]
        public async Task Change_ShouldReturn_UnauthorizedResult()
        {
            using (var controller = new TerritoriesController(new AuthorizationServiceMock(), null, null, null, null))
            {
                var result = await controller.Change(0, 1);
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async Task Change_ShouldReturn_NotFound()
        {
            using (var controller = new TerritoriesController(
                new AuthorizationServiceMock(),
                new CenterUserServiceMock(),
                null,
                new UserManagerMock(null),
                new TerritoryServiceMock())
            {
                ControllerContext = MockAuthorizedControllerContextProvider.GetAuthorizedControllerContext(nameof(ManagerPermissions.ManageTerritorialRapporteurs))
            })
            {
                var result = await controller.Change(1, 2);
                Assert.IsType<NotFoundObjectResult>(result);
            }
        }

        [Fact]
        public async Task Change_ShouldReturn_BadRequest()
        {
            using (var controller = new TerritoriesController(
               new AuthorizationServiceMock(),
               new CenterUserServiceMock(),
               null,
               new UserManagerMock(new User() { Id = 876, RoleNames = new List<string>() { CenterPosts.MDTManagement } }),
               new TerritoryServiceMock())
            {
                ControllerContext = MockAuthorizedControllerContextProvider.GetAuthorizedControllerContext(nameof(ManagerPermissions.ManageTerritorialRapporteurs))
            })
            {
                var result = await controller.Change(1, 876);
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async Task Change_NotFoundException_ShouldReturn_BadRequest()
        {
            using (var controller = new TerritoriesController(
               new AuthorizationServiceMock(),
               new CenterUserServiceMock(),
               new LocalizerMock<TerritoriesController>(),
               new UserManagerMock(new User() { Id = 876, RoleNames = new List<string>() { CenterPosts.TerritorialRapporteur } }),
               new TerritoryServiceMock(new NotFoundException("NotFound")))
            {
                ControllerContext = MockAuthorizedControllerContextProvider.GetAuthorizedControllerContext(nameof(ManagerPermissions.ManageTerritorialRapporteurs))
            })
            {
                var result = await controller.Change(1, 876);
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async Task Change_TerritoryException_ShouldReturn_BadRequest()
        {
            using (var controller = new TerritoriesController(
               new AuthorizationServiceMock(),
               new CenterUserServiceMock(),
               new LocalizerMock<TerritoriesController>(),
               new UserManagerMock(new User() { Id = 876, RoleNames = new List<string>() { CenterPosts.TerritorialRapporteur } }),
               new TerritoryServiceMock(new TerritoryException("TEx")))
            {
                ControllerContext = MockAuthorizedControllerContextProvider.GetAuthorizedControllerContext(nameof(ManagerPermissions.ManageTerritorialRapporteurs))
            })
            {
                var result = await controller.Change(1, 876);
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async Task Change_ShouldReturn_OkResult()
        {
            using (var controller = new TerritoriesController(
               new AuthorizationServiceMock(),
               new CenterUserServiceMock(),
               new LocalizerMock<TerritoriesController>(),
               new UserManagerMock(new User() { Id = 876, RoleNames = new List<string>() { CenterPosts.TerritorialRapporteur } }),
               new TerritoryServiceMock())
            {
                ControllerContext = MockAuthorizedControllerContextProvider.GetAuthorizedControllerContext(nameof(ManagerPermissions.ManageTerritorialRapporteurs))
            })
            {
                var result = await controller.Change(1, 876);
                Assert.IsType<OkResult>(result);
            }
        }

        [Fact]
        public async Task Consultant_ShouldReturn_UnauthorizedResult()
        {
            using (var controller = new TerritoriesController(new AuthorizationServiceMock(), null, null, null, null))
            {
                var result = await controller.Consultant(0, "");
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async Task Consultant_NotFoundException_ShouldReturn_BadRequest()
        {
            using (var controller = new TerritoriesController(
               new AuthorizationServiceMock(),
               null,
               new LocalizerMock<TerritoriesController>(),
               null,
               new TerritoryServiceMock(new NotFoundException("NotFound")))
            {
                ControllerContext = MockAuthorizedControllerContextProvider.GetAuthorizedControllerContext(nameof(ManagerPermissions.ManageTerritorialRapporteurs))
            })
            {
                var result = await controller.Consultant(1, "");
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async Task Consultant_ShouldReturn_OkResult()
        {
            using (var controller = new TerritoriesController(
               new AuthorizationServiceMock(),
               null,
               new LocalizerMock<TerritoriesController>(),
               null,
               new TerritoryServiceMock())
            {
                ControllerContext = MockAuthorizedControllerContextProvider.GetAuthorizedControllerContext(nameof(ManagerPermissions.ManageTerritorialRapporteurs))
            })
            {
                var result = await controller.Consultant(1, "");
                Assert.IsType<OkResult>(result);
            }
        }

        [Fact]
        public async Task Consultant_TerritoryException_ShouldReturn_BadRequest()
        {
            using (var controller = new TerritoriesController(
               new AuthorizationServiceMock(),
               null,
               new LocalizerMock<TerritoriesController>(),
               null,
               new TerritoryServiceMock(new TerritoryException("TEx")))
            {
                ControllerContext = MockAuthorizedControllerContextProvider.GetAuthorizedControllerContext(nameof(ManagerPermissions.ManageTerritorialRapporteurs))
            })
            {
                var result = await controller.Consultant(1, "");
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }


        private class TerritoryServiceMock : ITerritoryService
        {
            private Exception _expectedException;


            public TerritoryServiceMock(Exception expectedException = null)
            {
                _expectedException = expectedException;
            }


            public Task<IEnumerable<Territory>> GetTerritoriesAsync()
            {
                return Task.FromResult(
                    new[]
                    {
                    new Territory() { Id = 1, TerritorialRapporteurId = 51, Name = "MrA", Consultant = "MrsA" },
                    new Territory() { Id = 2, TerritorialRapporteurId = 52, Name = "MrB", Consultant = "MrsB" },
                    new Territory() { Id = 3, TerritorialRapporteurId = 53, Name = "MrC", Consultant = "MrsC" },
                    }.AsEnumerable());
            }

            public Task ChangeTerritorialRapporteurAsync(int territoryId, int newUserId) => ThrowEx();

            public Task ChangeConsultantAsync(int territoryId, string name) => ThrowEx();

            [ExcludeFromCodeCoverage]
            public Task<bool> CacheEnabledAsync()
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public void ClearTerritoryCache()
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<User> GetRapporteurToSettlementAsync(int zipCode, string settlementName)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<IUser>> GetReviewersAsync(int zipCode, string settlementName)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<Territory> GetTerritoryAsync(int id)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<IDictionary<Territory, IEnumerable<int>>> GetUsedZipCodesByTerritoriesAsync()
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task RemoveTerritoriesFromUserAsync(int userId)
                => throw new NotImplementedException();


            private Task ThrowEx()
            {
                if (_expectedException != null)
                {
                    throw _expectedException;
                }

                return Task.CompletedTask;
            }
        }

        private class UserManagerMock : UserManager<IUser>
        {
            private readonly IList<IUser> _result;
            private readonly IUser _foundUser;


            public UserManagerMock(IUser foundUser) : base(
                new Mock<IUserStore<IUser>>().Object, null, null, null, null, null, null, null, null)
            {
                _foundUser = foundUser;

                _result = new List<IUser>()
                {
                    new User() { RoleNames = new List<string>() { CenterPosts.TerritorialRapporteur } }
                };
            }


            public override Task<IList<IUser>> GetUsersInRoleAsync(string roleName)
                => Task.FromResult(_result);

            public override Task<IUser> FindByIdAsync(string userId)
                => Task.FromResult(_foundUser);
        }

        private class CenterUserServiceMock : ICenterUserService
        {
            public Task<IEnumerable<LocalUserWithDokiNetMemberViewModel>> GetUsersByLocalUsersAsync(IEnumerable<User> localUsers)
                => Task.FromResult(Enumerable.Empty<LocalUserWithDokiNetMemberViewModel>());

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<LocalUserWithDokiNetMemberViewModel>> GetUsersWithCenterRolesAsync()
                => throw new NotImplementedException();
        }
    }
}
