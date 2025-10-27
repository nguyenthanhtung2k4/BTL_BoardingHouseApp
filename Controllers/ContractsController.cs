using System.Linq;
using System.Threading.Tasks;
using BoardingHouseApp.Data;
using BoardingHouseApp.Models;
using BoardingHouseApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BoardingHouseApp.Controllers
{
    [Authorize]
    public class ContractsController : Controller
    {
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

        private void PopulateDropdowns(int? selectedRoomId = null, int? selectedTenantId = null)
        {
            // Room: Khóa chính là RoomId, Hiển thị là RoomNumber (Đã đúng)
            ViewData["RoomId"] = new SelectList(
                _context.Rooms.OrderBy(r => r.RoomNumber),
                "RoomId",
                "RoomNumber",
                selectedRoomId
            );

            // SỬA LỖI: Đổi thuộc tính giá trị từ "Id" sang "TenantId"
            ViewData["TenantId"] = new SelectList(
                _context.Tenants.OrderBy(t => t.FullName),
                "TenantId", 
                "FullName",
                selectedTenantId
            );
        }

        // GET: Contracts/Create
        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new ContractCreationViewModel());
        }



        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractCreationViewModel model)
        {
            bool businessLogicError = false;

            // --- KIỂM TRA NGOẠI LỆ NGHIỆP VỤ ---

            // 1. EndDate phải sau StartDate
            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "Ngày Kết Thúc phải sau Ngày Bắt Đầu.");
                businessLogicError = true;
            }

            // 2. Nếu có Ngày Thanh Toán thực tế, phải có Phương Thức Thanh Toán
            if (model.InitialPaymentDate.HasValue && string.IsNullOrWhiteSpace(model.InitialPaymentMethod))
            {
                ModelState.AddModelError(nameof(model.InitialPaymentMethod), "Nếu bạn nhập Ngày Thanh Toán, Phương Thức Thanh Toán là bắt buộc.");
                businessLogicError = true;
            }

            // 3. Phòng không được có hợp đồng hoạt động trùng lặp
            var roomCurrentlyOccupied = await _context.Contracts
                .AnyAsync(c => c.RoomId == model.RoomId && c.IsActive && c.EndDate >= model.StartDate && !c.IsDeleted);

            if (roomCurrentlyOccupied)
            {
                ModelState.AddModelError(nameof(model.RoomId), "Phòng này hiện đang có hợp đồng khác có hiệu lực trùng với khoảng thời gian này.");
                businessLogicError = true;
            }


            if (ModelState.IsValid && !businessLogicError)
            {
                // Bắt đầu Transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Tạo và Lưu Hợp đồng (Contracts)
                    var contract = new Contracts
                    {
                        RoomId = model.RoomId,
                        TenantId = model.TenantId,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        IsActive = model.IsActive,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsDeleted = false
                    };
                    _context.Contracts.Add(contract);
                    await _context.SaveChangesAsync();

                    // 2. Tạo và Lưu Thanh toán/Hóa đơn Ban đầu (Payment)
                    var payment = new Payment
                    {
                        ContractId = contract.Id,
                        Amount = model.InitialAmount,
                        Description = model.InitialDescription,

                        // Xác định trạng thái và ngày/phương thức thanh toán
                        Status = model.InitialPaymentDate.HasValue ? 1 : 0, // 1=Đã TT, 0=Chưa TT
                        PaymentDate = model.InitialPaymentDate,
                        PaymentMethod = model.InitialPaymentDate.HasValue ? model.InitialPaymentMethod : "Hóa đơn/Chưa thanh toán",

                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();

                    // 3. Commit Transaction nếu mọi thứ thành công
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu thất bại
                    await transaction.RollbackAsync();

                    // Ghi log lỗi vào hệ thống (thực tế)
                    // _logger.LogError(ex, "Lỗi khi tạo Hợp đồng và Thanh toán.");

                    ModelState.AddModelError(string.Empty, "Lỗi hệ thống khi lưu dữ liệu. Vui lòng kiểm tra lại thông tin và thử lại.");
                }
            }

            // Nếu model không hợp lệ hoặc có lỗi, phải nạp lại ViewData (khắc phục lỗi NullReferenceException)
            PopulateDropdowns(model.RoomId, model.TenantId);
            return View(model);
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
            if (contract.Id == 0)
            {
                contract.Id = id;
            }

            // 1. Loại bỏ các trường tự động quản lý để tránh lỗi validation không cần thiết
            ModelState.Remove("UpdatedAt");
            ModelState.Remove("Tenant");
            ModelState.Remove("Room");
            ModelState.Remove("Payments");
            ModelState.Remove("IsDeleted");

            // Lưu ý: Chúng ta giữ lại "CreatedAt" từ Bind để không bị mất giá trị gốc

            if (ModelState.IsValid)
            {
                try
                {
                    // 2. Cập nhật trường UpdateAt
                    contract.UpdatedAt = DateTime.Now;
                    

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
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Room)    
                .Include(c => c.Tenant) 
                .Include(c => c.Payments.OrderByDescending(p => p.CreatedAt)) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
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


        // POST: /Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var contract = await _context.Contracts.FindAsync(id);

                if (contract != null)
                {
                    contract.IsDeleted = true; 
                    contract.UpdatedAt = DateTime.UtcNow; 

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
