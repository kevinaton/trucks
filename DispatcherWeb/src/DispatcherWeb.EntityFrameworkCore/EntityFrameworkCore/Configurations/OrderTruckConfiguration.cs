using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class OrderTruckConfiguration : IEntityTypeConfiguration<OrderTruck>
    {
        public void Configure(EntityTypeBuilder<OrderTruck> builder)
        {
            builder
                .HasMany(e => e.DependentOrderTrucks)
                .WithOne(e => e.ParentOrderTruck)
                .HasForeignKey(e => e.ParentOrderTruckId);
        }
    }
}
