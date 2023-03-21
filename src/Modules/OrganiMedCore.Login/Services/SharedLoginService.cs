using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Email;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;
using OrganiMedCore.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.Login.Services
{
    public class SharedLoginService : ISharedLoginService
    {
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly UserManager<IUser> _userManager;
        private readonly IUserService _userService;
        private readonly ShellSettings _shellSettings;
        private readonly IPasswordGeneratorService _passwordGeneratorService;


        public IStringLocalizer T { get; set; }


        public SharedLoginService(
            ISharedDataAccessorService sharedDataAccessorService,
            UserManager<IUser> userManager,
            IUserService userService,
            ShellSettings shellSettings,
            IStringLocalizer<SharedLoginService> stringLocalizer,
            IPasswordGeneratorService passwordGeneratorService)
        {
            _sharedDataAccessorService = sharedDataAccessorService;
            _userManager = userManager;
            _userService = userService;
            _shellSettings = shellSettings;
            _passwordGeneratorService = passwordGeneratorService;

            T = stringLocalizer;
        }


        public async Task<IUser> FindSharedUserByUserNameAsync(IServiceScope managersServiceScope, string userName)
        {
            var managersUserManager = managersServiceScope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var sharedUser = await managersUserManager.FindByNameAsync(userName);
            if (sharedUser == null || !(sharedUser as User).As<EVisitUser>().IsEVisitUser)
            {
                return null;
            }

            return sharedUser;
        }

        public bool IsEVisitUser(IUser sharedUser)
        {
            var eVisitUser = (sharedUser as User).As<EVisitUser>();

            return eVisitUser.IsEVisitUser;
        }

        public async Task<IUser> CheckPasswordSignInAsync(IServiceScope managersServiceScope, IUser sharedUser, string password)
        {
            var managersUserService = managersServiceScope.ServiceProvider.GetRequiredService<SignInManager<IUser>>();
            if (!(await managersUserService.CheckPasswordSignInAsync(sharedUser, password, false)).Succeeded)
            {
                return null;
            }

            // Shared user ID is stored in the tenant's user's username.
            return await _userManager.FindByNameAsync((sharedUser as User).Id.ToString());
        }

        public async Task<IUser> SharedRegisterAsync(
            IServiceScope managersServiceScope,
            string email,
            bool emailConfirmed,
            string password,
            IList<string> tenantRoles,
            Action<User> sharedUserAlteration,
            Action<string, string> reportError)
        {
            var managersUserService = managersServiceScope.ServiceProvider.GetRequiredService<IUserService>();
            // The username is always the email.
            var sharedIUser = await managersUserService.CreateUserAsync(new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = emailConfirmed,
                RoleNames = new List<string>()
            }, password, reportError);
            if (sharedIUser == null)
            {
                return null;
            }

            var sharedUser = (User)sharedIUser;
            sharedUser.Alter<EVisitUser>(nameof(EVisitUser), x =>
            {
                x.IsEVisitUser = true;
                x.EVisitLoginEnabled = true;
                x.PermittedTenans.Add(_shellSettings.Name);
            });

            sharedUserAlteration?.Invoke(sharedUser);

            var managersSession = managersServiceScope.ServiceProvider.GetRequiredService<ISession>();

            // Shared user ID is stored in the tenant's username.
            var tenantUser = await _userService.CreateUserAsync(
                new User
                {
                    UserName = sharedUser.Id.ToString(),
                    Email = email,
                    RoleNames = tenantRoles
                },
                _passwordGeneratorService.GenerateRandomPassword(16),
                reportError);

            if (tenantUser == null)
            {
                managersSession.Cancel();
            }
            else
            {
                managersSession.Save(sharedIUser);
            }

            return tenantUser;
        }

        public async Task<bool> ChangeSharedUsersPasswordAsync(IServiceScope managersServiceScope, ClaimsPrincipal principal, string currentPassword, string newPassword, Action<string, string> reportError)
        {
            var user = await _userService.GetAuthenticatedUserAsync(principal);
            var managersUserManager = managersServiceScope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var sharedUser = await managersUserManager.FindByIdAsync(user.UserName);
            if (sharedUser == null || !AuthorizedToSignInOnTenant(sharedUser))
            {
                return false;
            }

            var managersUserService = managersServiceScope.ServiceProvider.GetRequiredService<IUserService>();

            return await managersUserService.ChangePasswordAsync(sharedUser, currentPassword, newPassword, reportError);
        }

        public async Task SharedForgotPasswordAsync(IServiceScope managersServiceScope, string userIdentifier, ControllerContext controllerContext, IUrlHelper urlHelper, Microsoft.AspNetCore.Http.HttpContext httpContext, ViewDataDictionary viewData, ITempDataDictionary tempData)
        {
            var managersUserService = managersServiceScope.ServiceProvider.GetRequiredService<IUserService>();
            var managersSession = managersServiceScope.ServiceProvider.GetRequiredService<ISession>();
            var managersUserManager = managersServiceScope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var user = (User)await managersUserService.GetForgotPasswordUserAsync(userIdentifier);

            if (user == null || !(await managersUserManager.IsEmailConfirmedAsync(user)))
            {
                return;
            }

            user.ResetToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.ResetToken));
            // send email with callback link
            // TODO: this e-mail sending is a copy from the original controller but it's a mess, hopefully they will refactor it.
            var resetPasswordUrl = urlHelper.Action("ResetPassword", "Password", new { code = user.ResetToken }, httpContext.Request.Scheme);
            await SendEmailAsync(managersServiceScope, user, controllerContext, urlHelper, httpContext, viewData, tempData, user.Email, T["Reset your password"], new LostPasswordViewModel() { User = user, LostPasswordUrl = resetPasswordUrl }, "TemplateUserLostPassword");

            managersSession.Save(user);

            return;
        }

        public async Task<bool> SharedResetPasswordAsync(IServiceScope managersServiceScope, string userIdentifier, string resetToken, string newPassword, Action<string, string> reportError)
        {
            var managersUserService = managersServiceScope.ServiceProvider.GetRequiredService<IUserService>();

            return await managersUserService.ResetPasswordAsync(userIdentifier, Encoding.UTF8.GetString(Convert.FromBase64String(resetToken)), newPassword, reportError);
        }

        public async Task<bool> SendEmailAsync(IServiceScope managersServiceScope, User user, ControllerContext controllerContext, IUrlHelper urlHelper, Microsoft.AspNetCore.Http.HttpContext httpContext, ViewDataDictionary viewData, ITempDataDictionary tempData, string email, string subject, object model, string viewName)
        {
            // TODO: itt a FindView-nál a view null
            var managersShapeFactory = managersServiceScope.ServiceProvider.GetRequiredService<IShapeFactory>();
            var managersDisplayManager = managersServiceScope.ServiceProvider.GetRequiredService<IHtmlDisplay>();
            var managersSmtpService = managersServiceScope.ServiceProvider.GetRequiredService<ISmtpService>();

            var options = httpContext.RequestServices.GetRequiredService<IOptions<MvcViewOptions>>();
            controllerContext.RouteData.Values["action"] = viewName;
            controllerContext.RouteData.Values["controller"] = "";
            var viewEngineResult = options.Value.ViewEngines.Select(x => x.FindView(controllerContext, viewName, true)).FirstOrDefault(x => x != null);
            var displayContext = new DisplayContext()
            {
                ServiceProvider = controllerContext.HttpContext.RequestServices,
                Value = await managersShapeFactory.CreateAsync(viewName, model),
                //ViewContext = new ViewContext(controllerContext, viewEngineResult.View, viewData, tempData, new StringWriter(), new HtmlHelperOptions())
            };
            var htmlContent = await managersDisplayManager.ExecuteAsync(displayContext);

            var message = new MailMessage()
            {
                Body = htmlContent.ToString(),
                IsBodyHtml = true,
                Subject = subject,
                To = email
            };

            // send email
            var result = await managersSmtpService.SendAsync(message);

            return result.Succeeded;
        }

        public bool AuthorizedToSignInOnTenant(IUser sharedUser)
        {
            var eVisitUser = (sharedUser as User).As<EVisitUser>();

            return eVisitUser.IsEVisitUser &&
                eVisitUser.EVisitLoginEnabled &&
                eVisitUser.PermittedTenans.Contains(_shellSettings.Name);
        }
    }
}
