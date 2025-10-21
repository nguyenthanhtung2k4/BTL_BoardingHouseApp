using BoardingHouseApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BoardingHouseApp.Models.ViewModels
{
    public class ContractCreationViewModel
    {
        // === Thuộc tính của Contracts ===
        [Required(ErrorMessage = "Vui lòng chọn Phòng thuê.")]
        [Display(Name = "Phòng Thuê")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Người thuê.")]
        [Display(Name = "Người Thuê")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Ngày Bắt Đầu.")]
        [Display(Name = "Ngày Bắt Đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Ngày Kết Thúc.")]
        [Display(Name = "Ngày Kết Thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Có Hiệu Lực")]
        public bool IsActive { get; set; } = true;

        // === Thuộc tính của Initial Payment (Thanh toán/Hóa đơn ban đầu) ===
        [Required(ErrorMessage = "Vui lòng nhập Số Tiền Thanh Toán.")]
        [Display(Name = "Số Tiền Thanh Toán Ban Đầu (Tiền cọc/Tháng 1)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0.")]
        public double InitialAmount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mô Tả Thanh Toán.")]
        [Display(Name = "Mô Tả Thanh Toán")]
        public string? InitialDescription { get; set; } // Ví dụ: Tiền cọc & Tiền nhà tháng 1

        [Display(Name = "Ngày Thanh Toán Thực Tế (Nếu Đã Trả)")]
        [DataType(DataType.Date)]
        public DateTime? InitialPaymentDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Phương Thức Thanh Toán.")]
        [Display(Name = "Phương Thức Thanh Toán")]
        public string? InitialPaymentMethod { get; set; }
    }
}