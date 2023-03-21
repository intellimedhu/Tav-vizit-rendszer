using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Services
{
    /// <summary>
    /// Provides services related to the shared (the default tenant stores the users) login.
    /// </summary>
    public interface ISharedLoginService
    {
        Task<IUser> FindSharedUserByUserNameAsync(IServiceScope managersServiceScope, string userName);

        /// <summary>
        /// Determines whether the provided credentials are valid thus the user can sign in.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>The user if it can sign in. Else null.</returns>
        Task<IUser> CheckPasswordSignInAsync(IServiceScope managersServiceScope, IUser sharedUser, string password);

        /// <summary>
        /// Registers the user to the default tenant.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="email">Email.</param>
        /// <param name="emailConfirmed">Email again.</param>
        /// <param name="password">The password.</param>
        /// <param name="tenantRoles">The user's roles in the tenant because the roles are not global.</param>
        /// <param name="reportError">The error reporting action.</param>
        /// <returns>The local tenant user (not the manager tenant's user) in case of success. Null in case of unsuccess.</returns>
        Task<IUser> SharedRegisterAsync(
            IServiceScope managersServiceScope, 
            string email, 
            bool emailConfirmed, 
            string password,
            IList<string> tenantRoles,
            Action<User> sharedUserAlteration, 
            Action<string, string> reportError);

        /// <summary>
        /// Changes the shared user's password.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="principal">The signed in user.</param>
        /// <param name="currentPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="reportError">The error reporting action.</param>
        /// <returns>True in case of success.</returns>
        Task<bool> ChangeSharedUsersPasswordAsync(IServiceScope managersServiceScope, ClaimsPrincipal principal, string currentPassword, string newPassword, Action<string, string> reportError);

        /// <summary>
        /// Starts the forgot password workflow. TODO: refactor it if a newer version of the user management comes out.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="userIdentifier"></param>
        /// <param name="controllerContext"></param>
        /// <param name="urlHelper"></param>
        /// <param name="httpContext"></param>
        /// <param name="viewData"></param>
        /// <param name="tempData"></param>
        Task SharedForgotPasswordAsync(IServiceScope managersServiceScope, string userIdentifier, ControllerContext controllerContext, IUrlHelper urlHelper, HttpContext httpContext, ViewDataDictionary viewData, ITempDataDictionary tempData);

        /// <summary>
        /// Starts the reset password workflow. TODO: refactor it if a newer version of the user management comes out.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="userIdentifier"></param>
        /// <param name="resetToken"></param>
        /// <param name="newPassword"></param>
        /// <param name="reportError"></param>
        /// <returns></returns>
        Task<bool> SharedResetPasswordAsync(IServiceScope managersServiceScope, string userIdentifier, string resetToken, string newPassword, Action<string, string> reportError);

        /// <summary>
        /// Starts the email sending part of the password registration related workflows. TODO: refactor it if a newer version of the user management comes out.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="user"></param>
        /// <param name="controllerContext"></param>
        /// <param name="urlHelper"></param>
        /// <param name="httpContext"></param>
        /// <param name="viewData"></param>
        /// <param name="tempData"></param>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="model"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        Task<bool> SendEmailAsync(IServiceScope managersServiceScope, User user, ControllerContext controllerContext, IUrlHelper urlHelper, Microsoft.AspNetCore.Http.HttpContext httpContext, ViewDataDictionary viewData, ITempDataDictionary tempData, string email, string subject, object model, string viewName);

        bool AuthorizedToSignInOnTenant(IUser sharedUser);
    }
}
