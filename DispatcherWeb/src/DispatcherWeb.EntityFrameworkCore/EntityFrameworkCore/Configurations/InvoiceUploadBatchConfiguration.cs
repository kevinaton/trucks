using DispatcherWeb.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class InvoiceUploadBatchConfiguration : IEntityTypeConfiguration<InvoiceUploadBatch>
    {
        public void Configure(EntityTypeBuilder<InvoiceUploadBatch> builder)
        {
            builder
                .HasMany(e => e.Invoices)
                .WithOne(e => e.UploadBatch)
                .HasForeignKey(e => e.UploadBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
