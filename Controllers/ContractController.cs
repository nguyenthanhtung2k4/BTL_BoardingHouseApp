using Microsoft.AspNetCore.Mvc;

namespace BoardingHouseApp.Controllers
{
    public class ContractController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
