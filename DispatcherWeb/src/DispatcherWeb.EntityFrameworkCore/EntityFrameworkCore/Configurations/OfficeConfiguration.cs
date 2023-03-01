﻿using DispatcherWeb.Offices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class OfficeConfiguration : IEntityTypeConfiguration<Office>
    {
        public void Configure(EntityTypeBuilder<Office> builder)
        {
            builder
                .HasMany(e => e.Trucks)
                .WithOne(e => e.Office)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.Users)
                .WithOne(e => e.Office)
                .HasForeignKey(e => e.OfficeId);
        }
    }
}
