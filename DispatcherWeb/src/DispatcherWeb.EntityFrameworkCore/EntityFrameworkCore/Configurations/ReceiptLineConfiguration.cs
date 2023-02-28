using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class ReceiptLineConfiguration : IEntityTypeConfiguration<ReceiptLine>
    {
        public void Configure(EntityTypeBuilder<ReceiptLine> builder)
        {
            builder
                .Property(e => e.MaterialQuantity)
                .HasColumnType("decimal(18, 4)");

            builder
                .Property(e => e.FreightQuantity)
                .HasColumnType("decimal(18, 4)");

            builder
                .Property(e => e.MaterialRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.FreightRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.MaterialAmount)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.FreightAmount)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .HasOne(e => e.Service)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.MaterialUom)
                .WithMany()
                .HasForeignKey(e => e.MaterialUomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.FreightUom)
                .WithMany()
                .HasForeignKey(e => e.FreightUomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.LoadAt)
                .WithMany()
                .HasForeignKey(e => e.LoadAtId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.DeliverTo)
                .WithMany()
                .HasForeignKey(e => e.DeliverToId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.OrderLine)
                .WithMany(e => e.ReceiptLines)
                .IsRequired(false)
                .HasForeignKey(e => e.OrderLineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
