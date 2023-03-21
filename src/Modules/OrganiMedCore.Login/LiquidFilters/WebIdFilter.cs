using Fluid;
using Fluid.Values;
using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Exceptions;
using Microsoft.Extensions.Logging;
using OrchardCore.Liquid;
using OrganiMedCore.Login.Services;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.LiquidFilters
{
    public class WebIdFilter : ILiquidFilter
    {
        private readonly ILogger _logger;
        private readonly ISharedUserService _sharedUserService;


        public WebIdFilter(ILogger<WebIdFilter> logger, ISharedUserService sharedUserService)
        {
            _logger = logger;
            _sharedUserService = sharedUserService;
        }


        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();
            if (string.IsNullOrEmpty(url))
            {
                return FluidValue.Create(url);
            }

            try
            {
                var uriBuilder = new UriBuilder(url);

                var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();
                if (dokiNetMember == null || string.IsNullOrEmpty(dokiNetMember.WebId))
                {
                    return FluidValue.Create(url);
                }

                uriBuilder.AppendQueryParams("web_id", dokiNetMember.WebId);

                // In the following cases it isn't required to have a port number:
                if ((uriBuilder.Scheme == "https" && uriBuilder.Port == 443) ||
                    (uriBuilder.Scheme == "http" && uriBuilder.Port == 80))
                {
                    uriBuilder.Port = -1;
                }

                return FluidValue.Create(uriBuilder.ToString());
            }
            catch (UserHasNoMemberRightIdException)
            {
                return FluidValue.Create(url);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, $"Trying to parse uri: \"{url}\"");

                return FluidValue.Create(url);
            }
        }
    }
}
