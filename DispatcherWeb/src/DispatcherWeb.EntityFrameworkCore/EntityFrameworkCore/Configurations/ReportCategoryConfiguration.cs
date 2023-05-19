using DispatcherWeb.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class ReportCategoryConfiguration : IEntityTypeConfiguration<ReportCategory>
    {
        public void Configure(EntityTypeBuilder<ReportCategory> builder)
        {
            builder.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(30);
        }
    }
}
