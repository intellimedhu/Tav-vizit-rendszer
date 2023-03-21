using System.Threading.Tasks;

namespace OrganiMedCore.Email.Services
{
    public interface IEmailNotificationDeliveryService
    {
        Task SendAsync();
    }
}
