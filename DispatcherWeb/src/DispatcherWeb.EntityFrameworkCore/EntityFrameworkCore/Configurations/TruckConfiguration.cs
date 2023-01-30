using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TruckConfiguration : IEntityTypeConfiguration<Truck>
    {
        public void Configure(EntityTypeBuilder<Truck> builder)
        {
            builder
                .HasOne(e => e.DefaultDriver)
                .WithMany(e => e.DefaultTrucks)
                .HasForeignKey(e => e.DefaultDriverId)
                ;

            builder
                .HasOne(e => e.VehicleCategory)
                .WithMany()
                .HasForeignKey(e => e.VehicleCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
