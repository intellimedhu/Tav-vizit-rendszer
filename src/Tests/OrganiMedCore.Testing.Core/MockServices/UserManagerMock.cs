using Microsoft.AspNetCore.Identity;
using Moq;
using System.Collections.Generic;

namespace OrganiMedCore.Testing.Core.MockServices
{
    public static class UserManagerMock
    {
        public static Mock<UserManager<TUser>> Create<TUser>() where TUser : class
        {
            IList<IUserValidator<TUser>> UserValidators = new List<IUserValidator<TUser>>();
            IList<IPasswordValidator<TUser>> PasswordValidators = new List<IPasswordValidator<TUser>>();

            var store = new Mock<IUserStore<TUser>>();
            UserValidators.Add(new UserValidator<TUser>());
            PasswordValidators.Add(new PasswordValidator<TUser>());
            var mgr = new Mock<UserManager<TUser>>(store.Object,
                null,
                null,
                UserValidators,
                PasswordValidators,
                null,
                null,
                null,
                null);
            return mgr;
        }
    }
}
