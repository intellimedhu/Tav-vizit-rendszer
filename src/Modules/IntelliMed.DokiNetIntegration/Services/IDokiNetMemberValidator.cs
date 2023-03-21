using IntelliMed.DokiNetIntegration.Models;
using System;
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration.Services
{
    public interface IDokiNetMemberValidator
    {
        Task<bool> ValidateLoginToTenantAsync<T>(T dokiNetMember, Action<string> reportError)
            where T : DokiNetMember;
    }
}
