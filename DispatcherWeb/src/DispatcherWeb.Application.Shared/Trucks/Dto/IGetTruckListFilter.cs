using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Trucks.Dto
{
    public interface IGetTruckListFilter
    {
        int? OfficeId { get; set; }
        int? VehicleCategoryId { get; set; }
        FilterActiveStatus Status { get; set; }
        bool? IsOutOfService { get; set; }
        bool PlatesExpiringThisMonth { get; set; }
    }
}
