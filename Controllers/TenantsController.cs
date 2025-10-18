using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoardingHouseApp.Data;
using BoardingHouseApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace BoardingHouseApp.Controllers
{
    [Authorize]
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
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return View(tenants);
        }

        // GET: /Tenants/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Tenants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Phone,Email")] Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                tenant.CreatedAt = DateTime.Now;
                tenant.UpdatedAt = DateTime.Now;

                _context.Add(tenant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tenant);
        }

        // GET: /Tenants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound();
            return View(tenant);
        }

        // POST: /Tenants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TenantId,FullName,Phone,Email")] Tenant tenant)
        {
            if (id != tenant.TenantId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTenant = await _context.Tenants.FindAsync(id);
                    if (existingTenant == null) return NotFound();

                    existingTenant.FullName = tenant.FullName;
                    existingTenant.Phone = tenant.Phone;
                    existingTenant.Email = tenant.Email;
                    existingTenant.UpdatedAt = DateTime.Now;

                    _context.Update(existingTenant);
                    await _context.SaveChangesAsync();
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

        // GET: /Tenants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var tenant = await _context.Tenants.FirstOrDefaultAsync(m => m.TenantId == id);
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
    }
}
