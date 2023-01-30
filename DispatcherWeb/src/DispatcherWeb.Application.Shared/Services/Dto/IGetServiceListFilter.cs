using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Services.Dto
{
    public interface IGetServiceListFilter
    {
        string Name { get; set; }
        FilterActiveStatus Status { get; set; }
    }
}
