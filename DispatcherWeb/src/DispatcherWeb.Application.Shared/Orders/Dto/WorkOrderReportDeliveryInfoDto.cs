using System;

namespace DispatcherWeb.Orders.Dto
{
    public class WorkOrderReportDeliveryInfoDto
    {
        public string TruckNumber { get; set; }
        public string TicketNumber { get; set; }
        public decimal? Quantity { get; set; }
        public string UomName { get; set; }
        public Guid? TicketPhotoId { get; set; }
        public string TicketPhoto { get; set; }
        public WorkOrderReportLoadDto Load { get; set; }
        public string DriverName { get; set; }
    }
}
