using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BoardingHouseApp.Models;
public class Contracts
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int TenantId { get; set; }

    [Required]
    public int RoomId { get; set; }

   
    [Required]
    [Column(TypeName = "date")]
    public DateTime StartDate { get; set; }

    
    [Required]
    [Column(TypeName = "date")]
    public DateTime EndDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey("TenantId")]
    public Tenant Tenant { get; set; }

    [ForeignKey("RoomId")]
    public Room Room { get; set; }

    public ICollection<Payment> Payments { get; set; }
}