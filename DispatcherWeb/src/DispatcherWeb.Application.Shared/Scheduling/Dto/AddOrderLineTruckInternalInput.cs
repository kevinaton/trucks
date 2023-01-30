using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Scheduling.Dto
{
    public class AddOrderLineTruckInternalInput : AddOrderLineTruckInput
    {
        public AddOrderLineTruckInternalInput()
        {
            
        }
        public AddOrderLineTruckInternalInput(AddOrderLineTruckInput addOrderLineTruckInput, decimal utilization)
        {
            base.TruckId = addOrderLineTruckInput.TruckId;
            base.OrderLineId = addOrderLineTruckInput.OrderLineId;
            base.ParentId = addOrderLineTruckInput.ParentId;
            base.DriverId = addOrderLineTruckInput.DriverId;
            Utilization = utilization;
        }
        public decimal Utilization { get; set; }
    }
}
