using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.ProjectHistory.Dto
{
    public class GetProjectHistoryInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ProjectId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "DateTime desc";
            }

            if (Sorting.Contains("DateTime"))
            {
                Sorting = Sorting.Replace("DateTime", "@DateTime");
            }
        }
    }
}
