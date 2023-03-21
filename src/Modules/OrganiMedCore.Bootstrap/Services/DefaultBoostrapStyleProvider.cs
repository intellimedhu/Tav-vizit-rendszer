using OrganiMedCore.Bootstrap.Models;
using System.Threading.Tasks;

namespace OrganiMedCore.Bootstrap.Services
{
    public class DefaultBoostrapStyleProvider : IBoostrapStyleProvider
    {
        public Task<BootstrapStyle[]> GetStylesAsync()
            => Task.FromResult(new[]
            {
                new BootstrapStyle() { Name = "Primary", TechnicalName = "primary" },
                new BootstrapStyle() { Name = "Info", TechnicalName = "info" },
                new BootstrapStyle() { Name = "Secondary", TechnicalName = "secondary" },
                new BootstrapStyle() { Name = "Warning", TechnicalName = "warning" },
                new BootstrapStyle() { Name = "Success", TechnicalName = "success" },
                new BootstrapStyle() { Name = "Danger", TechnicalName = "danger" },
                new BootstrapStyle() { Name = "Dark", TechnicalName = "dark" },
                new BootstrapStyle() { Name = "Light", TechnicalName = "light" }
            });
    }
}
