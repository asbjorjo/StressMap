﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StressData.Database.Constants;
using StressData.Model;

namespace StressData.Database.Configurations
{
    public class StressRecordConfiguration : IEntityTypeConfiguration<StressRecord>
    {
        public virtual void Configure(EntityTypeBuilder<StressRecord> builder)
        {
            builder.ToTable(TableName.StressRecord);

            builder.HasKey(s => s.Id);

            builder.HasAlternateKey(s => s.WsmId);

            builder.Property(s => s.Location)
                .IsRequired();
            builder.Property(s => s.Quality)
                .IsRequired()
                .HasConversion<string>();
            builder.Property(s => s.Regime)
                            .IsRequired()
                            .HasConversion<string>();
            builder.Property(s => s.Type)
                            .IsRequired()
                            .HasConversion<string>();

            builder.HasIndex(s => s.Quality);
            builder.HasIndex(s => s.Regime);
            builder.HasIndex(s => s.Type);
        }
    }
}
