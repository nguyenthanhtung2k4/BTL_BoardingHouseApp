using Microsoft.EntityFrameworkCore;
using BoardingHouseApp.Models;

namespace BoardingHouseApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Room> Rooms { get; set; }
    }
}
