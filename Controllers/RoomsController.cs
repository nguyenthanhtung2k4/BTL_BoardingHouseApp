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
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
            return View(rooms);
        }

        // GET: Room/Create
        [HttpGet]
        [Authorize(Roles = "Admin")] // THÊM AUTHORIZE
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("RoomNumber,Price,Status")] Room room)
        {
            // KIỂM TRA SỐ PHÒNG ĐÃ TỒN TẠI (KHÔNG BAO GỒM ĐÃ XÓA)
            bool roomNumberExists = await _context.Rooms
                .AnyAsync(r => r.RoomNumber == room.RoomNumber && !r.IsDeleted);
            
            if (roomNumberExists)
            {
                TempData["ErrorMessage"] = "Số phòng này đã tồn tại. Vui lòng chọn số phòng khác.";
                return View(room);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    room.CreatedAt = DateTime.Now;
                    room.UpdatedAt = DateTime.Now;
                    _context.Add(room);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Thêm phòng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // BẮT LỖI UNIQUE CONSTRAINT TỪ DATABASE
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Rooms_RoomNumber"))
                    {
                        TempData["ErrorMessage"] = "Số phòng này đã tồn tại trong hệ thống.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm phòng. Vui lòng thử lại.";
                    }
                    return View(room);
                }
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.";
            return View(room);
        }

        // GET: /Rooms/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null || room.IsDeleted)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: /Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,RoomNumber,Price,Status")] Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            // KIỂM TRA SỐ PHÒNG ĐÃ TỒN TẠI (TRỪ PHÒNG HIỆN TẠI)
            bool roomNumberExists = await _context.Rooms
                .AnyAsync(r => r.RoomNumber == room.RoomNumber && 
                              r.RoomId != id && 
                              !r.IsDeleted);
            
            if (roomNumberExists)
            {
                TempData["ErrorMessage"] = "Số phòng này đã tồn tại. Vui lòng chọn số phòng khác.";
                return View(room);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRoom = await _context.Rooms.FindAsync(id);
                    if (existingRoom == null || existingRoom.IsDeleted)
                    {
                        return NotFound();
                    }

                    existingRoom.RoomNumber = room.RoomNumber;
                    existingRoom.Price = room.Price;
                    existingRoom.Status = room.Status;
                    existingRoom.UpdatedAt = DateTime.Now;

                    _context.Update(existingRoom);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật phòng thành công!";
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
                catch (DbUpdateException ex)
                {
                    // BẮT LỖI UNIQUE CONSTRAINT TỪ DATABASE
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Rooms_RoomNumber"))
                    {
                        TempData["ErrorMessage"] = "Số phòng này đã tồn tại trong hệ thống.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật phòng. Vui lòng thử lại.";
                    }
                    return View(room);
                }
            }
            
            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.";
            return View(room);
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id && !e.IsDeleted);
        }

        // GET: /Rooms/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == id && !r.IsDeleted);
                
            if (room == null) return NotFound();
            return View(room);
        }

        // POST: /Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null || room.IsDeleted)
            {
                return NotFound();
            }

            // Kiểm tra xem phòng có đang được thuê không
            bool hasActiveContract = await _context.Contracts
                .AnyAsync(c => c.RoomId == id && c.IsActive);
                
            if (hasActiveContract)
            {
                TempData["ErrorMessage"] = "Không thể xóa phòng đang có hợp đồng thuê hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            room.IsDeleted = true;
            room.UpdatedAt = DateTime.Now;

            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xoá phòng thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}