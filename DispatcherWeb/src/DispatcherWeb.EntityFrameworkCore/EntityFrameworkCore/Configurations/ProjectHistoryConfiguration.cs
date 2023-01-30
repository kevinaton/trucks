using DispatcherWeb.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class ProjectHistoryConfiguration : IEntityTypeConfiguration<ProjectHistoryRecord>
    {
        public void Configure(EntityTypeBuilder<ProjectHistoryRecord> builder)
        {
            builder
                .HasOne(e => e.Project)
                .WithMany(e => e.ProjectHistory)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder    
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder    
                .HasOne(e => e.Office)
                .WithMany()
                .HasForeignKey(e => e.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
