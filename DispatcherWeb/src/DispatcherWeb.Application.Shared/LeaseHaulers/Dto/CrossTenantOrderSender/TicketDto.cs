using System;

namespace DispatcherWeb.LeaseHaulers.Dto.CrossTenantOrderSender
{
    public class TicketDto
    {
        public int Id { get; set; }
        public int? OrderLineId { get; set; }
        public int? MaterialCompanyOrderLineId { get; set; }
        public string TicketNumber { get; set; }
        public decimal Quantity { get; set; }
        public TruckDto Truck { get; set; }
        public string TruckCode { get; set; }
        public ServiceDto Service { get; set; }
        public LocationDto DeliverTo { get; set; }
        public LocationDto LoadAt { get; set; }
        public UnitOfMeasureDto UnitOfMeasure { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public DriverDto Driver { get; set; }
        public Guid? TicketPhotoId { get; set; }
        public string TicketPhotoFilename { get; set; }
        public decimal FuelSurcharge { get; set; }
    }
}
