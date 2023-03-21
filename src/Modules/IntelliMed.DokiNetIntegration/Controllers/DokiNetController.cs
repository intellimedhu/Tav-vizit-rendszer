using IntelliMed.DokiNetIntegration.Constants;
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
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration.Controllers
{
    [Admin]
    public class DokiNetController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<User> _userDisplayManager;
        private readonly INotifier _notifier;
        private readonly UserManager<IUser> _userManager;


        public IHtmlLocalizer T { get; set; }


        public DokiNetController(
            IAuthorizationService authorizationService,
            IDisplayManager<User> userDisplayManager,
            IHtmlLocalizer<DokiNetController> htmlLocalizer,
            INotifier notifier,            
            UserManager<IUser> userManager)
        {
            _authorizationService = authorizationService;
            _notifier = notifier;
            _userDisplayManager = userDisplayManager;
            _userManager = userManager;

            T = htmlLocalizer;
        }


        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Users.Permissions.ManageUsers))
            {
                return Unauthorized();
            }

            if (!(await _userManager.FindByIdAsync(id) is User user))
            {
                return NotFound();
            }

            var shape = await _userDisplayManager.BuildEditorAsync(user, this, false, GroupIds.DokiNetMemberEditor);

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

            if (!(await _userManager.FindByIdAsync(id) is User user))
            {
                return NotFound();
            }

            var shape = await _userDisplayManager.UpdateEditorAsync(user, this, false, GroupIds.DokiNetMemberEditor);

            await _userManager.UpdateAsync(user);

            _notifier.Success(T["A doki.NET-es adatok mentése sikeres volt."]);

            return RedirectToAction("Index", "Admin", new { Area = "OrchardCore.Users" });
        }
    }
}
