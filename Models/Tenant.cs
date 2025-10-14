using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BoardingHouseApp.Models;
using System.Diagnostics.Contracts;

namespace BoardingHouseApp.Models

{
    public class Tenant
    {
        [Key]
        public int TenantId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, Phone]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        // FK đến Room
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public Room Room { get; set; }

        // Quan hệ 1-1 với Contract
        public Contract? Contract { get; set; }
    }
}
