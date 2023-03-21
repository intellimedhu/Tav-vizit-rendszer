using IntelliMed.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers
{
    [Authorize]
    public class TerritoriesController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICenterUserService _centerUserService;
        private readonly UserManager<IUser> _userManager;
        private readonly ITerritoryService _territoryService;


        public IHtmlLocalizer T { get; set; }


        public TerritoriesController(
            IAuthorizationService authorizationService,
            ICenterUserService centerUserService,
            IHtmlLocalizer<TerritoriesController> htmlLocalizer,
            UserManager<IUser> userManager,
            ITerritoryService territoryService)
        {
            _authorizationService = authorizationService;
            _centerUserService = centerUserService;
            _userManager = userManager;
            _territoryService = territoryService;

            T = htmlLocalizer;
        }


        [Route("szeh-referensek-es-szaktanacsadok")]
        public async Task<IActionResult> Index()
        {
            var managePermission = await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageTerritorialRapporteurs);
            if (!managePermission &&
                !await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ViewTerritorialRapporteurs))
            {
                return Unauthorized();
            }

            var territories = await _territoryService.GetTerritoriesAsync();
            var territorialRapporteurs = (await _userManager.GetUsersInRoleAsync(CenterPosts.TerritorialRapporteur))
                .Select(user => user as User);

            ViewData["Users"] = await _centerUserService.GetUsersByLocalUsersAsync(territorialRapporteurs);
            ViewData["ManagePermission"] = managePermission;

            return View(territories);
        }

        public async Task<IActionResult> Change(int territoryId, int userId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageTerritorialRapporteurs))
            {
                return Unauthorized();
            }

            if (!(await _userManager.FindByIdAsync(userId.ToString()) is User newUser))
            {
                return NotFound("User not found");
            }

            if (!newUser.RoleNames.Contains(CenterPosts.TerritorialRapporteur))
            {
                return BadRequest("The selected user is not territorial rapporteur");
            }

            try
            {
                await _territoryService.ChangeTerritorialRapporteurAsync(territoryId, newUser.Id);

                return Ok();
            }
            catch (Exception ex) when (ex is NotFoundException || ex is TerritoryException)
            {
                return BadRequest(T[ex.Message].Value);
            }
        }

        public async Task<IActionResult> Consultant(int territoryId, string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageTerritorialRapporteurs))
            {
                return Unauthorized();
            }

            try
            {
                await _territoryService.ChangeConsultantAsync(territoryId, name);

                return Ok();
            }
            catch (Exception ex) when (ex is NotFoundException || ex is TerritoryException)
            {
                return BadRequest(T[ex.Message].Value);
            }
        }
    }
}
