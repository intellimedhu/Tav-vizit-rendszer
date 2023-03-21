using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrganiMedCore.Navigation.Services;

namespace OrganiMedCore.Navigation
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddScoped<IMenuManager, MenuManager>();
        }
    }
}
