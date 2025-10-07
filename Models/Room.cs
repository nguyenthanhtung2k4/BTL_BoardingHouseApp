using System.ComponentModel.DataAnnotations;

namespace BoardingHouseApp.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string RoomNo { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal RentPrice { get; set; }

        [Range(0, 1000)]
        public double Area { get; set; }
    }
}
