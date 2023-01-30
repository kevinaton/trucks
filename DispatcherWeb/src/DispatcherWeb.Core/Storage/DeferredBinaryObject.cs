using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DispatcherWeb.Storage
{
    [Table("DeferredBinaryObject")]
    public class DeferredBinaryObject : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public DeferredBinaryObjectDestination Destination { get; set; }
        public Guid BinaryObjectId { get; set; }
    }
}
