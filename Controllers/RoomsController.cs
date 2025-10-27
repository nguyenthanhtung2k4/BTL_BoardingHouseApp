using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoardingHouseApp.Data;
using BoardingHouseApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace BoardingHouseApp.Controllers
{
    [Authorize]
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
                .Where(r => !r.IsDeleted) // Chỉ lấy phòng chưa bị xoá
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
        public async Task<IActionResult> Create([Bind("RoomNumber,Price,Status")] Room room)
        {
            // Kiểm tra số phòng đã tồn tại chưa
            bool roomNumberExists = await _context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber);
            if (roomNumberExists)
            {
                TempData["ErrorMessage"] = "Số phòng này đã tồn tại.";
                return View(room);
            }

            if (ModelState.IsValid)
            {
                room.CreatedAt = DateTime.Now;
                room.UpdatedAt = DateTime.Now;
                _context.Add(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm phòng thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm phòng. Vui lòng kiểm tra lại thông tin.";
            return View(room);
        }

        // GET: /Rooms/Edit/5
        // GET: /Rooms/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: /Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,RoomNumber,Price,Status")] Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy room hiện tại từ database
                    var existingRoom = await _context.Rooms.FindAsync(id);
                    if (existingRoom == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính
                    existingRoom.RoomNumber = room.RoomNumber;
                    existingRoom.Price = room.Price;
                    existingRoom.Status = room.Status;
                    existingRoom.UpdatedAt = DateTime.Now;

                    // Cập nhật trong context
                    _context.Update(existingRoom);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.RoomId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(room);
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
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
            if (room == null)
            {
                return NotFound();
            }

            // Thay vì xoá, đánh dấu là đã xoá
            room.IsDeleted = true;
            room.UpdatedAt = DateTime.Now;

            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xoá phòng thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
