using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StressData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressData.Database.Configurations
{
    class StressRecordConfigurationSqlServer : StressRecordConfiguration
    {
        public override void Configure(EntityTypeBuilder<StressRecord> builder)
        {
            base.Configure(builder);

            builder.Property(s => s.Location)
                .HasColumnType("POINTZ");
        }
    }
}
