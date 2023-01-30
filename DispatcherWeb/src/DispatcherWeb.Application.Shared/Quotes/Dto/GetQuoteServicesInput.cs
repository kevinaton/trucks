using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Quotes.Dto
{
    public class GetQuoteServicesInput : SortedInputDto, IShouldNormalize
    {
        public int QuoteId { get; set; }

        public int? LoadAtId { get; set; }
        public int? DeliverToId { get; set; }
        public int? ServiceId { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public DesignationEnum? Designation { get; set; }
        public bool ForceDuplicateFilters { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Id";
            }
        }
    }
}
