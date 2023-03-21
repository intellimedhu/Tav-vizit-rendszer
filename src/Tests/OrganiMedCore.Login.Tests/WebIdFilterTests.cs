using Fluid.Values;
using IntelliMed.DokiNetIntegration.Exceptions;
using IntelliMed.DokiNetIntegration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.Login.LiquidFilters;
using OrganiMedCore.Login.Services;
using OrganiMedCore.Testing.Core.MockServices;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.Login.Tests
{
    public class WebIdFilterTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ProcessAsync_InvalidUrl_ShouldReturn_Input(string url)
        {
            var filter = new WebIdFilter(null, null);
            var result = await filter.ProcessAsync(FluidValue.Create(url), null, null);
            Assert.Equal(string.Empty, result.ToStringValue());
        }

        [Fact]
        public async Task ProcessAsync_NoMemberRightId_ShouldReturn_Input()
        {
            const string url = "http://doki.net/info.aspx";
            var filter = new WebIdFilter(null, new SharedUserServiceMock(true, null));
            var result = await filter.ProcessAsync(FluidValue.Create(url), null, null);
            Assert.Equal(url, result.ToStringValue());
        }

        [Fact]
        public async Task ProcessAsync_NoDokiNetMember_ShouldReturn_Input()
        {
            const string url = "https://doki.net/index";
            var filter = new WebIdFilter(null, new SharedUserServiceMock(false, null));
            var result = await filter.ProcessAsync(FluidValue.Create(url), null, null);
            Assert.Equal(url, result.ToStringValue());
        }

        [Fact]
        public async Task ProcessAsync_InvalidFormat_ShouldReturn_Input()
        {
            const string url = "invalid url";
            var logger = new LoggerMock<WebIdFilter>();
            var filter = new WebIdFilter(logger, new SharedUserServiceMock(false, new DokiNetMember()));
            var result = await filter.ProcessAsync(FluidValue.Create(url), null, null);
            Assert.Equal(url, result.ToStringValue());
            Assert.NotEmpty(logger.Logs);
        }

        [Theory]
        [InlineData("MYWEBID", "http://doki.net/doc.aspx", "http://doki.net/doc.aspx?web_id=MYWEBID")]
        [InlineData("MYWEBID", "http://doki.net/doc.aspx?a=123", "http://doki.net/doc.aspx?a=123&web_id=MYWEBID")]
        [InlineData("MYWEBID", "http://doki.net:80/b.aspx", "http://doki.net/b.aspx?web_id=MYWEBID")]
        [InlineData("MYWEBID", "http://doki.net:8080/doc2.aspx", "http://doki.net:8080/doc2.aspx?web_id=MYWEBID")]
        [InlineData("MYWEBID", "https://doki.net/doc.aspx", "https://doki.net/doc.aspx?web_id=MYWEBID")]
        [InlineData("MYWEBID", "https://doki.net:443/a.aspx", "https://doki.net/a.aspx?web_id=MYWEBID")]
        [InlineData("MYWEBID", "https://doki.net:8081/doc2.aspx", "https://doki.net:8081/doc2.aspx?web_id=MYWEBID")]
        public async Task ProcessAsync_AllValid(string webId, string url, string expectedResult)
        {
            var filter = new WebIdFilter(null, new SharedUserServiceMock(false, new DokiNetMember() { WebId = webId }));
            var result = await filter.ProcessAsync(FluidValue.Create(url), null, null);
            Assert.Equal(expectedResult, result.ToStringValue());
        }


        private class SharedUserServiceMock : ISharedUserService
        {
            private readonly bool _shouldThrow;
            private readonly DokiNetMember _expectedResult;


            public SharedUserServiceMock(bool shouldThrow, DokiNetMember expectedResult)
            {
                _shouldThrow = shouldThrow;
                _expectedResult = expectedResult;
            }


            public Task<DokiNetMember> GetCurrentUsersDokiNetMemberAsync()
            {
                if (_shouldThrow)
                {
                    throw new UserHasNoMemberRightIdException();
                }

                return Task.FromResult(_expectedResult);
            }

            [ExcludeFromCodeCoverage]
            public Task<IUser> CreateOrUpdateSharedUserUsingDokiNetMemberAsync(IServiceScope managersServiceScope, DokiNetMember dokiNetMember, IList<string> tenantRoles, IUpdateModel updater)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<IdentityResult> DeleteLocalUserIfHasNoRolesAsync(IServiceScope scope, User localUser)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<DokiNetMember>> GetDokiNetMembersFromManagersScopeByLocalUserIdsAsync(IEnumerable<string> sharedUserIds)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<IUser> GetSharedUserByDokiNetMemberAsync(IServiceScope scope, DokiNetMember dokiNetMember)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<DokiNetMember> GetUsersDokiNetMemberAsync(User localUser)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<bool> SignInDokiNetMemberAsync(DokiNetMember dokiNetMember, Action<string> reportError, IUpdateModel updater, bool rememberMe)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<DokiNetMember> UpdateAndGetCurrentUsersDokiNetMemberDataAsync()
                => throw new NotImplementedException();
        }
    }
}
