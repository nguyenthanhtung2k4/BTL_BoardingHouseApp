using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardingHouseApp.Models
{
    public class Tenant
    {
        [Key]
        public int TenantId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        [EmailAddress]
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


