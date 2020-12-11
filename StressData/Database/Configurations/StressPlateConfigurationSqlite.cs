using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StressData.Database.Constants;
using StressData.Model;

namespace StressData.Database.Configurations
{
    public class StressPlateConfigurationSqlite : StressPlateConfiguration
    {
        public override void Configure(EntityTypeBuilder<StressPlate> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.Outline)
                .HasSrid(GeometryConstants.SRID);
        }
    }
}
