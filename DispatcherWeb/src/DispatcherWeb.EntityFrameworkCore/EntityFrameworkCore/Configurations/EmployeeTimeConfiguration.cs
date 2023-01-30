using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class EmployeeTimeConfiguration : IEntityTypeConfiguration<EmployeeTime>
    {
        public void Configure(EntityTypeBuilder<EmployeeTime> builder)
        {
            builder
                .Property(e => e.Latitude)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimalLocation);

            builder
                .Property(e => e.Longitude)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimalLocation);

            builder
                .HasOne(e => e.TimeClassification)
                .WithMany()
                .HasForeignKey(e => e.TimeClassificationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Driver)
                .WithMany()
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(p => p.TimeOff)
                .WithMany(p => p.EmployeeTimes)
                .HasForeignKey(p => p.TimeOffId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
