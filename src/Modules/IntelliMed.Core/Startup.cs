using IntelliMed.Core.Http;
using IntelliMed.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using Polly;
using System;

namespace IntelliMed.Core
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddScoped<IBetterUserService, BetterUserService>();
            services.AddScoped<IPagerService, PagerService>();
            services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();
            services.AddTransient<ISharedDataAccessorService, SharedDataAccessorService>();
            services.AddSingleton<KeepAliveService>();

            // Http
            services.AddHttpClient<IHttpRequestHandler, HttpRequestHandler>()
                .AddTransientHttpErrorPolicy(policy =>
                    policy.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)));

            // Resources
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();
        }
    }
}