using DispatcherWeb.Tickets;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto
{
    public class RevenueBreakdownByTruckItemRaw : ITicketQuantity
    {
        public DateTime? DeliveryDate { get; set; }
        public Shift? Shift { get; set; }
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public decimal? FreightPricePerUnit { get; set; }
        public decimal? MaterialPricePerUnit { get; set; }
        public decimal ActualQuantity { get; set; }
        public DesignationEnum Designation { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public int? TicketUomId { get; set; }
        public decimal FreightPriceOriginal { get; set; }
        public decimal MaterialPriceOriginal { get; set; }
        public bool IsFreightPriceOverridden { get; set; }
        public bool IsMaterialPriceOverridden { get; set; }
        public decimal? OrderLineTicketsSum { get; set; }
        public decimal FuelSurcharge { get; set; }
        public decimal PercentQtyForTicket => OrderLineTicketsSum > 0 ? ActualQuantity / OrderLineTicketsSum.Value : 0;

        decimal ITicketQuantity.Quantity => ActualQuantity;
        public decimal ActualMaterialQuantity => this.GetMaterialQuantity() ?? 0;
        public decimal ActualFreightQuantity => this.GetFreightQuantity() ?? 0;
    }
}
