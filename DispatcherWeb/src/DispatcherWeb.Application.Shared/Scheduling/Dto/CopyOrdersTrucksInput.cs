using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Scheduling.Dto
{
    public class CopyOrdersTrucksInput
    {
        public int OriginalOrderId { get; set; }
        public int[] NewOrderIds { get; set; }
        public int? OrderLineId { get; set; }
        public bool ProceedOnConflict { get; set; }

    }
}
