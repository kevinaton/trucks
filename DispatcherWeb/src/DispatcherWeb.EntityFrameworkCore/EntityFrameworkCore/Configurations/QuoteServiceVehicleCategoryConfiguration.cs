using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class QuoteServiceVehicleCategoryConfiguration : IEntityTypeConfiguration<QuoteServiceVehicleCategory>
    {
        public void Configure(EntityTypeBuilder<QuoteServiceVehicleCategory> builder)
        {
            builder
                .HasOne(e => e.QuoteService)
                .WithMany()
                .HasForeignKey(e => e.QuoteServiceId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder
                .HasOne(e => e.VehicleCategory)
                .WithMany()
                .HasForeignKey(e => e.VehicleCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
