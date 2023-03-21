using IntelliMed.DokiNetIntegration.Models;
using OrchardCore.Users;
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration.Services
{
    public interface IDokiNetUserLoginHandler
    {
        Task HandleUserBeforeLogin(IUser localUser, DokiNetMember dokiNetMember);

        Task HandleUserAfterLogin(IUser localUser, DokiNetMember dokiNetMember);
    }
}
