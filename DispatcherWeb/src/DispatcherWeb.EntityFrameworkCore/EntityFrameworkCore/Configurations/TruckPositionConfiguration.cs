using System;
using DispatcherWeb.TruckPositions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    [Obsolete]
    public class TruckPositionObsoleteConfiguration : IEntityTypeConfiguration<TruckPositionObsolete>
    {
        public void Configure(EntityTypeBuilder<TruckPositionObsolete> builder)
        {
            builder
                .Property(e => e.Latitude)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimalLocation);

            builder
                .Property(e => e.Longitude)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimalLocation);
        }
    }
}
