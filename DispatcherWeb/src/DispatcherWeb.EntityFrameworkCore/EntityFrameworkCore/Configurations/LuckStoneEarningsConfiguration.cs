using DispatcherWeb.LuckStone;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class LuckStoneEarningsConfiguration : IEntityTypeConfiguration<LuckStoneEarnings>
    {
        public void Configure(EntityTypeBuilder<LuckStoneEarnings> builder)
        {
            builder
                .HasOne(x => x.Batch)
                .WithMany(x => x.LuckStoneEarnings)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
