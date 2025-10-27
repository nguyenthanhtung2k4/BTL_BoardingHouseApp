using System;
using System.Linq;
using System.Threading.Tasks;
using BoardingHouseApp.Data;
using BoardingHouseApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BoardingHouseApp.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // --- PHƯƠNG THỨC HỖ TRỢ ---
        private int GetCurrentTenantId()
        {
            var tenantIdClaim = User.FindFirst("TenantId");
            if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                return tenantId;
            }
            return 0;
        }

        // Phương thức hỗ trợ nạp Dropdown Hợp đồng - CHỈ ADMIN
        private void PopulateContractsDropdown(int? selectedContractId = null)
        {
            if (User.IsInRole("Admin"))
            {
                var contractsList = _context.Contracts
                    .Include(c => c.Room)
                    .Include(c => c.Tenant)
                    .Where(c => c.IsActive && !c.IsDeleted)
                    .Select(c => new
                    {
                        c.Id,
                        DisplayText = $"HD #{c.Id} - Phòng: {c.Room!.RoomNumber} - Người thuê: {c.Tenant!.FullName}"
                    })
                    .OrderBy(c => c.Id);

                ViewData["ContractId"] = new SelectList(contractsList, "Id", "DisplayText", selectedContractId);
            }
        }

        private void NormalizePaymentStatus(Payment payment)
        {
            if (payment.Status == 1)
            {
                if (!payment.PaymentDate.HasValue)
                {
                    payment.Status = 0;
                }
            }
            else if (payment.Status == 0 || payment.Status == 2)
            {
                payment.PaymentDate = null;
            }
        }

        // --- CÁC ACTIONS CHÍNH (CRUD) ---

        // GET: Payments/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IQueryable<Payment> paymentsQuery = _context.Payments
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Room)
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Tenant);

            // PHÂN QUYỀN: Tenant chỉ xem payments của mình
            if (User.IsInRole("Tenant"))
            {
                var tenantId = GetCurrentTenantId();
                paymentsQuery = paymentsQuery.Where(p => p.Contract!.TenantId == tenantId);
            }

            var payments = await paymentsQuery
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(payments);
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Room)
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null) return NotFound();

            // PHÂN QUYỀN: Tenant chỉ xem payments của mình
            if (User.IsInRole("Tenant"))
            {
                var tenantId = GetCurrentTenantId();
                if (payment.Contract?.TenantId != tenantId)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xem hóa đơn này.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(payment);
        }

        // GET: Payments/Create - CHỈ ADMIN
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            PopulateContractsDropdown();
            return View();
        }

        // POST: Payments/Create - CHỈ ADMIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Description,PaymentDate,PaymentMethod,Amount,ContractId,Status")] Payment payment)
        {
            NormalizePaymentStatus(payment);
            payment.CreatedAt = DateTime.Now;
            payment.UpdatedAt = DateTime.Now;

            ModelState.Remove("Contract");

            if (ModelState.IsValid)
            {
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo thanh toán thành công!";
                return RedirectToAction(nameof(Index));
            }

            PopulateContractsDropdown(payment.ContractId);
            return View(payment);
        }

        // GET: Payments/Edit/5 - CHỈ ADMIN
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            PopulateContractsDropdown(payment.ContractId);
            return View(payment);
        }

        // POST: Payments/Edit/5 - CHỈ ADMIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,PaymentDate,PaymentMethod,Amount,ContractId,Status,CreatedAt")] Payment payment)
        {
            if (id != payment.Id) return NotFound();

            NormalizePaymentStatus(payment);
            payment.UpdatedAt = DateTime.Now;

            ModelState.Remove("Contract");
            ModelState.Remove("UpdatedAt");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thanh toán thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Payments.Any(e => e.Id == payment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateContractsDropdown(payment.ContractId);
            return View(payment);
        }

        // GET: Payments/Delete/5 - CHỈ ADMIN
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Room)
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null) return NotFound();

            return View(payment);
        }

        // POST: Payments/Delete/5 - CHỈ ADMIN
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa thanh toán thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}