using DispatcherWeb.TruckPositions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TruckPositionConfiguration : IEntityTypeConfiguration<TruckPosition>
    {
        public void Configure(EntityTypeBuilder<TruckPosition> builder)
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
