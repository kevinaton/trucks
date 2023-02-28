using System;

namespace DispatcherWeb.Orders.Dto
{
    public class WorkOrderReportLoadDto
    {
        public DateTime? DeliveryTime { get; set; }
        public string SignatureName { get; set; }
        public Guid? SignatureId { get; set; }
        public string Signature { get; set; }
    }
}
