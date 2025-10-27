using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BoardingHouseApp.Models
{
    [Index(nameof(RoomNumber), IsUnique = true)]
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required]
        public string RoomNumber { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public RoomStatus Status { get; set; } = RoomStatus.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false; // Thêm giá trị mặc định
    }

    public enum RoomStatus
    {
        [Display(Name = "Trống")]
        Empty = 0,
        
        [Display(Name = "Đang thuê")]
        Occupied = 1,
        
        [Display(Name = "Bảo trì")]
        Maintenance = 2
    }
}