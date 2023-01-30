using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class EmployeeTimeClassificationConfiguration : IEntityTypeConfiguration<EmployeeTimeClassification>
    {
        public void Configure(EntityTypeBuilder<EmployeeTimeClassification> builder)
        {
            builder
                .HasOne(e => e.Driver)
                .WithMany(e => e.EmployeeTimeClassifications)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.TimeClassification)
                .WithMany(e => e.EmployeeTimeClassifications)
                .HasForeignKey(e => e.TimeClassificationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
