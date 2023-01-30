using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.TimeOffs.Dto
{
    public class GetTimeOffRecordsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? StartDateStart { get; set; }
        public DateTime? StartDateEnd { get; set; }
        public int? DriverId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "DriverName";
            }
        }
    }
}
