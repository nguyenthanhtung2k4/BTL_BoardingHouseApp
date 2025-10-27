using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoardingHouseApp.Models;
using BoardingHouseApp.Data;
using BCrypt.Net;

namespace BoardingHouseApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if ((model.Username == "admin" || model.Username == "adminApp@gmail.com") && model.Password == "admin123")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim("UserType", "Admin"),
                    new Claim("LoginTime", DateTime.Now.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                TempData["SuccessMessage"] = "Đăng nhập admin thành công!";
                return RedirectToAction("Index", "Home");
            }

            // 🔐 KIỂM TRA TENANT TỪ DATABASE
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Email == model.Username && !t.isDeleted);

            if (tenant != null)
            {
                // Verify password với BCrypt
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, tenant.hashPassword);
                
                if (isPasswordValid)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, tenant.FullName),
                        new Claim(ClaimTypes.Email, tenant.Email),
                        new Claim(ClaimTypes.Role, "Tenant"),
                        new Claim("UserType", "Tenant"),
                        new Claim("TenantId", tenant.TenantId.ToString()),
                        new Claim("LoginTime", DateTime.Now.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    TempData["SuccessMessage"] = $"Chào mừng {tenant.FullName}!";
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Đã đăng xuất thành công!";
            return RedirectToAction("Login", "Login");
        }
    }
}