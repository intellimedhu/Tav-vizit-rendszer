using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class DokiNetMemberValidator : IDokiNetMemberValidator
    {
        public Task<bool> ValidateLoginToTenantAsync<T>(T dokiNetMember, Action<string> reportError)
            where T : DokiNetMember
        {
            dokiNetMember.ThrowIfNull();
            reportError.ThrowIfNull();

            var result = true;
            if (!dokiNetMember.HasMemberShip)
            {
                reportError("Ön nem tagja a Magyar Diabetes Társaságnak, így a bejelentkezés nem lehetséges.");
                result = false;
            }
            else if (!dokiNetMember.IsDueOk)
            {
                reportError("Az Ön tagdíja nem rendezett a Magyar Diabetes Társaság felé, így a bejelentkezés nem lehetséges.");
                result = false;
            }

            return Task.FromResult(result);
        }
    }
}
