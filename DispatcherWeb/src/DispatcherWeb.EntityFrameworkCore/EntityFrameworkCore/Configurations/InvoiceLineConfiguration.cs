using DispatcherWeb.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
    {
        public void Configure(EntityTypeBuilder<InvoiceLine> builder)
        {
            builder
                .HasOne(e => e.Carrier)
                .WithMany()
                .HasForeignKey(e => e.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Ticket)
                .WithOne(e => e.InvoiceLine)
                .HasForeignKey<InvoiceLine>(e => e.TicketId)
                .OnDelete(DeleteBehavior.Restrict);


            builder
                .Property(e => e.MaterialRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.FreightRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.Quantity)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.MaterialExtendedAmount)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.FreightExtendedAmount)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.Tax)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.Subtotal)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.ExtendedAmount)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
