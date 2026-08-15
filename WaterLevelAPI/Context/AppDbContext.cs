using Microsoft.EntityFrameworkCore;
using WaterLevelAPI.Model;

namespace WaterLevelAPI.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<WatterLevel> WaterLevels => Set<WatterLevel>();
        public DbSet<DeviceCommand> DeviceCommands => Set<DeviceCommand>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Um comando por dispositivo: garante o upsert em DeviceCommandService.
            modelBuilder.Entity<DeviceCommand>()
                .HasIndex(x => x.DeviceId)
                .IsUnique();
        }
    }
}
