using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Drivers.Dto
{
    public interface IGetDriverListFilter
    {
        string Name { get; set; }
        FilterActiveStatus Status { get; set; }
        int? OfficeId { get; set; }
        bool? HasUserId { get; set; }
    }
}
