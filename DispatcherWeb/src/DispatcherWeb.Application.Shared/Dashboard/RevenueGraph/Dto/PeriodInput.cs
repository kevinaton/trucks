using DispatcherWeb.Dashboard.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dashboard.RevenueGraph.Dto
{
    public class PeriodInput
    {
        public DateTime PeriodBegin { get; set; }
        public DateTime PeriodEnd { get; set; }
        public TicketType TicketType { get; set; }

    }
}
