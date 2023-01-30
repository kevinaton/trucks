using DispatcherWeb.Tickets;

namespace DispatcherWeb.Dispatching.Dto
{
    public class ViewDispatchTicketDto : ITicketQuantity
    {
        public decimal Quantity { get; set; }
        public DesignationEnum Designation { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public int? TicketUomId { get; set; }
    }
}
