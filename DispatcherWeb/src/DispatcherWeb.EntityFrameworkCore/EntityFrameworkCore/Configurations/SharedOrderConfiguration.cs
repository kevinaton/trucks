using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class SharedOrderConfiguration : IEntityTypeConfiguration<SharedOrder>
    {
        public void Configure(EntityTypeBuilder<SharedOrder> builder)
        {
            builder
                .HasOne(e => e.Office)
                .WithMany(e => e.SharedOrders)
                .HasForeignKey(e => e.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Order)
                .WithMany(e => e.SharedOrders)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
