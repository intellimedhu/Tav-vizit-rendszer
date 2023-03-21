using Microsoft.AspNetCore.Mvc;

namespace OrganiMedCore.PatientApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult App() => View();
    }
}
