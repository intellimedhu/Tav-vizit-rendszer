using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using IntelliMed.DokiNetIntegration.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration.Controllers.Api
{
    [Authorize]
    [Route("api/doki-net-members", Name = "DokiNetMembersApi")]
    public class DokiNetMembersApiController : Controller
    {
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger _logger;


        public DokiNetMembersApiController(IDokiNetService dokiNetService, ILogger<DokiNetMembersApiController> logger)
        {
            _dokiNetService = dokiNetService;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> Get(string name)
        {
            try
            {
                var members = await _dokiNetService.SearchDokiNetMemberByName<DokiNetMember>(name);

                return Ok(members.Select(member =>
                {
                    var viewModel = new DokiNetMemberSearchResultViewModel();
                    viewModel.UpdateViewModel(member);

                    return viewModel;
                }));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.SearchDokiNetMemberByName", name);

                return Ok(new DokiNetMember[] { });
            }
        }
    }
}
