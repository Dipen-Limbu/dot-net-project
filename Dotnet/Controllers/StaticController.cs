using Microsoft.AspNetCore.Mvc;

namespace Dotnet.Controllers
{
    public class StaticController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
