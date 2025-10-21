using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardingHouseApp.Models;

public class Contracts
{
    [Key]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } // Đổi từ Status -> IsActive
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }


    // Trạng thái  true là xóa hẳn 
    public bool IsDeleted { get; set; } = false;

    public int TenantId { get; set; }
    [ForeignKey(nameof(TenantId))]
    public Tenant Tenant { get; set; } = null!;

    public int RoomId { get; set; }
    [ForeignKey(nameof(RoomId))]
    public Room Room { get; set; } = null!;

    public ICollection<Payment>? Payments { get; set; }
}