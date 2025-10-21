using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardingHouseApp.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Mô Tả")]
        public string? Description { get; set; } // Ví dụ: Tiền cọc, Tiền nhà tháng 1...

        [Display(Name = "Ngày Thanh Toán Thực Tế")]
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; } 

        [Required]
        [Display(Name = "Phương Thức")]
        public string? PaymentMethod { get; set; } 

        [Required]
        [Display(Name = "Trạng Thái")]
        public int Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Số Tiền")]
        public double Amount { get; set; }

        [Required]
        [Display(Name = "Hợp Đồng")]
        [ForeignKey("ContractId")]
        public int ContractId { get; set; }
        public Contracts? Contract { get; set; }
    }
}