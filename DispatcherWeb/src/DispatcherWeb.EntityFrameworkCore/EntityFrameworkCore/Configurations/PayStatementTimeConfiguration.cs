﻿using DispatcherWeb.PayStatements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class PayStatementTimeConfiguration : IEntityTypeConfiguration<PayStatementTime>
    {
        public void Configure(EntityTypeBuilder<PayStatementTime> builder)
        {
            builder
                .HasMany(x => x.EmployeeTimeRecords)
                .WithOne(x => x.PayStatementTime)
                .HasForeignKey(x => x.PayStatementTimeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.PayStatementDetail)
                .WithMany(x => x.PayStatementTimeRecords)
                .HasForeignKey(x => x.PayStatementDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.TimeClassification)
                .WithMany(x => x.PayStatementTimeRecords)
                .HasForeignKey(x => x.TimeClassificationId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
