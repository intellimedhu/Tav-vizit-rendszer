using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Xunit;

namespace IntelliMedCore.Tests
{
    public class PasswordGeneratorServiceTests
    {
        [Theory]
        [InlineData(true, false, false, false)]
        [InlineData(true, true, false, false)]
        [InlineData(true, true, true, true)]
        [InlineData(false, true, false, false)]
        [InlineData(false, true, true, false)]
        [InlineData(false, true, true, true)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, true, true)]
        [InlineData(false, false, false, true)]
        public void GenerateRandomPassword_RequireDigit_ShouldContainDigit(
            bool requireDigit,
            bool requireUppercase,
            bool requireLowercase,
            bool requireNonAlphanumeric)
        {
            var service = new PasswordGeneratorService(new Identity(new IdentityOptions()
            {
                Password = new PasswordOptions()
                {
                    RequireDigit = requireDigit,
                    RequireUppercase = requireUppercase,
                    RequireLowercase = requireLowercase,
                    RequireNonAlphanumeric = requireNonAlphanumeric
                }
            }));

            var password = service.GenerateRandomPassword(20);

            if (requireDigit)
            {
                Assert.Matches(new Regex(@"\d"), password);
            }

            if (requireUppercase)
            {
                Assert.Matches(new Regex(@"[A-Z]"), password);
            }

            if (requireLowercase)
            {
                Assert.Matches(new Regex(@"[a-z]"), password);
            }

            if (requireNonAlphanumeric)
            {
                Assert.Matches(new Regex(@"[!@$?_-]"), password);
            }
        }

        [Fact]
        public void GenerateRandomPassword_RequiredLenght()
        {
            const int length = 13;
            var service = new PasswordGeneratorService(new Identity(new IdentityOptions()));
            var result = service.GenerateRandomPassword(length);
            Assert.Equal(length, result.Length);
        }


        private class Identity : IOptions<IdentityOptions>
        {
            public IdentityOptions Value { get; private set; }


            public Identity(IdentityOptions options)
            {
                Value = options;
            }
        }
    }
}
