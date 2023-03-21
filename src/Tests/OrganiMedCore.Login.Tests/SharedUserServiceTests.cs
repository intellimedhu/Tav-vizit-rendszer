using IntelliMed.DokiNetIntegration.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrganiMedCore.Login.Exceptions;
using OrganiMedCore.Login.Services;
using OrganiMedCore.Testing.Core.MockServices;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.Login.Tests
{
    public class SharedUserServiceTests
    {
        [Fact]
        public async Task CreateOrUpdate_ScopeNull_ShouldThrow_ArgumentNullException()
        {
            var service = new SharedUserService(null, null, null, null, null, null, null, null, null, null, null, null, null);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.CreateOrUpdateSharedUserUsingDokiNetMemberAsync(null, null, null, null));
        }

        [Fact]
        public async Task CreateOrUpdate_DokiNetMemberNull_ShouldThrow_ArgumentNullException()
        {
            var service = new SharedUserService(null, null, null, null, null, null, null, null, null, null, null, null, null);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.CreateOrUpdateSharedUserUsingDokiNetMemberAsync(new ServiceScopeMock(), null, null, null));
        }

        [Fact]
        public async Task CreateOrUpdate_UpdaterNull_ShouldThrow_ArgumentNullException()
        {
            var service = new SharedUserService(null, null, null, null, null, null, null, null, null, null, null, null, null);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.CreateOrUpdateSharedUserUsingDokiNetMemberAsync(new ServiceScopeMock(), new DokiNetMember(), null, null));
        }

        [Fact]
        public async Task CreateOrUpdate_NoEmail_ShouldThrow_DokiNetMemberRegistrationException()
        {
            var service = new SharedUserService(null, null, null, null, null, null, null, null, null, new LocalizerMock<SharedUserService>(), null, null, null);
            await Assert.ThrowsAsync<DokiNetMemberRegistrationException>(() =>
                service.CreateOrUpdateSharedUserUsingDokiNetMemberAsync(new ServiceScopeMock(), new DokiNetMember(), null, new UpdaterMock()));
        }

        [Fact]
        public async Task CreateOrUpdate_InvalidStampNumber_ShouldThrow_DokiNetMemberRegistrationException()
        {
            var service = new SharedUserService(null, null, null, null, null, null, null, null, null, new LocalizerMock<SharedUserService>(), null, null, null);
            await Assert.ThrowsAsync<DokiNetMemberRegistrationException>(() =>
                service.CreateOrUpdateSharedUserUsingDokiNetMemberAsync(
                    new ServiceScopeMock(),
                    new DokiNetMember()
                    {
                        Emails = new List<string>() { "a@b.c" },
                        StampNumber = "55-5-5-5"
                    },
                    null,
                    new UpdaterMock()));
        }


        private async Task<ISiteService> SetupSiteAsync<T>(Action<T> alteration) where T : new()
        {
            var siteService = new SiteServiceMock();
            var site = await siteService.GetSiteSettingsAsync();
            site.Alter(nameof(T), alteration);
            await siteService.UpdateSiteSettingsAsync(site);

            return siteService;
        }


        private class ServiceScopeMock : IServiceScope
        {
            public IServiceProvider ServiceProvider => throw new NotImplementedException();

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        private class UpdaterMock : IUpdateModel
        {
            public ModelStateDictionary ModelState => throw new NotImplementedException();

            public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
            {
                throw new NotImplementedException();
            }

            public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
            {
                throw new NotImplementedException();
            }

            public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
            {
                throw new NotImplementedException();
            }

            public bool TryValidateModel(object model)
            {
                throw new NotImplementedException();
            }

            public bool TryValidateModel(object model, string prefix)
            {
                throw new NotImplementedException();
            }
        }
    }
}
