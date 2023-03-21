using IntelliMed.Core.Constants;
using IntelliMed.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YesSql;
using YesSql.Indexes;

namespace IntelliMed.Core.Services
{
    public class SharedDataAccessorService : ISharedDataAccessorService
    {
        private readonly IShellHost _shellHost;
        private readonly ISiteService _siteService;
        private readonly KeepAliveService _keepAliveService;
        private readonly ShellSettings _shellSettings;


        public SharedDataAccessorService(
            IShellHost shellHost,
            ISiteService siteService,
            KeepAliveService keepAliveService,
            ShellSettings shellSettings)
        {
            _shellHost = shellHost;
            _siteService = siteService;
            _keepAliveService = keepAliveService;
            _shellSettings = shellSettings;
        }


        public Task<IEnumerable<ShellContext>> ListShellContextsAsync()
            => Task.FromResult(_shellHost.ListShellContexts());

        public async Task<IServiceScope> GetManagerServiceScopeAsync()
            => await GetTenantServiceScopeAsync(WellKnownNames.ManagerTenantName);

        public async Task<IServiceScope> GetTenantServiceScopeAsync(string tenantName)
        {
            var tenantShellContext = GetShellContext(tenantName);
            var scope = tenantShellContext.CreateScope();
            if (scope != null)
            {
                return scope;
            }

            // Creating an HTTP request to the tenant's URL.
            if (await WakeUpTenantAsync(tenantName))
            {
                tenantShellContext = GetShellContext(tenantName);
                scope = tenantShellContext.CreateScope();
            }

            return scope ?? throw new TenantUnavailableException($"The '{tenantName}' tenant is currently unavailable.");
        }

        public async Task<IShape> BuildManagerEditorAsync(IServiceScope managersServiceScope, ContentItem content, IUpdateModel updater, bool isNew, string groupId = "", string htmlFieldPrefix = "") =>
            await managersServiceScope
                .ServiceProvider
                .GetRequiredService<IContentItemDisplayManager>()
                .BuildEditorAsync(content, updater, isNew, groupId, htmlFieldPrefix);

        public async Task<IShape> UpdateManagerEditorAsync(IServiceScope managersServiceScope, ContentItem content, IUpdateModel updater, bool isNew, string groupId = "", string htmlFieldPrefix = "") =>
            await managersServiceScope
                .ServiceProvider
                .GetRequiredService<IContentItemDisplayManager>()
                .UpdateEditorAsync(content, updater, isNew, groupId, htmlFieldPrefix);

        public void Save(IServiceScope managersServiceScope, object obj) =>
                managersServiceScope.ServiceProvider.GetRequiredService<ISession>().Save(obj);

        public async Task<T> GetAsync<T>(IServiceScope managersServiceScope, int id) where T : class =>
             await managersServiceScope.ServiceProvider.GetRequiredService<ISession>().GetAsync<T>(id);

        public IQuery<T> Query<T>(IServiceScope managersServiceScope) where T : class =>
            managersServiceScope.ServiceProvider.GetRequiredService<ISession>().Query<T>();

        public IQuery<T, TIndex> Query<T, TIndex>(IServiceScope managersServiceScope, bool filterType = false)
            where T : class
            where TIndex : class, IIndex =>
            managersServiceScope.ServiceProvider.GetRequiredService<ISession>().Query<T, TIndex>(filterType);

        public IQuery<T, TIndex> Query<T, TIndex>(IServiceScope managersServiceScope, Expression<Func<TIndex, bool>> predicate, bool filterType = false)
            where T : class
            where TIndex : class, IIndex =>
            managersServiceScope.ServiceProvider.GetRequiredService<ISession>().Query<T, TIndex>(predicate, filterType);


        private ShellContext GetShellContext(string tenantName)
        {
            var shellContext = _shellHost.ListShellContexts().FirstOrDefault(x => x.Settings.Name == tenantName);
            if (shellContext == null)
            {
                throw new TenantUnavailableException($"The '{tenantName}' tenant is currently unavailable.");
            }

            return shellContext;
        }

        private async Task<bool> WakeUpTenantAsync(string requestedTenantName)
        {
            string defaultTenantsBaseUrl;
            if (_shellSettings.Name != WellKnownNames.ManagerTenantName)
            {
                using (var managersScope = await GetManagerServiceScopeAsync())
                {
                    defaultTenantsBaseUrl = (await managersScope.ServiceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync()).BaseUrl;
                }
            }
            else
            {
                defaultTenantsBaseUrl = (await _siteService.GetSiteSettingsAsync()).BaseUrl;
            }

            var tenantShellContext = GetShellContext(requestedTenantName);
            var uriBuilder = new UriBuilder(defaultTenantsBaseUrl)
            {
                Path = tenantShellContext.Settings.RequestUrlPrefix + "/" + KeepAliveConstants.KeepAliveRelativePath
            };

            return await _keepAliveService.KeepAliveAsync(uriBuilder.Uri);
        }
    }
}
