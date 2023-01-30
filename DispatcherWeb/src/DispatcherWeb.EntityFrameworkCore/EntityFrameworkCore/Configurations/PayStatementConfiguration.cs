using DispatcherWeb.PayStatements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class PayStatementConfiguration : IEntityTypeConfiguration<PayStatement>
    {
        public void Configure(EntityTypeBuilder<PayStatement> builder)
        {
            builder
                .HasMany(x => x.PayStatementDetails)
                .WithOne(x => x.PayStatement)
                .HasForeignKey(x => x.PayStatementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
