using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using System;

namespace OrganiMedCore.PatientApp
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();

            //services.ConfigureApplicationCookie(options =>
            //{
            //    options.Cookie.SameSite = SameSiteMode.None;
            //    options.Cookie.Name = "auth_cookie";
            //    options.Events = new CookieAuthenticationEvents()
            //    {
            //        OnRedirectToLogin = ctx =>
            //        {
            //            ctx.HttpContext.Response.StatusCode = 401;

            //            return Task.CompletedTask;
            //        }
            //    };
            //});

            //services.AddCors();

            //services.AddMvc(options =>
            //{
            //    options.Filters.Add(new ValidateAntiForgeryTokenAttribute());
            //});

            //services.AddAntiforgery(options =>
            //{
            //    options.HeaderName = "X-XSRF-TOKEN";
            //});
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            //app.UseCors();
            //app.UseCors(configurePolicy =>
            //{
            //    configurePolicy.AllowAnyHeader();
            //    configurePolicy.AllowAnyMethod();
            //    configurePolicy.AllowCredentials();
            //});

            //app.UseAuthentication();
        }
    }
}
