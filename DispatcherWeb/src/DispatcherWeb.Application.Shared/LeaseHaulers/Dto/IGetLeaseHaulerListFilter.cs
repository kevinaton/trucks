using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public interface IGetLeaseHaulerListFilter
    {
        string Name { get; set; }
        string City { get; set; }
        string State { get; set; }
    }
}
