using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StressData.Database.Constants;
using StressData.Model;

namespace StressData.Database.Configurations
{
    class StressPlateConfigurationPostgreSQL : StressPlateConfiguration
    {
        public override void Configure(EntityTypeBuilder<StressPlate> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.Outline)
                .HasColumnType("geometry (polygon, " + GeometryConstants.SRID + ")");
        }
    }
}
