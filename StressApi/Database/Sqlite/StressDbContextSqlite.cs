using Microsoft.EntityFrameworkCore;

namespace StressApi.Database.Sqlite
{
    public class StressDbContextSqlite : StressDbContext
    {
        public StressDbContextSqlite(DbContextOptions<StressDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
