using System;
using System.Linq;
using System.Threading.Tasks;
using BoardingHouseApp.Data;
using BoardingHouseApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

        // Phương thức hỗ trợ nạp Dropdown Hợp đồng
        private void PopulateContractsDropdown(int? selectedContractId = null)
        {
            // Lấy các Hợp đồng đang hoạt động, hiển thị RoomNumber và Tenant Name
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

        // FIX LOGIC: Chuẩn hóa dữ liệu Status và PaymentDate
        private void NormalizePaymentStatus(Payment payment)
        {
            // Nếu trạng thái là Đã thanh toán (Status = 1)
            if (payment.Status == 1)
            {
                // Bắt buộc phải có Ngày Thanh Toán Thực Tế
                if (!payment.PaymentDate.HasValue)
                {
                    // Nếu Status = 1 nhưng không có ngày, chuyển về Status = 0
                    payment.Status = 0;
                }
            }
            // Nếu trạng thái là Chưa thanh toán (Status = 0: Hóa đơn) HOẶC Quá hạn (Status = 2)
            else if (payment.Status == 0 || payment.Status == 2)
            {
                // Thanh toán chưa thực hiện, Ngày Thanh Toán Thực Tế phải là NULL
                payment.PaymentDate = null;
            }
        }

        // --- CÁC ACTIONS CHÍNH (CRUD) ---

        // GET: Payments/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pay = _context.Payments
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Room)
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Tenant)
                .OrderByDescending(p => p.CreatedAt);

            return View(await pay.ToListAsync());
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

            return View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            PopulateContractsDropdown();
            return View();
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,PaymentDate,PaymentMethod,Amount,ContractId,Status")] Payment payment)
        {
            // Áp dụng logic chuẩn hóa (FIX LỖI)
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

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            PopulateContractsDropdown(payment.ContractId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Payments/Delete/5
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

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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