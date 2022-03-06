using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioMit.Controllers
{
    public class AntiforgeryController : Controller
    {
        private readonly IAntiforgery _antiforgery;
        public AntiforgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Get()
        {
            AntiforgeryTokenSet tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            return Json(tokens.RequestToken!);
        }
    }
}
