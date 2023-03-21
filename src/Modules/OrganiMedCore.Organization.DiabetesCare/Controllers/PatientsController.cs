using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.DiabetesCare.Controllers
{
    [Authorize]
    public class PatientsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;


        public PatientsController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }


        [Route("paciensek/agp/{eVisitPatientProfileId}")]
        public async Task<IActionResult> Index(string eVisitPatientProfileId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AgpAvailable))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(eVisitPatientProfileId))
            {
                return BadRequest();
            }

            ViewData["EVisitPatientProfileId"] = eVisitPatientProfileId;

            return View();
        }

        [Route("paciensek/agp-diagram/{eVisitPatientProfileId}")]
        public async Task<IActionResult> Agp(string eVisitPatientProfileId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AgpAvailable))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(eVisitPatientProfileId))
            {
                return BadRequest();
            }

            ViewData["EVisitPatientProfileId"] = eVisitPatientProfileId;

            return View();
        }
    }
}
