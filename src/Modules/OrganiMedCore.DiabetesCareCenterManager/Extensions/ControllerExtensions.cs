using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Notify;

namespace OrganiMedCore.DiabetesCareCenterManager.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult RedirectWhenUserIsNotDokiNetMember(this Controller controller, INotifier notifier, IHtmlLocalizer t)
        {
            notifier.Error(t["Sajnáljuk, de az Ön tagi adatai hiányosak a rendszerben, így a kért oldal nem tekinthető meg."]);

            return controller.RedirectToHome();
        }

        public static IActionResult RedirectToHome(this Controller controller)
            => controller.LocalRedirect("~/");
    }
}
