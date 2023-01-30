using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Orders.Dto
{
    public class StaggeredTimeLineDto
    {
        public int LineNumber { get; set; }
        public DateTime? TimeOnJob { get; set; }
    }
}
