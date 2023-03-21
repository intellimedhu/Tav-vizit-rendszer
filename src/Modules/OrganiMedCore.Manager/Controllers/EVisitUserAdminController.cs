using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.Manager.Constants;
using System.Threading.Tasks;

namespace OrganiMedCore.Manager.Controllers
{
    [Admin]
    public class EVisitUserAdminController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<IUser> _userManager;
        private readonly IDisplayManager<User> _userDisplayManager;
        private readonly INotifier _notifier;


        public IHtmlLocalizer T { get; set; }


        public EVisitUserAdminController(
            IAuthorizationService authorizationService,
            UserManager<IUser> userManager,
            IDisplayManager<User> userDisplayManager,
            INotifier notifier,
            IHtmlLocalizer<EVisitUserAdminController> htmlLocalizer)
        {
            _authorizationService = authorizationService;
            _userManager = userManager;
            _userDisplayManager = userDisplayManager;
            _notifier = notifier;

            T = htmlLocalizer;
        }


        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Users.Permissions.ManageUsers))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (!(user is User))
            {
                return NotFound();
            }

            var shape = await _userDisplayManager.BuildEditorAsync((User)user, this, false, GroupIds.EVisitUserEditor);

            return View(shape);
        }

        [HttpPost]
        [ActionName(nameof(Edit))]
        public async Task<IActionResult> EditPost(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Users.Permissions.ManageUsers))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var shape = await _userDisplayManager.UpdateEditorAsync((User)user, this, false, GroupIds.EVisitUserEditor);

            // https://github.com/OrchardCMS/OrchardCore/issues/1834 because of this a workaround is needed.
            // Basically we skip the validation check, this must be solved.
            //if (!ModelState.IsValid)
            //{
            //    return View(shape);
            //}
            // end of the workaround.

            await _userManager.UpdateAsync(user);

            _notifier.Success(T["OrganiMed felhasználó mentése sikeres."]);

            return RedirectToAction("Index", "Admin", new { Area = "OrchardCore.Users" });
        }

    }
}
