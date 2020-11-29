using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StressData.Model;

namespace StressData.Database.Configurations
{
    class StressRecordConfigurationSqlServer : StressRecordConfiguration
    {
        public override void Configure(EntityTypeBuilder<StressRecord> builder)
        {
            base.Configure(builder);
        }
    }
}
