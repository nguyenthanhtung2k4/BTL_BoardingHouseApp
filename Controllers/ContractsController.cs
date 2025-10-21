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
    public class ContractsController : Controller
    {
        // GIẢ ĐỊNH: Thay thế bằng DbContext thực tế của bạn
        private readonly AppDbContext _context;

        public ContractsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        // GET: /Contracts
        public async Task<IActionResult> Index()
        {
            var contracts = _context.Contracts
                                    .Where(x => !x.IsDeleted)
                                    .Include(c => c.Tenant) 
                                    .Include(c => c.Room)   
                                    .OrderByDescending(c => c.CreatedAt);

            return View(await contracts.ToListAsync());
        }

        [HttpGet]
        // GET: /Contracts/Create
        public IActionResult Create()
        {
            try
            {
                if (!_context.Tenants.Any() || !_context.Rooms.Any() )
                {
                    // Cảnh báo nếu có dữ liệu bị bỏ qua hoặc danh sách rỗng
                    TempData["WarningMessage"] = "Cảnh báo: Không có người thuê nào trong hệ thống hoặc một số người thuê bị thiếu Tên/ID.";
                }

                ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName");
                ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomNumber");

                return View();

            }catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi nạp dữ liệu: {ex.Message}. Vui lòng kiểm tra kết nối CSDL và Model.";
                ViewData["TenantId"] = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
                ViewData["RoomId"] = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
                return View(new Contracts()); // Trả về đối tượng trống
            }
        }

        // POST: /Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IsActive,StartDate,EndDate,TenantId,RoomId")] Contracts contract)
        {
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");
            ModelState.Remove("Tenant");
            ModelState.Remove("Room");
            ModelState.Remove("Payments");

            if (ModelState.IsValid)
            {
                try
                {
                    contract.UpdatedAt = null;

                    _context.Add(contract);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thêm hợp đồng mới thành công! 🎉";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    // Lỗi DB (ví dụ: Constraint Violation, ID không tồn tại)
                    TempData["ErrorMessage"] = $"Lỗi CSDL: Không thể lưu hợp đồng. Vui lòng kiểm tra ID người thuê/phòng. Chi tiết: {dbEx.InnerException?.Message ?? dbEx.Message}";
                }
                catch (Exception ex)
                {
                    // Lỗi chung
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi không xác định khi tạo hợp đồng: {ex.Message}";
                }
            }

            // --- 2. Xử lý khi ModelState.IsValid
            var validTenants = _context.Tenants
                .AsNoTracking()
                .Where(t => t.TenantId > 0 && !string.IsNullOrEmpty(t.FullName))
                .Select(t => new { t.TenantId, t.FullName })
                .ToList();
            
            ViewData["TenantId"] = new SelectList(validTenants, "TenantId", "FullName", contract.TenantId);

            var validRooms = _context.Rooms
                .AsNoTracking()
                .Where(r => r.RoomId > 0 && !string.IsNullOrEmpty(r.RoomNumber))
                .Select(r => new { r.RoomId, r.RoomNumber })
                .ToList();
            ViewData["RoomId"] = new SelectList(validRooms, "RoomId", "RoomNumber", contract.RoomId);

            return View(contract);

        }


        // GET: /Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // 1. Kiểm tra ID
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID hợp đồng.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // 2. Tìm kiếm Hợp đồng
                var contract = await _context.Contracts.FindAsync(id);
                if (contract == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy hợp đồng yêu cầu.";
                    return RedirectToAction(nameof(Index));
                }

                // 3. Chuẩn bị SelectList (Tương tự như Create, nhưng chọn giá trị hiện tại)

                // Danh sách người thuê
                var validTenants = await _context.Tenants
                    .AsNoTracking()
                    .Where(t => t.TenantId > 0 && !string.IsNullOrEmpty(t.FullName))
                    .Select(t => new { t.TenantId, t.FullName })
                    .ToListAsync();

                // Truyền contract.TenantId để đặt giá trị đã chọn
                ViewData["TenantId"] = new SelectList(validTenants, "TenantId", "FullName", contract.TenantId);

                // Danh sách phòng
                var validRooms = await _context.Rooms
                    .AsNoTracking()
                    .Where(r => r.RoomId > 0 && !string.IsNullOrEmpty(r.RoomNumber))
                    .Select(r => new { r.RoomId, r.RoomNumber })
                    .ToListAsync();

                // Truyền contract.RoomId để đặt giá trị đã chọn
                ViewData["RoomId"] = new SelectList(validRooms, "RoomId", "RoomNumber", contract.RoomId);

                return View(contract);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu chỉnh sửa: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


        // POST: /Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContractsId,IsActive,StartDate,EndDate,TenantId,RoomId,CreatedAt")] Contracts contract)
        {
            // Kiểm tra ID trong URL có khớp với ID trong Model không
            if (id != contract.Id)
            {
                TempData["ErrorMessage"] = "ID hợp đồng không khớp.";
                return RedirectToAction(nameof(Index));
            }

            // 1. Loại bỏ các trường tự động quản lý để tránh lỗi validation không cần thiết
            ModelState.Remove("UpdatedAt");
            ModelState.Remove("Tenant");
            ModelState.Remove("Room");
            ModelState.Remove("Payments");

            // Lưu ý: Chúng ta giữ lại "CreatedAt" từ Bind để không bị mất giá trị gốc

            if (ModelState.IsValid)
            {
                try
                {
                    // 2. Cập nhật trường UpdateAt
                    contract.UpdatedAt = DateTime.UtcNow;

                    // 3. Cập nhật vào DB
                    _context.Update(contract);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật hợp đồng thành công! 🎉";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Xử lý lỗi đồng thời (Concurrency Exception)
                    if (!_context.Contracts.Any(e => e.Id == contract.Id))
                    {
                        TempData["ErrorMessage"] = "Hợp đồng này đã bị xóa bởi người dùng khác.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        // Lỗi đồng thời khác
                        throw;
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    TempData["ErrorMessage"] = $"Lỗi CSDL: Kiểm tra ID người thuê/phòng có hợp lệ không. Chi tiết: {dbEx.InnerException?.Message ?? dbEx.Message}";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi không xác định khi cập nhật hợp đồng: {ex.Message}";
                }
            }

            // 4. Nếu ModelState không hợp lệ hoặc xảy ra lỗi lưu DB: Tái tạo SelectList

            // Danh sách người thuê
            var validTenants = await _context.Tenants
                .AsNoTracking()
                .Where(t => t.TenantId > 0 && !string.IsNullOrEmpty(t.FullName))
                .Select(t => new { t.TenantId, t.FullName })
                .ToListAsync();
            ViewData["TenantId"] = new SelectList(validTenants, "TenantId", "FullName", contract.TenantId);

            // Danh sách phòng
            var validRooms = await _context.Rooms
                .AsNoTracking()
                .Where(r => r.RoomId > 0 && !string.IsNullOrEmpty(r.RoomNumber))
                .Select(r => new { r.RoomId, r.RoomNumber })
                .ToListAsync();
            ViewData["RoomId"] = new SelectList(validRooms, "RoomId", "RoomNumber", contract.RoomId);

            return View(contract); // Trả về View với dữ liệu lỗi
        }


        // GET: /Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID hợp đồng.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Sử dụng .Include() để tải thông tin Tenant và Room cùng lúc
                var contract = await _context.Contracts
                    .Include(c => c.Tenant)
                    .Include(c => c.Room)
                    // Lấy thêm Payments nếu bạn muốn hiển thị lịch sử thanh toán
                    .Include(c => c.Payments)
                    .FirstOrDefaultAsync(m => m.Id == id); // Dùng Id, không phải ContractsId

                if (contract == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy hợp đồng yêu cầu.";
                    return RedirectToAction(nameof(Index));
                }

                return View(contract);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải chi tiết hợp đồng: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Contracts/Delete/5 (Hiển thị trang xác nhận)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID hợp đồng.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Sử dụng .Include() để tải thông tin Tenant và Room để hiển thị chi tiết xác nhận
                var contract = await _context.Contracts
                    .Include(c => c.Tenant)
                    .Include(c => c.Room)
                    .FirstOrDefaultAsync(m => m.Id == id); // Dùng Id

                if (contract == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy hợp đồng yêu cầu.";
                    return RedirectToAction(nameof(Index));
                }

                return View(contract);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu xóa: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


        // POST: /Contracts/Delete/5 (Thực hiện Soft Delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var contract = await _context.Contracts.FindAsync(id);

                if (contract != null)
                {
                    contract.IsDeleted = true; // Đánh dấu là đã xóa
                    contract.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian thay đổi

                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Hợp đồng đã được ẩn (xóa mềm) thành công. 🗑️";
                }
                else
                {
                    TempData["ErrorMessage"] = "Hợp đồng không tồn tại.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi ẩn hợp đồng: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}