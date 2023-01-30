using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Customers.Dto
{
    public interface IGetCustomerListFilter
    {
        string Name { get; set; }
        FilterActiveStatus Status { get; set; }
    }
}
