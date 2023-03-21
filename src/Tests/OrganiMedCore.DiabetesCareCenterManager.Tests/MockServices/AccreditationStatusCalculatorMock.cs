using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests.MockServices
{
    public class AccreditationStatusCalculatorMock : IAccreditationStatusCalculator
    {
        private readonly AccreditationStatus? _expectedStatus;


        public AccreditationStatusCalculatorMock(AccreditationStatus? expectedStatus = null)
        {
            _expectedStatus = expectedStatus;
        }


        public Task<AccreditationStatusResult> CalculateAccreditationStatusAsync(CenterProfilePart part)
            => Task.FromResult(new AccreditationStatusResult()
            {
                AccreditationStatus = _expectedStatus ?? AccreditationStatus.Accredited
            });
    }
}
