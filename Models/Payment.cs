using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardingHouseApp.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public string Method { get; set; }  // tiền mặt, chuyển khoản...

        // FK đến Contract
        public int ContractId { get; set; }

        [ForeignKey("ContractId")]
        public Contract Contract { get; set; }
    }
}
