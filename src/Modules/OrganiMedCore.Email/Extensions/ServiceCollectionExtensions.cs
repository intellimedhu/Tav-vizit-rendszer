using Microsoft.Extensions.DependencyInjection;
using OrganiMedCore.Email.Services;

namespace OrganiMedCore.Email.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTemplateProvider<T>(this IServiceCollection services) where T : class, IEmailTemplateProvider
            => services.AddScoped<IEmailTemplateProvider, T>();

        public static IServiceCollection AddEmailDataProcessor<T>(this IServiceCollection services) where T : class, IEmailDataProcessor
            => services.AddScoped<IEmailDataProcessor, T>();
    }
}
