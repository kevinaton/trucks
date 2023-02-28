namespace DispatcherWeb.Tickets
{
    public class TicketQuantityDto : ITicketQuantity
    {
        public TicketQuantityDto()
        {
        }

        public TicketQuantityDto(decimal quantity, DesignationEnum designation, int? materialUomId, int? freightUomId, int? ticketUomId)
        {
            Quantity = quantity;
            Designation = designation;
            MaterialUomId = materialUomId;
            FreightUomId = freightUomId;
            TicketUomId = ticketUomId;
        }

        public decimal Quantity { get; set; }

        public DesignationEnum Designation { get; set; }

        public int? MaterialUomId { get; set; }

        public int? FreightUomId { get; set; }

        public int? TicketUomId { get; set; }

        public decimal FuelSurcharge { get; set; }
    }
}
