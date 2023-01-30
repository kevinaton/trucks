using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.FuelPurchases.Dto
{
    public class GetFuelPurchasePagedListInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public int? OfficeId { get; set; }

        public DateTime? FuelDateTimeBegin { get; set; }
        public DateTime? FuelDateTimeEnd { get; set; }

        public int? TruckId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "TruckCode";
            }
        }

    }
}
