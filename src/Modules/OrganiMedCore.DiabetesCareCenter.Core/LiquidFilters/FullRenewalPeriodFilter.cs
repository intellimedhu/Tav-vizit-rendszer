using Fluid;
using Fluid.Values;
using IntelliMed.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.LiquidFilters
{
    public class FullRenewalPeriodFilter : ILiquidFilter
    {
        private readonly IServiceProvider _serviceProvider;


        public FullRenewalPeriodFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var result = false;
            if (DateTime.TryParse(input.ToStringValue(), out var dateLocal))
            {
                CenterRenewalSettings settings;

                var requestedFromTenant = arguments["is_tenant"].Or(arguments.At(0)).ToBooleanValue();
                if(!requestedFromTenant)
                {
                    var renewalPeriodSettingsService = _serviceProvider.GetRequiredService<IRenewalPeriodSettingsService>();
                    settings = await renewalPeriodSettingsService.GetCenterRenewalSettingsAsync();
                }
                else
                {
                    var sharedDataAccessorService = _serviceProvider.GetRequiredService<ISharedDataAccessorService>();
                    using (var scope = await sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
                    {
                        var renewalPeriodSettingsService = scope.ServiceProvider.GetRequiredService<IRenewalPeriodSettingsService>();
                        settings = await renewalPeriodSettingsService.GetCenterRenewalSettingsAsync();
                    }
                }                

                var fullPeriod = settings.GetCurrentFullPeriod(dateLocal.ToUniversalTime());

                result = fullPeriod != null;
            }

            return FluidValue.Create(result);
        }
    }
}
