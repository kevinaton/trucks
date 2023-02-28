using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure.Attributes;
using DispatcherWeb.Orders;

namespace DispatcherWeb.Trucks
{
    [Table("FuelPurchase")]
    public class FuelPurchase : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int TruckId { get; set; }
        public Truck Truck { get; set; }

        public DateTime FuelDateTime { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Rate { get; set; }

        [MileageColumn]
        public decimal? Odometer { get; set; }

        [StringLength(Ticket.MaxTicketNumberLength)]
        public string TicketNumber { get; set; }
    }
}
