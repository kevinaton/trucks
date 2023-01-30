using DispatcherWeb.Tickets;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Dashboard.RevenueGraph.Dto
{
    public class RevenueGraphDataItemFromTickets : RevenueGraphDataItem, ITicketQuantity
    {
        public decimal Quantity { get; set; }
        public DesignationEnum Designation { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public int? TicketUomId { get; set; }
        public override decimal FreightQuantity { get => this.GetFreightQuantity() ?? 0; }
        public override decimal MaterialQuantity { get => this.GetMaterialQuantity() ?? 0; }
    }
}
