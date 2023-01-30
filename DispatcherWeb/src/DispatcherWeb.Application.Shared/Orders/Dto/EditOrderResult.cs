using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Orders.Dto
{
    public class EditOrderResult
    {
        public int Id { get; set; }

        public bool Completed { get; set; }

        public List<string> NotAvailableTrucks { get; set; }
        public bool HasZeroQuantityItems { get; set; }
    }
}
