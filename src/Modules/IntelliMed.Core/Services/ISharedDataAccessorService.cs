using IntelliMed.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell.Builders;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YesSql;
using YesSql.Indexes;

namespace IntelliMed.Core.Services
{
    /// <summary>
    /// Service which helps working with service scopes and wraps some often used services to the manager's scope.
    /// Also provides acces to the shared data stored on the default tenant.
    /// </summary>
    public interface ISharedDataAccessorService
    {
        Task<IEnumerable<ShellContext>> ListShellContextsAsync();

        /// <summary>
        /// Gets a tenant's service scope.
        /// </summary>
        /// <param name="tenantName">The name of the tenant.</param>
        /// <exception cref="TenantUnavailableException"></exception>
        /// <returns>The tenant's service scope.</returns>
        Task<IServiceScope> GetTenantServiceScopeAsync(string tenantName);

        /// <summary>
        /// Gets the default tenant's services scope.
        /// </summary>
        /// <returns>The default tenant's service scope.</returns>
        Task<IServiceScope> GetManagerServiceScopeAsync();

        /// <summary>
        /// Calls IContentItemDisplayManager.BuildEditorAsync in the context of the manager's scope.
        /// </summary>
        Task<IShape> BuildManagerEditorAsync(IServiceScope managersServiceScope, ContentItem content, IUpdateModel updater, bool isNew, string groupId = "", string htmlFieldPrefix = "");

        /// <summary>
        /// Calls IContentItemDisplayManager.UpdateEditorAsync in the context of the manager's scope.
        /// </summary>
        Task<IShape> UpdateManagerEditorAsync(IServiceScope managersServiceScope, ContentItem content, IUpdateModel updater, bool isNew, string groupId = "", string htmlFieldPrefix = "");

        /// <summary>
        /// Calls ISession.Save in the context of the manager's scope.
        /// </summary>
        void Save(IServiceScope managersServiceScope, object obj);

        /// <summary>
        /// Calls ISession.GetAsync in the context of the manager's scope.
        /// </summary>
        Task<T> GetAsync<T>(IServiceScope managersServiceScope, int id) where T : class;

        /// <summary>
        /// Calls ISession.Query in the context of the manager's scope.
        /// </summary>
        IQuery<T> Query<T>(IServiceScope managersServiceScope) where T : class;

        /// <summary>
        /// Calls ISession.Query in the context of the manager's scope.
        /// </summary>
        IQuery<T, TIndex> Query<T, TIndex>(IServiceScope managersServiceScope, bool filterType = false)
            where T : class
            where TIndex : class, IIndex;

        /// <summary>
        /// Calls ISession.Query in the context of the manager's scope.
        /// </summary>
        IQuery<T, TIndex> Query<T, TIndex>(IServiceScope managersServiceScope, Expression<Func<TIndex, bool>> predicate, bool filterType = false)
            where T : class
            where TIndex : class, IIndex;
    }
}
