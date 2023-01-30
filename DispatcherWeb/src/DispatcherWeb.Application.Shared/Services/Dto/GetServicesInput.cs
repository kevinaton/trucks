using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Services.Dto
{
    public class GetServicesInput : PagedAndSortedInputDto, IShouldNormalize, IGetServiceListFilter
    {
        public string Name { get; set; }

        public FilterActiveStatus Status { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Service1";
            }
        }
    }
}
