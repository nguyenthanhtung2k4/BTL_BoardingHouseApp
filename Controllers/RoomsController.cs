using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoardingHouseApp.Data;
using BoardingHouseApp.Models;

namespace BoardingHouseApp.Controllers
{
    public class RoomsController : Controller
    {
        private readonly AppDbContext _db;
        public RoomsController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var rooms = await _db.Rooms.AsNoTracking().ToListAsync();
            return View(rooms);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            if (!ModelState.IsValid) return View(room);
            _db.Add(room);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
