using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TrailerAssignmentConfiguration : IEntityTypeConfiguration<TrailerAssignment>
    {
        public void Configure(EntityTypeBuilder<TrailerAssignment> builder)
        {
            builder
                .HasOne(e => e.Tractor)
                .WithMany(e => e.TrailerAssignmentsOfTractor)
                .HasForeignKey(e => e.TractorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Trailer)
                .WithMany(e => e.TrailerAssignmentsOfTrailer)
                .HasForeignKey(e => e.TrailerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Office)
                .WithMany()
                .HasForeignKey(e => e.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
