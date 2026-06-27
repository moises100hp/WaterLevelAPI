using Microsoft.EntityFrameworkCore;
using WaterLevelAPI.Model;

namespace WaterLevelAPI.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<WatterLevel> WaterLevels => Set<WatterLevel>();
    }
}
