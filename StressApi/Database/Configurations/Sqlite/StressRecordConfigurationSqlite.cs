using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StressApi.Database.Constants;
using StressData.Model;

namespace StressApi.Database.Configurations.Sqlite
{
    public class StressRecordConfigurationSqlite : StressRecordConfiguration
    {
        public override void Configure(EntityTypeBuilder<StressRecord> builder)
        {
            base.Configure(builder);

            builder.Property(s => s.Location).HasSrid(GeometryConstants.SRID);
        }
    }
}
