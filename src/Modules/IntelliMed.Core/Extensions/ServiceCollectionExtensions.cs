using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using YesSql.Indexes;

namespace IntelliMed.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSiteSettingDisplayDriver<T>(this IServiceCollection services) where T : class, IDisplayDriver<ISite>
            => services.AddScoped<IDisplayDriver<ISite>, T>();

        public static IServiceCollection AddDisplayDriver<TModel, T>(this IServiceCollection services) where T : class, IDisplayDriver<TModel>
            => services.AddScoped<IDisplayDriver<TModel>, T>();

        public static IServiceCollection AddContentPartDisplayDriver<T>(this IServiceCollection services) where T : class, IContentPartDisplayDriver
            => services.AddScoped<IContentPartDisplayDriver, T>();

        public static IServiceCollection AddDataMigration<T>(this IServiceCollection services) where T : class, IDataMigration
            => services.AddScoped<IDataMigration, T>();

        public static IServiceCollection AddIndexProvider<T>(this IServiceCollection services) where T : class, IIndexProvider
            => services.AddSingleton<IIndexProvider, T>();

        public static IServiceCollection AddContentPartHandler<T>(this IServiceCollection services) where T : class, IContentPartHandler
            => services.AddScoped<IContentPartHandler, T>();

        public static IServiceCollection AddPermission<T>(this IServiceCollection services) where T : class, IPermissionProvider
            => services.AddScoped<IPermissionProvider, T>();

        public static IServiceCollection AddBackgroundTask<T>(this IServiceCollection services) where T : class, IBackgroundTask
            => services.AddSingleton<IBackgroundTask, T>();

        public static IServiceCollection AddNavigation<T>(this IServiceCollection services) where T : class, INavigationProvider
            => services.AddScoped<INavigationProvider, T>();
    }
}
