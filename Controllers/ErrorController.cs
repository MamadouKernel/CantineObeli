using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Obeli_K.Controllers
{
    public class ErrorController : Controller
    {
        [AllowAnonymous]
        public new IActionResult Unauthorized()
        {
            return View();
        }

        [AllowAnonymous]
        public new IActionResult NotFound()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult ServerError()
        {
            return View();
        }
    }
}
