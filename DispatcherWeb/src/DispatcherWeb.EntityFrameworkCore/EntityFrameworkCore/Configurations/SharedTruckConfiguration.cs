using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class SharedTruckConfiguration : IEntityTypeConfiguration<SharedTruck>
    {
        public void Configure(EntityTypeBuilder<SharedTruck> builder)
        {
            builder
                .HasOne(e => e.Office)
                .WithMany()
                .HasForeignKey(e => e.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Truck)
                .WithMany(e => e.SharedTrucks)
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
