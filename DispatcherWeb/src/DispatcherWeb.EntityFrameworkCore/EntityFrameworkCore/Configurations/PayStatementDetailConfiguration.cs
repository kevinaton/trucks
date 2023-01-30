﻿using DispatcherWeb.PayStatements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class PayStatementDetailConfiguration : IEntityTypeConfiguration<PayStatementDetail>
    {
        public void Configure(EntityTypeBuilder<PayStatementDetail> builder)
        {
            builder
                .HasOne(x => x.PayStatement)
                .WithMany(x => x.PayStatementDetails)
                .HasForeignKey(x => x.PayStatementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Driver)
                .WithMany()
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.PayStatementTickets)
                .WithOne(x => x.PayStatementDetail)
                .HasForeignKey(x => x.PayStatementDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.PayStatementTimeRecords)
                .WithOne(x => x.PayStatementDetail)
                .HasForeignKey(x => x.PayStatementDetailId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
