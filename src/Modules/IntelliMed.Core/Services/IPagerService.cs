using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Navigation;
using YesSql;

namespace IntelliMed.Core.Services
{
    /// <summary>
    /// Service related to pagination.
    /// </summary>
    public interface IPagerService
    {
        /// <summary>
        /// Outfactored from the built-in controllers.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="pagerParameters"></param>
        /// <param name="query"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        Task<IActionResult> PagedViewAsync(Controller controller, PagerParameters pagerParameters, IQuery<ContentItem> query, string viewName);
    }
}
