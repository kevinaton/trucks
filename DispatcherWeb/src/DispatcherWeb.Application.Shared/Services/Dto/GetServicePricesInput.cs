using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Services.Dto
{
    public class GetServicePricesInput : SortedInputDto, IShouldNormalize
    {
        public int ServiceId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "MaterialUomName";
            }
        }
    }
}
