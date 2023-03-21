using Microsoft.AspNetCore.Mvc;

namespace OrganiMedCore.Organization.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index() => View();
    }
}
