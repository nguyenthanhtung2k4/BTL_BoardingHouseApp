using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoardingHouseApp.Data;
using BoardingHouseApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BoardingHouseApp.Controllers
{
    public class RoomsController : Controller
    {
        private readonly AppDbContext _context;

        public RoomsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Rooms
        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(rooms);
        }

        // GET: Room/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // POST: /Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomNumber,Price")] Room room)
        {
            if (ModelState.IsValid)
            {
                room.CreatedAt = DateTime.Now;
                room.UpdatedAt = DateTime.Now;
                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: /Rooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        // POST: /Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,RoomNumber,Price")] Room room)
        {
            if (id != room.RoomId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRoom = await _context.Rooms.FindAsync(id);
                    if (existingRoom == null) return NotFound();

                    existingRoom.RoomNumber = room.RoomNumber;
                    existingRoom.Price = room.Price;
                    existingRoom.UpdatedAt = DateTime.Now;

                    _context.Update(existingRoom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Rooms.Any(e => e.RoomId == room.RoomId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: /Rooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);
            if (room == null) return NotFound();
            return View(room);
        }

        // POST: /Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
