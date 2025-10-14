using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BoardingHouseApp.Models;

namespace BoardingHouseApp.Models
{
    public class Contract
    {
        [Key]
        public int ContractId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public double Deposit { get; set; }

        // FK đến Tenant
        public int TenantId { get; set; }

        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }

        // Quan hệ 1-n với Payment
        public ICollection<Payment>? Payments { get; set; }
    }
}
