﻿using DispatcherWeb.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class InvoiceBatchConfiguration : IEntityTypeConfiguration<InvoiceBatch>
    {
        public void Configure(EntityTypeBuilder<InvoiceBatch> builder)
        {
            builder
                .HasMany(e => e.Invoices)
                .WithOne(e => e.Batch)
                .HasForeignKey(e => e.BatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
