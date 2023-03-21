using IntelliMed.Core.Exceptions;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.Email.Tests
{
    public class EmailTemplateProviderBaseTests
    {
        [Theory]
        [InlineData("", typeof(ArgumentException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public async Task GetEmailTemplateByIdAsync_ShouldThrow(string templateId, Type exceptionType)
        {
            var templateProvider = new EmailTemplateProviderBaseConcrete(null, null);

            await Assert.ThrowsAsync(exceptionType, () => templateProvider.GetEmailTemplateByIdAsync(templateId));
        }

        [Fact]
        public async Task GetEmailTemplateByIdAsync_ShouldReturnAll()
        {
            var template = new EmailTemplate()
            {
                Id = "t0",
                Name = "n0",
                Tokens = new HashSet<string>() { "a", "b" }
            };

            var templateProvider = new EmailTemplateProviderBaseConcrete(
                new EmailDataProcessorFactory(null), new[] { template });

            var result = await templateProvider.GetEmailTemplateByIdAsync("t0");

            Assert.NotNull(result);
            Assert.Equal(template.Id, result.Id);
            Assert.Equal(template.Name, result.Name);
            Assert.Equal(template.Tokens, result.Tokens);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(7)]
        public async Task GetEmailTemplatesAsync_ShouldReturnAll(int count)
        {
            var emailTemplates = new EmailTemplate[count];
            for (var i = 0; i < count; i++)
            {
                emailTemplates[i] = new EmailTemplate()
                {
                    Id = $"T_{i}",
                    Name = "N" + i,
                    Tokens = new HashSet<string>(Enumerable.Range(0, i).Select(n => "tkn_" + n))
                };
            }

            var templateProvider = new EmailTemplateProviderBaseConcrete(
                new EmailDataProcessorFactory(null),
                emailTemplates);

            var result = await templateProvider.GetEmailTemplatesAsync();

            Assert.Equal(count, result.Count());
            Assert.All(emailTemplates, original => result.Any(t =>
                t.Id == original.Id &&
                t.Name == original.Name &&
                t.Tokens.Equals(original.Tokens)));
        }

        [Fact]
        public async Task ProcessAsync_ShouldThrow()
        {
            var templateProvider = new EmailTemplateProviderBaseConcrete(
                new EmailDataProcessorFactory(new[] { new Edp1("tp1", null) }),
                new[] { new EmailTemplate() { Id = "tp1" } });

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => templateProvider.ProcessAsync(null, new { }, "rawbody"));

            await Assert.ThrowsAsync<ArgumentException>(
                () => templateProvider.ProcessAsync("", new { }, "rawbody"));

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => templateProvider.ProcessAsync("id", null, "rawbody"));

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => templateProvider.ProcessAsync("id", new { }, null));

            await Assert.ThrowsAsync<ArgumentException>(
                () => templateProvider.ProcessAsync("id", new { }, ""));

            await Assert.ThrowsAsync<NotFoundException>(
                () => templateProvider.ProcessAsync("tp2", new { }, "raw body"));
        }

        [Fact]
        public async Task ProcessAsync_ShouldReturnProcessed()
        {
            var templateId = "T_X1";
            var expectedResult = "This will be the result after process finished.";

            var templateProvider = new EmailTemplateProviderBaseConcrete(
                new EmailDataProcessorFactory(
                new[]
                {
                    new Edp1(templateId, expectedResult)
                }),
                new[] { new EmailTemplate() { Id = templateId } });

            var result = await templateProvider.ProcessAsync(templateId, new { }, "rb");
            Assert.Equal(expectedResult, result);
        }
    }

    class EmailTemplateProviderBaseConcrete : EmailTemplateProviderBase
    {
        private readonly IEnumerable<EmailTemplate> _expectedResult;


        public EmailTemplateProviderBaseConcrete(
            IEmailDataProcessorFactory emailDataProcessorFactory,
            IEnumerable<EmailTemplate> expectedResult) :
            base(emailDataProcessorFactory)
        {
            _expectedResult = expectedResult;
        }

        public override Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync()
            => Task.FromResult(_expectedResult);
    }

    class Edp1 : IEmailDataProcessor
    {
        private readonly string _expectedResult;


        public string TemplateId { get; }


        public Edp1(string templateId, string expectedResult)
        {
            _expectedResult = expectedResult;

            TemplateId = templateId;
        }


        public Task<string> ProcessAsync(object data, string rawBody)
            => Task.FromResult(_expectedResult);
    }
}
