using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface IDccmCrossLoginHandler
    {
        Task<Guid> GenerateNonceAsync(int memberRightId);

        Task<int?> ValidateNonceAsync(Guid nonce);
    }
}
