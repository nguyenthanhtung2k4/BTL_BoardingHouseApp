using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardingHouseApp.Data;
using BoardingHouseApp.Models;
using System.Threading.Tasks;
using System.Linq;

namespace BoardingHouseApp.Controllers
{
    public class TenantsController : Controller
    {
        private readonly AppDbContext _context;

        public TenantsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Tenants
        public async Task<IActionResult> Index()
        {
            var tenants = await _context.Tenants
                .Include(t => t.TenantId)
                .ToListAsync();
            return View(tenants);
        }

        // GET: /Tenants/Create
        public async Task<IActionResult> Create()
        {
            await PrepareRoomsDropDown();
            return View();
        }

        // POST: /Tenants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenantId,FullName,Phone,Email,RoomId")] Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tenant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await PrepareRoomsDropDown(tenant.TenantId);
            return View(tenant);
        }

        // GET: /Tenants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound();
            await PrepareRoomsDropDown(tenant.TenantId);
            return View(tenant);
        }

        // POST: /Tenants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TenantId,FullName,Phone,Email,RoomId")] Tenant tenant)
        {
            if (id != tenant.TenantId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tenant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TenantExists(tenant.TenantId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PrepareRoomsDropDown(tenant.TenantId);
            return View(tenant);
        }

        // GET: /Tenants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var tenant = await _context.Tenants
                .Include(t => t.TenantId)
                .FirstOrDefaultAsync(m => m.TenantId == id);
            if (tenant == null) return NotFound();
            return View(tenant);
        }

        // POST: /Tenants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant != null)
            {
                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TenantExists(int id) =>
            _context.Tenants.Any(e => e.TenantId == id);

        private async Task PrepareRoomsDropDown(int? selectedRoomId = null)
        {
            var rooms = await _context.Rooms
                .Select(r => new { r.RoomId, Display = r.RoomNumber })
                .ToListAsync();
            ViewData["RoomId"] = new SelectList(rooms, "RoomId", "Display", selectedRoomId);
        }
    }
}
