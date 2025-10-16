using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BoardingHouseApp.Models;

namespace BoardingHouseApp.Models
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }
        public bool Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt{ get; set; }
        public DateTime updateAt{ get; set; }

        public int TenantId { get; set; }

        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }

        [ForeignKey("RoomId")]
        public Room Room { get; set; }

        // Quan hệ 1-n với Payment
        public ICollection<Payment>? Payments { get; set; }
    }
}
