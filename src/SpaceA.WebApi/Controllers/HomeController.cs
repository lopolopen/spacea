using Microsoft.AspNetCore.Mvc;

namespace SpaceA.WebApi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}