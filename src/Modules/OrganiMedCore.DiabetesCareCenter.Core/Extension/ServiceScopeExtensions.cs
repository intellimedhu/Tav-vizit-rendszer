using IntelliMed.Core.Constants;
using IntelliMed.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Extensions
{
    public static class ServiceScopeExtensions
    {
        public static async Task<IServiceScope> GetCareCenterManagerServiceScopeAsync(this ISharedDataAccessorService sharedDataAccessorService)
            => await sharedDataAccessorService.GetTenantServiceScopeAsync(WellKnownNames.DiabetesCareCenterManagerTenantName);
    }
}
