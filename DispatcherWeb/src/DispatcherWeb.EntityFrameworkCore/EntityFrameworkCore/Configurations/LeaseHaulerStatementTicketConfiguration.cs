using DispatcherWeb.LeaseHaulerStatements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class LeaseHaulerStatementTicketConfiguration : IEntityTypeConfiguration<LeaseHaulerStatementTicket>
    {
        public void Configure(EntityTypeBuilder<LeaseHaulerStatementTicket> builder)
        {
            builder
                .HasIndex(x => x.TicketId)
                .IsUnique();

            builder
                .HasOne(x => x.Ticket)
                .WithOne(x => x.LeaseHaulerStatementTicket)
                .HasForeignKey<LeaseHaulerStatementTicket>(x => x.TicketId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
