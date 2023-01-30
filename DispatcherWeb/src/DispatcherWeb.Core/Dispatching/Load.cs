using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Orders;

namespace DispatcherWeb.Dispatching
{
    [Table("Load")]
    public class Load : FullAuditedEntity, IMustHaveTenant
    {
        public Load()
        {
            Tickets = new HashSet<Ticket>();
        }

        public int TenantId { get; set; }

        public int DispatchId { get; set; }
        public Dispatch Dispatch { get; set; }

        public DateTime? SourceDateTime { get; set; }
        public DateTime? DestinationDateTime { get; set; }

        [Obsolete]
        public int? TicketId { get; set; }

        public double? SourceLatitude { get; set; }
        public double? SourceLongitude { get; set; }

        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }

        public Guid? SignatureId { get; set; }
        public string SignatureName { get; set; }
        public Guid? DeferredSignatureId { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
