using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Services;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Controllers
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    [Admin]
    public class CenterProfileEditorInfoSettingsController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICenterProfileInfoService _centerProfileEditorInfoService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly ISession _session;


        public IHtmlLocalizer T { get; set; }


        public CenterProfileEditorInfoSettingsController(
            IAuthorizationService authorizationService,
            ICenterProfileInfoService centerProfileEditorInfoService,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentManager contentManager,
            IHtmlLocalizer<CenterProfileEditorInfoSettingsController> htmlLocalizer,
            INotifier notifier,
            ISession session)
        {
            _authorizationService = authorizationService;
            _centerProfileEditorInfoService = centerProfileEditorInfoService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
            _notifier = notifier;
            _session = session;

            T = htmlLocalizer;
        }


        public async Task<IActionResult> EditSettings(string contentType)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            if (!_centerProfileEditorInfoService.AllowedContentType(contentType))
            {
                _notifier.Error(T["The specified content type is now allowed."]);

                return Redirect("~/admin");
            }

            var (contentItem, isNew) = await _centerProfileEditorInfoService.GetOrCreateNewContentItemAsync(contentType);
            var shape = await _contentItemDisplayManager.BuildEditorAsync(contentItem, this, isNew);

            ViewData["ContentType"] = contentType;

            return View(shape);
        }

        [HttpPost, ActionName(nameof(EditSettings))]
        public async Task<IActionResult> EditSettingsPost([FromQuery]string contentType)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            if (!_centerProfileEditorInfoService.AllowedContentType(contentType))
            {
                _notifier.Error(T["The specified content type is now allowed."]);

                return Redirect("~/admin");
            }

            var (contentItem, isNew) = await _centerProfileEditorInfoService.GetOrCreateNewContentItemAsync(contentType);
            var shape = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this, isNew);
            if (!ModelState.IsValid)
            {
                _session.Cancel();

                ViewData["ContentType"] = contentType;

                return View(nameof(EditSettings), shape);
            }

            await _centerProfileEditorInfoService.SaveContentItemAsync(contentItem, isNew);

            _notifier.Success(T["Az információs blokkok mentése sikeresen megtörtént."]);

            return RedirectToAction(nameof(EditSettings),new { contentType });
        }
    }
}
