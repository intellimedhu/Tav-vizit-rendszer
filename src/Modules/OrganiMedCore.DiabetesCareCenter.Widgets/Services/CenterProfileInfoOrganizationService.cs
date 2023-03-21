using IntelliMed.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Services
{
    [Feature("OrganiMedCore.Organization.DiabetesCareCenter.Widgets")]
    public class CenterProfileInfoOrganizationService : ICenterProfileInfoService
    {
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public CenterProfileInfoOrganizationService(ISharedDataAccessorService sharedDataAccessorService)
        {
            _sharedDataAccessorService = sharedDataAccessorService;
        }

        public bool AllowedContentType(string contentType)
            => throw new NotImplementedException();

        public async Task<(ContentItem contentItem, bool isNew)> GetOrCreateNewContentItemAsync(string contentType)
        {
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                return await scope.ServiceProvider.GetRequiredService<ICenterProfileInfoService>()
                    .GetOrCreateNewContentItemAsync(contentType);
            }
        }

        public Task SaveContentItemAsync(ContentItem contentItem, bool isNew) => Task.CompletedTask;
    }
}
