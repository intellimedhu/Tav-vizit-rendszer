using IntelliMed.DokiNetIntegration.Models;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class DokiNetMemberValidatorTest
    {
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public async Task DokiNetMemberValidator_ShouldBeInvalid_ForMembership(bool hasMembership, bool membershipFeePaid, bool expectedResult)
        {
            var validator = new DokiNetMemberValidator();

            var member = new DokiNetMember()
            {
                HasMemberShip = hasMembership,
                IsDueOk = membershipFeePaid
            };

            var errors = new List<string>();

            Assert.Equal(expectedResult, await validator.ValidateLoginToTenantAsync(member, msg => errors.Add(msg)));
            if (!expectedResult)
            {
                Assert.NotEmpty(errors);
            }
        }
    }
}
