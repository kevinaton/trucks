using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class WialonDeviceTypeConfiguration : IEntityTypeConfiguration<WialonDeviceType>
    {
        public void Configure(EntityTypeBuilder<WialonDeviceType> builder)
        {
            builder
                .Property(x => x.Id)
                .ValueGeneratedNever();
        }
    }
}
