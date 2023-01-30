using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;
using System;

namespace DispatcherWeb.EmployeeTime.Dto
{
    public class GetEmployeeTimeRecordsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? StartDateStart { get; set; }
        public DateTime? StartDateEnd { get; set; }
        public int? TimeClassificationId { get; set; }
        public long? EmployeeId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "EmployeeName";
            }
        }
    }
}
