using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StressData.Model;

namespace StressData.Database.Configurations
{
    class StressPlateConfigurationPostgreSQL : StressPlateConfiguration
    {
        public override void Configure(EntityTypeBuilder<StressPlate> builder)
        {
            base.Configure(builder);
        }
    }
}
