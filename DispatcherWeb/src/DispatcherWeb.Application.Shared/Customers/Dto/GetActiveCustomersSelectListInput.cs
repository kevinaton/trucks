using DispatcherWeb.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Customers.Dto
{
    public class GetActiveCustomersSelectListInput : GetSelectListInput
    {
        public bool IncludeInactiveWithInvoices { get; set; }
    }
}
