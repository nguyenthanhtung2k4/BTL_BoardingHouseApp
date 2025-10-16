using BoardingHouseApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardingHouseApp.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Login() => View();

        public IActionResult Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Username == "admin" && model.Password == "admin")
            {
                // Đăng nhập thành công, chuyển hướng đến trang chủ
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }
        }
    }
}
