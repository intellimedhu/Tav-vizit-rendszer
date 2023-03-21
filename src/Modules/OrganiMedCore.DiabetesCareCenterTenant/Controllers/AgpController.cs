//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Threading.Tasks;

//namespace OrganiMedCore.DiabetesCareCenterTenant.Controllers
//{
//    [Authorize]
//    public class AgpController : Controller
//    {
//        private readonly IAuthorizationService _authorizationService;


//        public AgpController(IAuthorizationService authorizationService)
//        {
//            _authorizationService = authorizationService;
//        }


//        [Route("paciensek/tav-vizit/{eVisitPatientProfileId?}")]
//        public async Task<IActionResult> Index(string eVisitPatientProfileId)
//        {
//            if (!await _authorizationService.AuthorizeAsync(User, Organization.Permissions.ManagePatinets))
//            {
//                return Unauthorized();
//            }

//            ViewData["EVisitPatientProfileId"] = eVisitPatientProfileId;

//            return View();
//        }

//        [Route("paciensek/tav-vizit-elemzes/{eVisitPatientProfileId?}")]
//        public async Task<IActionResult> Patient(string eVisitPatientProfileId)
//        {
//            if (!await _authorizationService.AuthorizeAsync(User, Organization.Permissions.ManagePatinets))
//            {
//                return Unauthorized();
//            }

//            ViewData["EVisitPatientProfileId"] = eVisitPatientProfileId;

//            return View();
//        }
//    }
//}
