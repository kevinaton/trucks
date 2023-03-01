using System;

namespace DispatcherWeb.Tickets.Dto
{
    public class GetTruckForTicketDriverInput
    {
        public int DriverId { get; set; }


        public int? OrderLineId { get; set; }
        public DateTime? OrderDate { get; set; }

        public bool ValidateInput()
        {
            if (DriverId == 0)
            {
                return false;
            }

            if (OrderLineId == null && OrderDate == null)
            {
                return false;
            }

            return true;
        }
    }
}
