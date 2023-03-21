namespace OrganiMedCore.Email.Services
{
    public interface IEmailDataProcessorFactory
    {
        IEmailDataProcessor ResolveEmailDataProcessor(string templateId);
    }
}
