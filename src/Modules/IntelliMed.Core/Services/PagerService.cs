using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using YesSql;

namespace IntelliMed.Core.Services
{
    public class PagerService : IPagerService
    {
        private readonly ISiteService _siteService;

        public dynamic New { get; set; }


        public PagerService(ISiteService siteService, IShapeFactory shapeFactory)
        {
            _siteService = siteService;

            New = shapeFactory;
        }


        public async Task<IActionResult> PagedViewAsync(
            Controller controller,
            PagerParameters pagerParameters,
            IQuery<ContentItem> query,
            string viewName)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            var maxPagedCount = siteSettings.MaxPagedCount;
            if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
            {
                pager.PageSize = maxPagedCount;
            }

            var viewModel = (await New.ViewModel())
                .ContentItems(await query.Slice(pager.GetStartIndex(), pager.PageSize).ListAsync())
                .Pager((await New.Pager(pager)).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : await query.CountAsync()));

            return controller.View(viewName, viewModel);
        }
    }
}
