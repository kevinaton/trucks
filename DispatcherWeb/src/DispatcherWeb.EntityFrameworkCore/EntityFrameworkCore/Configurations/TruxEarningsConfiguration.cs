using DispatcherWeb.Trux;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TruxEarningsConfiguration : IEntityTypeConfiguration<TruxEarnings>
    {
        public void Configure(EntityTypeBuilder<TruxEarnings> builder)
        {
            builder
                .HasOne(x => x.Batch)
                .WithMany(x => x.TruxEarnings)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
