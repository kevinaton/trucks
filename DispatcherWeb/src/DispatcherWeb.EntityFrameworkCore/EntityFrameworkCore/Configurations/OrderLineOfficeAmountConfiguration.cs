using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class OrderLineOfficeAmountConfiguration : IEntityTypeConfiguration<OrderLineOfficeAmount>
    {
        public void Configure(EntityTypeBuilder<OrderLineOfficeAmount> builder)
        {
            builder
                .Property(e => e.ActualQuantity)
                .HasColumnType("decimal(18, 2)");

            builder
                .HasOne(e => e.OrderLine)
                .WithMany(e => e.OfficeAmounts)
                .HasForeignKey(e => e.OrderLineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Office)
                .WithMany()
                .HasForeignKey(e => e.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
