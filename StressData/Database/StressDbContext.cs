using Microsoft.EntityFrameworkCore;
using StressData.Model;
using StressData.Database.Configurations;
using StressData.Database.Configurations.Sqlite;

namespace StressData.Database
{
    public class StressDbContext : DbContext
    {
        public StressDbContext(DbContextOptions<StressDbContext> options) : base(options)
        {
        }

        public DbSet<StressRecord> StressRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            switch (Database.ProviderName)
            {
                case "Microsoft.EntityFrameworkCore.Sqlite":
                    modelBuilder.ApplyConfiguration(new StressRecordConfigurationSqlite());
                    break;
                default:
                    modelBuilder.ApplyConfiguration(new StressRecordConfiguration());
                    break;
            }
        }
    }
}
