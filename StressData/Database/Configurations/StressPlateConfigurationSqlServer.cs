using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StressData.Model;

namespace StressData.Database.Configurations
{
    public class StressPlateConfigurationSqlServer : StressPlateConfiguration
    {
        public override void Configure(EntityTypeBuilder<StressPlate> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.Outline)
                .HasColumnType("polygon");
        }
    }
}
