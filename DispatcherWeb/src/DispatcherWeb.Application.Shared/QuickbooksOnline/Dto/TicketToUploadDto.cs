using System;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class TicketToUploadDto : ITicketQuantity
    {
        public DateTime? TicketDateTimeUtc { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public int? TicketUomId { get; set; }
        public bool? IsOrderLineMaterialTotalOverridden { get; set; }
        public bool? IsOrderLineFreightTotalOverridden { get; set; }
        public decimal? OrderLineMaterialTotal { get; set; }
        public decimal? OrderLineFreightTotal { get; set; }
        public DesignationEnum? Designation { get; set; }
        DesignationEnum ITicketQuantity.Designation => Designation ?? DesignationEnum.MaterialOnly;
        public bool HasOrderLine { get; set; }

        public decimal Quantity { get; set; }

        public TicketToUploadDto Clone()
        {
            return new TicketToUploadDto
            {
                TicketDateTimeUtc = TicketDateTimeUtc,
                FreightUomId = FreightUomId,
                MaterialUomId = MaterialUomId,
                TicketUomId = TicketUomId,
                IsOrderLineMaterialTotalOverridden = IsOrderLineMaterialTotalOverridden,
                IsOrderLineFreightTotalOverridden = IsOrderLineFreightTotalOverridden,
                OrderLineMaterialTotal = OrderLineMaterialTotal,
                OrderLineFreightTotal = OrderLineFreightTotal,
                Designation = Designation,
                HasOrderLine = HasOrderLine,
                Quantity = Quantity,
            };
        }
    }
}
