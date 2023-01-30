using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Scheduling.Dto
{
    public class OrderLineTruckDetailsDto : OrderLineTruckUtilizationEditDto
    {
        public DateTime? TimeOnJob { get; set; }
    }
}
