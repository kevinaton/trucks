using DispatcherWeb.Dispatching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class DispatchConfiguration : IEntityTypeConfiguration<Dispatch>
    {
        public void Configure(EntityTypeBuilder<Dispatch> builder)
        {
            builder
                .HasIndex(x => x.Status);

            builder
                .HasOne(x => x.OrderLineTruck)
                .WithMany(x => x.Dispatches)
                .HasForeignKey(x => x.OrderLineTruckId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
