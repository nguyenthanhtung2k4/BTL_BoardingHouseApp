using Microsoft.AspNetCore.Mvc;

namespace BoardingHouseApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
