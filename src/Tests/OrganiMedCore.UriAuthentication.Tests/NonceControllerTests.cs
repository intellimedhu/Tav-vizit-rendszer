using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.Login.Services;
using OrganiMedCore.Testing.Core.MockServices;
using OrganiMedCore.UriAuthentication.Controllers;
using OrganiMedCore.UriAuthentication.Models;
using OrganiMedCore.UriAuthentication.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.UriAuthentication.Tests
{
    public class NonceControllerTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("non-guid-value")]
        [InlineData("abcdef00000000000000000000000000")]
        public async Task Index_InvalidParameter_ShouldReturn_InvalidNonceViewResult(string value)
        {
            var controller = new NonceController(
                null,
                null,
                null,
                new NonceServiceMock(null),
                null,
                null,
                null);
            var result = await controller.Index(value);

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal("InvalidNonce", viewResult.ViewName);
        }

        [Fact]
        public async Task Index_SignInDokiNetMember_ShouldLogMemberNotFound()
        {
            var nonceAsString = "abcdef00000000000000000000000000";
            var nonce = new Nonce()
            {
                Value = Guid.Parse(nonceAsString),
                Type = NonceType.MemberRightId,
                TypeId = 888 // should not exists in service
            };
            var logger = new LoggerMock<NonceController>();
            var controller = new NonceController(
                new DokiNetServiceMock(),
                new LocalizerMock<NonceController>(),
                logger,
                new NonceServiceMock(nonce),
                null,
                null,
                null);

            var result = await controller.Index(nonceAsString);

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal("Error", viewResult.ViewName);
            Assert.Contains(logger.Logs, log => log.State.ToString().Contains("DokiNetMember not found by memberRightId"));
            Assert.Contains(viewResult.ViewData.Keys, key => key == "ErrorMessage");
        }

        [Theory]
        [InlineData(false, "~/unauthorized-local-url")]
        [InlineData(true, "~/authorized-local-url")]
        public async Task Index_SignInDokiNetMember_ShouldReturn_LocalRedirectResult(
            bool shouldSignIn,
            string redirectUrl)
        {
            var nonceAsString = "abcdef00000000000000000000000000";
            var nonce = new Nonce()
            {
                Value = Guid.Parse(nonceAsString),
                Type = NonceType.MemberRightId,
                TypeId = 100, // should exists in service
                RedirectUrl = redirectUrl
            };
            var logger = new LoggerMock<NonceController>();
            var controller = new NonceController(
                new DokiNetServiceMock(),
                new LocalizerMock<NonceController>(),
                logger,
                new NonceServiceMock(nonce),
                null,
                new SharedUserServiceMock(shouldSignIn),
                null);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "username")
                    },
                    // Let the second parameter empty, so the user won't be authenticated
                    string.Empty))
                }
            };

            var result = await controller.Index(nonceAsString);

            var localRedirectResult = result as LocalRedirectResult;

            Assert.NotNull(localRedirectResult);
            if (shouldSignIn)
            {
                Assert.Equal(redirectUrl, localRedirectResult.Url);
            }
        }
    }

    class NonceServiceMock : INonceService
    {
        private readonly Nonce _expectedResult;


        public NonceServiceMock(Nonce expectedResult)
        {
            _expectedResult = expectedResult;
        }


        public Task CleanupAsync()
            => throw new NotImplementedException();

        public Task CreateAsync(Nonce nonce)
            => throw new NotImplementedException();

        public Task CreateManyAsync(IEnumerable<Nonce> nonces)
            => throw new NotImplementedException();

        public Task<Nonce> GetByValue(Guid value)
            => Task.FromResult(_expectedResult);

        public Task<string> GetUriAsync(Guid value)
            => throw new NotImplementedException();
    }

    class SharedUserServiceMock : ISharedUserService
    {
        private readonly bool _shouldSignIn;


        public SharedUserServiceMock(bool shouldSignIn)
        {
            _shouldSignIn = shouldSignIn;
        }


        public Task<bool> SignInDokiNetMemberAsync(DokiNetMember dokiNetMember, Action<string> reportError, IUpdateModel updater, bool rememberMe)
            => Task.FromResult(_shouldSignIn);

        [ExcludeFromCodeCoverage]
        public Task<IUser> CreateOrUpdateSharedUserUsingDokiNetMemberAsync(IServiceScope managersServiceScope, DokiNetMember dokiNetMember, IList<string> tenantRoles, IUpdateModel updater)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<IdentityResult> DeleteLocalUserIfHasNoRolesAsync(IServiceScope scope, User localUser)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<DokiNetMember> GetCurrentUsersDokiNetMemberAsync()
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<DokiNetMember>> GetDokiNetMembersFromManagersScopeByLocalUserIdsAsync(IEnumerable<string> sharedUserIds)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<IUser> GetSharedUserByDokiNetMemberAsync(IServiceScope scope, DokiNetMember dokiNetMember)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<DokiNetMember> GetUsersDokiNetMemberAsync(User localUser)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<DokiNetMember> UpdateAndGetCurrentUsersDokiNetMemberDataAsync()
        {
            throw new NotImplementedException();
        }
    }
}
