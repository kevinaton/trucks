using DispatcherWeb.PayStatements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class PayStatementTicketConfiguration : IEntityTypeConfiguration<PayStatementTicket>
    {
        public void Configure(EntityTypeBuilder<PayStatementTicket> builder)
        {
            builder
                .HasOne(x => x.Ticket)
                .WithMany(x => x.PayStatementTickets)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.PayStatementDetail)
                .WithMany(x => x.PayStatementTickets)
                .HasForeignKey(x => x.PayStatementDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.TimeClassification)
                .WithMany(x => x.PayStatementTickets)
                .HasForeignKey(x => x.TimeClassificationId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
