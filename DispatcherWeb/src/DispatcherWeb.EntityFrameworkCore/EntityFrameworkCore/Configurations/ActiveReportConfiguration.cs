﻿using DispatcherWeb.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class ActiveReportConfiguration : IEntityTypeConfiguration<ActiveReport>
    {
        public void Configure(EntityTypeBuilder<ActiveReport> builder)
        {
            builder.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(30);

            builder.Property(e => e.Path)
                    .IsRequired()
                    .HasMaxLength(30);

            builder.Property(e => e.Description)
                    .HasMaxLength(180);

            builder.HasIndex(e => e.CategoryId);

            builder.HasOne(e => e.Category)
                    .WithMany(e => e.Report)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
