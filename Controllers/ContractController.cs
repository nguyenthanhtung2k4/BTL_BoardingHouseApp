using BoardingHouseApp.Data;
using BoardingHouseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BoardingHouseApp.Controllers
{
    public class ContractController : Controller
    {
        private readonly AppDbContext _context;
        public ContractController(AppDbContext context)
        {
        _context = context;

        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Eager loading Tenant và Room để hiển thị thông tin chi tiết hơn
            var contracts = _context.Contracts
                .Include(c => c.Tenant)
                .Include(c => c.Room)
                .OrderByDescending(c => c.StartDate); // Sắp xếp theo ngày bắt đầu mới nhất

            return View(await contracts.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Tenant)
                .Include(c => c.Room)
                .Include(c => c.Payments) // Có thể muốn xem danh sách thanh toán
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        public IActionResult Create()
        {
            // Chuẩn bị SelectList cho TenantId và RoomId
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "FullName"); // Giả sử Tenant có thuộc tính FullName
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomName"); // Giả sử Room có thuộc tính RoomName
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenantId,RoomId,StartDate,EndDate")] Contracts contract)
        {
            // Thiết lập ngày tạo và cập nhật
            contract.CreatedAt = DateTime.Now;
            contract.UpdateAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "FullName", contract.TenantId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomName", contract.RoomId);
            return View(contract);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            // Chuẩn bị SelectList
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "FullName", contract.TenantId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomName", contract.RoomId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenantId,RoomId,StartDate,EndDate,CreatedAt")] Contracts contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Thiết lập ngày cập nhật
                    contract.UpdateAt = DateTime.Now;

                    // Do CreatedAt đã được bind, chỉ cần thiết lập UpdatedAt và trạng thái là Modified
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
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

            // Nếu model không hợp lệ, load lại SelectList và trả về View
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "FullName", contract.TenantId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomName", contract.RoomId);
            return View(contract);
        }



        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Tenant)
                .Include(c => c.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------------------

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }


    }
}
