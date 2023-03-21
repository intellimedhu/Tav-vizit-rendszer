using OrganiMedCore.Bootstrap.Models;
using OrganiMedCore.Bootstrap.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.InformationOrientedTheme.Services
{
    public class BoostrapStyleProvider : IBoostrapStyleProvider
    {
        public Task<BootstrapStyle[]> GetStylesAsync()
            => Task.FromResult(new[]
            {
                new BootstrapStyle()
                {
                    Name = "Safe",
                    TechnicalName = "safe"
                }
            });
    }
}
