using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Customers.Dto
{
    public class GetCustomerContactDuplicateCountInput
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public int? ExceptId { get; set; }
    }
}
