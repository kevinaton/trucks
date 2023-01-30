using Abp.Application.Services.Dto;

namespace DispatcherWeb.Quotes.Dto
{
    public class GetQuoteServiceForEditInput : NullableIdDto
    {
        public GetQuoteServiceForEditInput()
        {
        }

        public GetQuoteServiceForEditInput(int? id, int? quoteId)
            : base(id)
        {
            QuoteId = quoteId;
        }

        public int? QuoteId { get; set; }
    }
}
