using Microsoft.AspNetCore.Mvc;

namespace ToplulukYonetimSistemi.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
