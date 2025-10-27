using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BoardingHouseApp.Models
{
    [Index(nameof(Phone), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class Tenant
    {
        [Key]
        public int TenantId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? hashPassword { get; set; }

        [EmailAddress]
        public required string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool isDeleted { get; set; } = false;
    }
}


