using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Quotes.Dto
{
    public class GetProjectQuotesInput : SortedInputDto, IShouldNormalize
    {
        public int? ProjectId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Id";
            }
        }
    }
}
