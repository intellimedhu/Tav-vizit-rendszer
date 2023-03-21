using OrganiMedCore.Bootstrap.Models;
using System.Threading.Tasks;

namespace OrganiMedCore.Bootstrap.Services
{
    public interface IBoostrapStyleProvider
    {
        Task<BootstrapStyle[]> GetStylesAsync();
    }
}
