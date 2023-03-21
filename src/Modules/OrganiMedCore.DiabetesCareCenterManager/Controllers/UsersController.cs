using IntelliMed.Core.Services;
using IntelliMed.DokiNetIntegration.Exceptions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using IntelliMed.DokiNetIntegration.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenterManager.Exceptions;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.DiabetesCareCenterManager.ViewModels;
using OrganiMedCore.Login.Exceptions;
using OrganiMedCore.Login.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers
{
    [Authorize]
    public class UsersController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICenterUserService _centerUserService;
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger _logger;
        private readonly INotifier _notifier;
        private readonly ISession _session;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISharedUserService _sharedUserService;
        private readonly ITerritoryService _territoryService;
        private readonly UserManager<IUser> _userManager;


        public IHtmlLocalizer T { get; set; }


        public UsersController(
            IAuthorizationService authorizationService,
            ICenterUserService centerUserService,
            IDokiNetService dokiNetService,
            IHtmlLocalizer<UsersController> htmlLocalizer,
            ILogger<UsersController> logger,
            INotifier notifier,
            ISession session,
            ISharedDataAccessorService sharedDataAccessorService,
            ISharedUserService sharedUserService,
            ITerritoryService territoryService,
            UserManager<IUser> userManager)
        {
            _authorizationService = authorizationService;
            _centerUserService = centerUserService;
            _dokiNetService = dokiNetService;
            _logger = logger;
            _notifier = notifier;
            _session = session;
            _sharedDataAccessorService = sharedDataAccessorService;
            _sharedUserService = sharedUserService;
            _userManager = userManager;
            _territoryService = territoryService;

            T = htmlLocalizer;
        }


        [Route("szeh-felhasznalok")]
        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageCenterManagerUsers))
            {
                return Unauthorized();
            }

            await AddCenterAuthorizationViewDataAsync();

            return View(await _centerUserService.GetUsersWithCenterRolesAsync());
        }

        [Route("szeh-felhasznalok/letrehozas")]
        public async Task<IActionResult> Create(string roleName = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageCenterManagerUsers))
            {
                return Unauthorized();
            }

            var frontEndViewModel = new EditCenterUserFrontEndViewModel();
            UpdateRolesInViewModel(frontEndViewModel, newUsersRole: roleName);

            await AddCenterAuthorizationViewDataAsync();

            return View(frontEndViewModel);
        }

        [ActionName(nameof(Create))]
        [HttpPost("szeh-felhasznalok/letrehozas", Name = "CreateDccmUserPost")]
        public async Task<IActionResult> CreatePost(EditCenterUserFrontEndViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageCenterManagerUsers))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid || !await AnyRoleSelectedAsync(viewModel))
            {
                await AddCenterAuthorizationViewDataAsync();

                return View(viewModel);
            }

            try
            {
                var dokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);
                if (dokiNetMember == null)
                {
                    ModelState.AddModelError(string.Empty, T["A választott tag nem található."].Value);
                    await AddCenterAuthorizationViewDataAsync();

                    return View(viewModel);
                }

                using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
                {
                    var sharedUser = await _sharedUserService.GetSharedUserByDokiNetMemberAsync(scope, dokiNetMember);
                    if (sharedUser != null)
                    {
                        var localUser = await _userManager.FindByNameAsync((sharedUser as User).Id.ToString());
                        if (localUser != null &&
                            new[]
                            {
                                CenterPosts.MDTManagement,
                                CenterPosts.MDTSecretary,
                                CenterPosts.OMKB,
                                CenterPosts.TerritorialRapporteur
                            }.Any(role => (localUser as User).RoleNames.Contains(role)))
                        {
                            throw new UserAlreadyHasCenterRoleExeption();
                        }
                    }

                    await _sharedUserService.CreateOrUpdateSharedUserUsingDokiNetMemberAsync(
                        scope,
                        dokiNetMember,
                        await GetLocalUserCenterRolesAsync(viewModel),
                        this);

                    _notifier.Success(T["A felhasználó sikeresen létrehozva"]);

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DokiNetMemberRegistrationException ex)
            {
                _session.Cancel();
                ModelState.AddModelError(string.Empty, ex.Message);
                await AddCenterAuthorizationViewDataAsync();

                _logger.LogError(ex, ex.Message, viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);

                return View(viewModel);
            }
            catch (MemberNotFoundException ex)
            {
                _logger.LogError(ex, ex.Message, viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);

                // TODO
                return BadRequest();
            }
            catch (UserAlreadyHasCenterRoleExeption ex)
            {
                _session.Cancel();
                _notifier.Error(T["A felhasználót már rögzítésre került."]);

                _logger.LogError(ex, ex.Message, viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                _session.Cancel();
                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById", viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _session.Cancel();
                _notifier.Error(T["A felhasználót nem sikerült létrehozni"]);

                await AddCenterAuthorizationViewDataAsync();

                _logger.LogError(ex, ex.Message);

                return View(viewModel);
            }
        }

        [Route("szeh-felhasznalok/szerkesztes/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageCenterManagerUsers))
            {
                return Unauthorized();
            }

            if (!(await _userManager.FindByIdAsync(id) is User localUser))
            {
                return NotFound();
            }

            if (!await AuthorizedToEditCenterUser(localUser))
            {
                return Unauthorized();
            }

            User sharedUser;
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                sharedUser = await userManager.FindByIdAsync(localUser.UserName) as User;
            }

            var member = sharedUser.As<DokiNetMember>();
            var viewModel = new EditCenterUserFrontEndViewModel()
            {
                EditDokiNetMemberViewModel = new EditDokiNetMemberViewModel()
            };
            viewModel.EditDokiNetMemberViewModel.UpdateViewModel(member);
            UpdateRolesInViewModel(viewModel, localUser);

            await AddCenterAuthorizationViewDataAsync();

            return View(viewModel);
        }

        [ActionName(nameof(Edit))]
        [HttpPost("szeh-felhasznalok/szerkesztes/{id}", Name = "EditDccmUserPost")]
        public async Task<IActionResult> EditPost(string id, EditCenterUserFrontEndViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageCenterManagerUsers))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid || !await AnyRoleSelectedAsync(viewModel))
            {
                await AddCenterAuthorizationViewDataAsync();

                return View(viewModel);
            }

            if (!(await _userManager.FindByIdAsync(id) is User localUser))
            {
                return NotFound();
            }

            if (!await AuthorizedToEditCenterUser(localUser))
            {
                return Unauthorized();
            }

            try
            {
                var dokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);
                if (dokiNetMember == null)
                {
                    throw new MemberNotFoundException();
                }

                await RevokeAllMDTRolesAsync(localUser);
                using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
                {
                    await _sharedUserService.CreateOrUpdateSharedUserUsingDokiNetMemberAsync(
                        scope,
                        dokiNetMember,
                        await GetLocalUserCenterRolesAsync(viewModel, localUser),
                        this);
                }

                _notifier.Success(T["A felhasználó módosítása megtörtént."]);

                return RedirectToAction(nameof(Index));
            }
            catch (MemberNotFoundException ex)
            {
                _session.Cancel();
                _notifier.Error(T["A megadott tag nem található."]);

                await AddCenterAuthorizationViewDataAsync();

                _logger.LogError(ex, ex.Message, viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);

                return View(viewModel);
            }
            catch (DokiNetMemberRegistrationException ex)
            {
                _session.Cancel();
                ModelState.AddModelError(string.Empty, ex.Message);
                _notifier.Error(T["A felhasználó módosítása nem sikerült."]);

                await AddCenterAuthorizationViewDataAsync();

                _logger.LogError(ex, ex.Message, viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);

                return View(viewModel);
            }
            catch (HttpRequestException ex)
            {
                _session.Cancel();
                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                await AddCenterAuthorizationViewDataAsync();

                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById", viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _session.Cancel();
                _notifier.Error(T["A felhasználó módosítása nem sikerült."]);

                await AddCenterAuthorizationViewDataAsync();

                _logger.LogError(ex, ex.Message);

                return View(viewModel);
            }
        }

        [Route("szeh-felhasznalok/torles/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageCenterManagerUsers))
            {
                return Unauthorized();
            }

            if (!(await _userManager.FindByIdAsync(id) is User localUser))
            {
                return NotFound();
            }

            if (!await AuthorizedToEditCenterUser(localUser))
            {
                return Unauthorized();
            }

            try
            {
                if (localUser.RoleNames.Contains(CenterPosts.TerritorialRapporteur))
                {
                    await _territoryService.RemoveTerritoriesFromUserAsync(localUser.Id);
                }

                await RevokeAllMDTRolesAsync(localUser);
                using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
                {
                    var result = await _sharedUserService.DeleteLocalUserIfHasNoRolesAsync(scope, localUser);
                    if (result == IdentityResult.Success)
                    {
                        if (!localUser.RoleNames.Any())
                        {
                            _notifier.Success(T["A felhasználó törlése megtörtént."]);
                        }
                        else
                        {
                            _notifier.Success(T["A felhasználó jogai megvonásra kerültek."]);
                        }
                    }
                    else
                    {
                        _session.Cancel();

                        _notifier.Error(T["A felhasználó törlése nem sikerült."]);

                        foreach (var error in result.Errors)
                        {
                            _notifier.Error(T[error.Description]);
                        }
                    }
                }
            }
            catch
            {
                _session.Cancel();

                _notifier.Error(T["A felhasználó törlése nem sikerült."]);
            }

            return RedirectToAction(nameof(Index));
        }


        private async Task RevokeAllMDTRolesAsync(User user)
        {
            await _userManager.RemoveFromRoleAsync(user, CenterPosts.MDTManagement);
            await _userManager.RemoveFromRoleAsync(user, CenterPosts.MDTSecretary);
            await _userManager.RemoveFromRoleAsync(user, CenterPosts.OMKB);
            await _userManager.RemoveFromRoleAsync(user, CenterPosts.TerritorialRapporteur);
        }

        private async Task AddCenterAuthorizationViewDataAsync()
        {
            ViewData[nameof(ManagerPermissions.ManageMDTManagement)] = await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageMDTManagement);
            ViewData[nameof(ManagerPermissions.ManageMDTSecretary)] = await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageMDTSecretary);
            ViewData[nameof(ManagerPermissions.ManageOMKB)] = await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageOMKB);
            ViewData[nameof(ManagerPermissions.ManageTerritorialRapporteurs)] = await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageTerritorialRapporteurs);
        }

        private async Task<bool> AuthorizedToEditCenterUser(User user)
        {
            if (user.RoleNames.Contains(CenterPosts.MDTManagement))
            {
                return await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageMDTManagement);
            }

            if (user.RoleNames.Contains(CenterPosts.MDTSecretary))
            {
                return await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageMDTSecretary);
            }

            if (user.RoleNames.Contains(CenterPosts.OMKB))
            {
                return await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageOMKB);
            }

            if (user.RoleNames.Contains(CenterPosts.TerritorialRapporteur))
            {
                return await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageTerritorialRapporteurs);
            }

            return false;
        }

        private async Task<IList<string>> GetLocalUserCenterRolesAsync(EditCenterUserFrontEndViewModel viewModel, User user = null)
        {
            var result = new List<string>();

            if (await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageMDTManagement) &&
                viewModel.RoleManageMDTManagement.IsSelected)
            {
                result.Add(CenterPosts.MDTManagement);
            }

            if (await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageMDTSecretary) &&
                viewModel.RoleManageMDTSecretary.IsSelected)
            {
                result.Add(CenterPosts.MDTSecretary);
                result.Add(CenterPosts.OMKB);
            }

            if (await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageOMKB) &&
                !viewModel.RoleManageMDTSecretary.IsSelected &&
                viewModel.RoleManageOMKB.IsSelected)
            {
                result.Add(CenterPosts.OMKB);
            }

            if (await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageTerritorialRapporteurs))
            {
                if (viewModel.RoleManageTerritorialRapporteur.IsSelected)
                {
                    result.Add(CenterPosts.TerritorialRapporteur);
                }
                else if (user != null)
                {
                    await _territoryService.RemoveTerritoriesFromUserAsync(user.Id);
                }
            }

            return result;
        }

        private void UpdateRolesInViewModel(EditCenterUserFrontEndViewModel viewModel, User user = null, string newUsersRole = null)
        {
            var isnew = user == null;

            viewModel.RoleManageMDTManagement = new RoleViewModel()
            {
                Role = CenterPosts.MDTManagement,
                IsSelected = !isnew
                    ? user.RoleNames.Contains(CenterPosts.MDTManagement)
                    : newUsersRole == CenterPosts.MDTManagement
            };

            viewModel.RoleManageMDTSecretary = new RoleViewModel()
            {
                Role = CenterPosts.MDTSecretary,
                IsSelected = !isnew
                    ? user.RoleNames.Contains(CenterPosts.MDTSecretary)
                    : newUsersRole == CenterPosts.MDTSecretary
            };

            viewModel.RoleManageOMKB = new RoleViewModel()
            {
                Role = CenterPosts.OMKB,
                IsSelected = !isnew
                    ? user.RoleNames.Contains(CenterPosts.OMKB)
                    : newUsersRole == CenterPosts.OMKB || newUsersRole == CenterPosts.MDTSecretary
            };

            viewModel.RoleManageTerritorialRapporteur = new RoleViewModel()
            {
                Role = CenterPosts.TerritorialRapporteur,
                IsSelected = !isnew
                    ? user.RoleNames.Contains(CenterPosts.TerritorialRapporteur)
                    : newUsersRole == CenterPosts.TerritorialRapporteur
            };
        }

        private async Task<bool> AnyRoleSelectedAsync(EditCenterUserFrontEndViewModel viewModel)
        {
            if (await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageMDTManagement) &&
                viewModel.RoleManageMDTManagement.IsSelected)
            {
                return true;
            }

            if (await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageMDTSecretary) &&
                viewModel.RoleManageMDTSecretary.IsSelected)
            {
                return true;
            }

            if (await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageOMKB) &&
               viewModel.RoleManageOMKB.IsSelected)
            {
                return true;
            }

            if (await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageTerritorialRapporteurs) &&
               viewModel.RoleManageTerritorialRapporteur.IsSelected)
            {
                return true;
            }

            ModelState.AddModelError(string.Empty, T["Legalább egy jogosultság kiválasztása kötelező."].Value);

            return false;
        }
    }
}
