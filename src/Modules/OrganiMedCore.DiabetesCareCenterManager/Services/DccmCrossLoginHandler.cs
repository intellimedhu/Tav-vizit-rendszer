using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenterManager.Settings;
using System;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class DccmCrossLoginHandler : IDccmCrossLoginHandler
    {
        private readonly ISession _session;


        public DccmCrossLoginHandler(ISession session)
        {
            _session = session;
        }


        public async Task<Guid> GenerateNonceAsync(int memberRightId)
        {
            var nonceSettings = await _session.Query<DccmLoginNonce>().FirstOrDefaultAsync();
            if (nonceSettings == null)
            {
                nonceSettings = new DccmLoginNonce();
            }

            var nonce = Guid.NewGuid();
            nonceSettings.Storage.Add(nonce, memberRightId);
            _session.Save(nonceSettings);

            return nonce;
        }

        public async Task<int?> ValidateNonceAsync(Guid nonce)
        {
            var nonceSettings = await _session.Query<DccmLoginNonce>().FirstOrDefaultAsync();
            if (nonceSettings == null)
            {
                nonceSettings = new DccmLoginNonce();
            }

            if (!nonceSettings.Storage.ContainsKey(nonce))
            {
                return null;
            }

            var memberRightId = nonceSettings.Storage[nonce];

            nonceSettings.Storage.Remove(nonce);
            _session.Save(nonceSettings);

            return memberRightId;
        }
    }
}
