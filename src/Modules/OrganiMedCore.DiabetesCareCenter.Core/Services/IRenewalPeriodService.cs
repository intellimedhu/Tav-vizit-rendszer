using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface IRenewalPeriodService
    {
        Task QueueNotificationsAboutUnsubmittedCenterProfile(DateTime utcNow);

        /// <summary>
        /// Sets the all center profiles' "RenewalCenterProfileStatus" property to "Unsubmitted" where the current status is null or 'Unsubmitted'.
        /// </summary>
        /// <returns>Count of affected center profiles.</returns>
        Task<int> ResetCenterProfileStatusesAsync(DateTime utcNow, bool force = false);
    }
}
