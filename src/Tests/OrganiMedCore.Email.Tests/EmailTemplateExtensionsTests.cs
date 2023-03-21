using OrganiMedCore.Email.Extensions;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.Email.Tests
{
    public class EmailTemplateExtensionsTests
    {
        [Theory]
        [InlineData("This is a -kind- day", "kind", "nice", "This is a nice day")]
        [InlineData("This is a kind day", "kind", "nice", "This is a kind day")]
        [InlineData("This is a -kind- day", "kin", "nice", "This is a -kind- day")]
        public void ReplaceToken_ShouldReplace_AsExpected(string subject, string token, string newValue, string expectedResult)
        {
            var result = subject.ReplaceToken(token, newValue);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ReplaceTokenAsync_ShouldReplace_AsExpected()
        {
            var subject = $"-where- -who- go";

            var result = await subject.ReplaceTokenAsync("where", async () =>
            {
                await Task.Delay(1);
                return "Here";
            });

            result = await result.ReplaceTokenAsync("who", async () =>
            {
                await Task.Delay(1);
                return "we";
            });

            Assert.Equal($"Here we go", result);
        }
    }
}
