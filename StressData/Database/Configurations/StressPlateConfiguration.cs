using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StressData.Database.Constants;
using StressData.Model;

namespace StressData.Database.Configurations
{
    public class StressPlateConfiguration : IEntityTypeConfiguration<StressPlate>
    {
        public virtual void Configure(EntityTypeBuilder<StressPlate> builder)
        {
            builder.ToTable(TableName.StressPlate);

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired();
            builder.Property(p => p.Outline)
                .IsRequired();

            builder.HasIndex(p => p.Name);
        }
    }
}
