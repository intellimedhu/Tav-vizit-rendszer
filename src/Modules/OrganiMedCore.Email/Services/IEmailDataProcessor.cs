using System.Threading.Tasks;

namespace OrganiMedCore.Email.Services
{
    public interface IEmailDataProcessor
    {
        string TemplateId { get; }

        Task<string> ProcessAsync(object data, string rawBody);
    }
}
