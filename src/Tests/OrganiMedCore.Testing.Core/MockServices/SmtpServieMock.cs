using OrchardCore.Email;
using System.Threading.Tasks;

namespace OrganiMedCore.Testing.Core.MockServices
{
    public class SmtpServieMock : ISmtpService
    {
        private readonly SmtpResult _expectedResult;


        public SmtpServieMock(SmtpResult expectedResult)
        {
            _expectedResult = expectedResult;
        }


        public Task<SmtpResult> SendAsync(MailMessage message)
        {
            return Task.FromResult(_expectedResult);
        }
    }
}
