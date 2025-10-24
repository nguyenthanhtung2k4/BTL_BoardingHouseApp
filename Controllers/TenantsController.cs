using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoardingHouseApp.Data;
using BoardingHouseApp.Models;
using Microsoft.AspNetCore.Authorization;
using BoardingHouseApp.Services;
using System.Security.Claims;

namespace BoardingHouseApp.Controllers
{
    [Authorize]
    public class TenantsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public TenantsController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /Tenants
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                // Admin: xem tất cả tenants
                var tenants = await _context.Tenants
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
                return View(tenants);
            }
            else if (User.IsInRole("Tenant"))
            {
                // Tenant: chỉ xem thông tin của chính mình
                var tenantId = GetCurrentTenantId();
                var tenant = await _context.Tenants
                    .Where(t => t.TenantId == tenantId)
                    .ToListAsync();
                return View(tenant);
            }

            return Forbid();
        }

        // GET: /Tenants/Create - CHỈ ADMIN
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Tenants/Create - CHỈ ADMIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("FullName,Phone,Email")] Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool emailExists = await _context.Tenants
                        .AnyAsync(t => t.Email == tenant.Email);

                    if (emailExists)
                    {
                        TempData["ErrorMessage"] = "Email này đã được đăng ký.";
                        return View(tenant);
                    }

                    string defaultPassword = GenerateDefaultPassword(tenant.Phone);
                    tenant.hashPassword = HashPassword(defaultPassword);
                    tenant.CreatedAt = DateTime.Now;
                    tenant.UpdatedAt = DateTime.Now;

                    _context.Add(tenant);
                    await _context.SaveChangesAsync();

                    string emailBody = $@"
                        <h3>Xin chào {tenant.FullName},</h3>
                        <p>Bạn đã được thêm vào hệ thống quản lý nhà trọ với thông tin đăng nhập sau:</p>
                        
                        <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <p><strong>Email đăng nhập:</strong> {tenant.Email}</p>
                            <p><strong>Mật khẩu mặc định:</strong> {defaultPassword}</p>
                            <p><strong>Số điện thoại:</strong> {tenant.Phone}</p>
                        </div>
                        
                        <p><strong>⚠️ Lưu ý quan trọng:</strong></p>
                        <ul>
                            <li>Không chia sẻ thông tin đăng nhập với người khác</li>
                            <li>Mật khẩu được tạo từ 6 số cuối điện thoại của bạn</li>
                        </ul>";

                    bool emailSent = await _emailService.SendEmailAsync(
                        tenant.Email,
                        tenant.FullName,
                        "Thông tin tài khoản - Boarding House Management",
                        emailBody
                    );
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi: {ex.Message}");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm người thuê.";
                    return View(tenant);
                }
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
            return View(tenant);
        }

        // GET: /Tenants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            if (User.IsInRole("Admin"))
            {
                // Admin có thể sửa bất kỳ tenant nào
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null) return NotFound();
                return View(tenant);
            }
            else if (User.IsInRole("Tenant"))
            {
                // Tenant chỉ có thể sửa thông tin của chính mình
                var tenantId = GetCurrentTenantId();
                if (id != tenantId)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa thông tin người khác.";
                    return RedirectToAction(nameof(Index));
                }

                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null) return NotFound();
                return View(tenant);
            }

            return Forbid();
        }

        // POST: /Tenants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TenantId,FullName,Phone,Email")] Tenant tenant)
        {
            if (User.IsInRole("Tenant"))
            {
                var currentTenantId = GetCurrentTenantId();
                if (id != currentTenantId)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa thông tin người khác.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (id != tenant.TenantId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTenant = await _context.Tenants.FindAsync(id);
                    if (existingTenant == null) return NotFound();

                    // Kiểm tra email trùng (trừ chính nó)
                    bool emailExists = await _context.Tenants
                        .AnyAsync(t => t.Email == tenant.Email && t.TenantId != id);

                    if (emailExists)
                    {
                        TempData["ErrorMessage"] = "Email này đã được đăng ký.";
                        return View(tenant);
                    }

                    existingTenant.FullName = tenant.FullName;
                    existingTenant.Phone = tenant.Phone;
                    existingTenant.Email = tenant.Email;
                    existingTenant.UpdatedAt = DateTime.Now;

                    _context.Update(existingTenant);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tenants.Any(e => e.TenantId == tenant.TenantId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tenant);
        }

        // GET: /Tenants/Delete/5 - CHỈ ADMIN
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var tenant = await _context.Tenants.FirstOrDefaultAsync(m => m.TenantId == id);
            if (tenant == null) return NotFound();
            return View(tenant);
        }

        // POST: /Tenants/Delete/5 - CHỈ ADMIN
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant != null)
            {
                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa người thuê thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // 🔐 HÀM LẤY TENANT ID HIỆN TẠI
        private int GetCurrentTenantId()
        {
            var tenantIdClaim = User.FindFirst("TenantId");
            if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                return tenantId;
            }
            return 0;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private string GenerateDefaultPassword(string phoneNumber)
        {
            string cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            return cleanPhone.Length >= 6 ?
                   cleanPhone.Substring(cleanPhone.Length - 6) :
                   cleanPhone.PadLeft(6, '0');
        }
    }
}