﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StressData.Database.Constants;
using StressData.Model;

namespace StressData.Database.Configurations.PostgreSQL
{
    class StressRecordConfigurationPostgreSQL : StressRecordConfiguration
    {
        public override void Configure(EntityTypeBuilder<StressRecord> builder)
        {
            base.Configure(builder);

            builder.Property(s => s.Location).HasColumnType("geometry (pointz, " + GeometryConstants.SRID + ")");
        
            builder.HasIndex(s => s.Location);
        }
    }
}
