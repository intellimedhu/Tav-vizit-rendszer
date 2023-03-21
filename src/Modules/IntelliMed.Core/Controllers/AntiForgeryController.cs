using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntelliMed.Core.Controllers.Api
{
    [ApiController]
    [IgnoreAntiforgeryToken]
    [Route("api/aft", Name = "AntiForgeryToken")]
    public class AntiForgeryController : Controller
    {
        private readonly IAntiforgery _antiForgery;


        public AntiForgeryController(IAntiforgery antiForgery)
        {
            _antiForgery = antiForgery;
        }


        [HttpGet]
        public IActionResult Get()
        {
            var tokens = _antiForgery.GetAndStoreTokens(HttpContext);

            Response.Cookies.Append(
                "XSRF_REQUEST_TOKEN",
                tokens.RequestToken,
                new CookieOptions()
                {
                    HttpOnly = false
                });

            return Ok();
        }
    }
}
