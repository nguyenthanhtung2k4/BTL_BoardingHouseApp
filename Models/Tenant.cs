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

        [Required, StringLength(10, ErrorMessage = "Số điện thoại tối đa 10 ký tự.")]
    
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; } 

        [Required(ErrorMessage = "Vui lòng chọn phòng.")]
        public int RoomId { get; set; }

        [ForeignKey(nameof(RoomId))]
        public Room? Room { get; set; }

        public Contract? Contract { get; set; }
    }
}
