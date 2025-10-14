using System.ComponentModel.DataAnnotations;

namespace BoardingHouseApp.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required]
        public string RoomNumber { get; set; }

        [Required]
        public double Price { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Quan hệ 1-n với Tenant
        public ICollection<Tenant>? Tenants { get; set; }
    }
}
