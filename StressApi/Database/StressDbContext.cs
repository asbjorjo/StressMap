using Microsoft.EntityFrameworkCore;
using StressData.Model;
using StressApi.Database.Configurations;

namespace StressApi.Database
{
    public class StressDbContext : DbContext
    {
        public StressDbContext(DbContextOptions<StressDbContext> options) : base(options)
        {
        }

        public DbSet<StressRecord> StressRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new StressRecordConfiguration());
        }
    }
}
