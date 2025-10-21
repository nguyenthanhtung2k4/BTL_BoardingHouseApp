using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardingHouseApp.Models;

public class Contracts
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    [Display(Name = "Hợp đồng có hiệu lực")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Display(Name = "Ngày Bắt Đầu")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [Display(Name = "Ngày Kết Thúc")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    // Trạng thái xóa mềm (Soft Delete)
    public bool IsDeleted { get; set; } = false;

    // --- Liên kết Khóa ngoại ---
    [Required]
    [Display(Name = "Người Thuê")]
    public int TenantId { get; set; }
    [ForeignKey(nameof(TenantId))]
    public Tenant Tenant { get; set; } = null!; 

    [Required]
    [Display(Name = "Phòng")]
    public int RoomId { get; set; }
    [ForeignKey(nameof(RoomId))]
    public Room Room { get; set; } = null!;
//Navigate 
    public ICollection<Payment>? Payments { get; set; } = new List<Payment>();
}