using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardingHouseApp.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public string? PaymentMethod { get; set; }  // tiền mặt, chuyển khoản...
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [Required]
        public double Amount { get; set; }

        [ForeignKey("ContractId")]
        public int ContractId { get; set; }
        public Contracts? Contract { get; set; }
    }
}
