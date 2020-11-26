using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StressData.Database.Constants;
using StressData.Model;

namespace StressData.Database.Configurations.Sqlite
{
    public class StressRecordConfigurationSqlite : StressRecordConfiguration
    {
        public override void Configure(EntityTypeBuilder<StressRecord> builder)
        {
            base.Configure(builder);

            builder.Property(s => s.Location)
                .HasColumnType("POINTZ")
                .HasSrid(GeometryConstants.SRID);
        }
    }
}
