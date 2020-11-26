using Microsoft.EntityFrameworkCore;
using StressData.Model;
using StressData.Database.Configurations;
using StressData.Database.Configurations.Sqlite;
using System;
using StressData.Database.Configurations.PostgreSQL;

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
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    modelBuilder.HasPostgresExtension("postgis");
                    modelBuilder.ApplyConfiguration(new StressRecordConfigurationPostgreSQL());
                    break;
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    modelBuilder.ApplyConfiguration(new StressRecordConfigurationSqlServer());
                    break;
                default:
                    throw new ArgumentException($"unknown provider {Database.ProviderName}");
            }
        }
    }
}
