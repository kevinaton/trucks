using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Orders.Dto
{
    public class SetSharedOrderLineInput
    {
        public int OrderLineId { get; set; }
        public int[] CheckedOfficeIds { get; set; }
    }
}
