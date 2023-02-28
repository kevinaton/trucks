using System;
using System.Collections.Generic;

namespace DispatcherWeb.Scheduling.Dto
{
    public class ScheduleOrderDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
		public Shift? Shift { get; set; }
        public int OfficeId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public bool CustomerIsCod { get; set; }
        public bool IsClosed { get; set; }
        public bool IsShared { get; set; }
        public OrderPriority Priority { get; set; }
        public decimal Utilization { get; set; }
        public IList<ScheduleOrderLineTruckDto> Trucks { get; set; }
		public string Item { get; set; }
		//public string UnitOfMeasure { get; set; }
		//public decimal? Quantity { get; set; }
		public int? Loads { get; set; }
		public decimal? EstimatedAmount { get; set; }
	}
}
