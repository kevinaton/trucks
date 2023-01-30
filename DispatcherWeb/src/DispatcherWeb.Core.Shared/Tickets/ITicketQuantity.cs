using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Tickets
{
    public interface ITicketQuantity
    {
        decimal Quantity { get; }
        DesignationEnum Designation { get; }
        int? MaterialUomId { get; }
        int? FreightUomId { get; }
        int? TicketUomId { get; }
        //decimal? MaterialQuantity { get; set; }
        //decimal? FreightQuantity { get; set; }
    }
}
