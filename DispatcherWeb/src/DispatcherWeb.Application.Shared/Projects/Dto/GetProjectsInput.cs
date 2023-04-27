using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Projects.Dto
{
    public class GetProjectsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public string Name { get; set; }
        public QuoteStatus? Status { get; set; }
        public DateTime? StartDateStart { get; set; }
        public DateTime? StartDateEnd { get; set; }
        public DateTime? EndDateStart { get; set; }
        public DateTime? EndDateEnd { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
