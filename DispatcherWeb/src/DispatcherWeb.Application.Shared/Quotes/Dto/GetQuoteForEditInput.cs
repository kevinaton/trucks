using Abp.Application.Services.Dto;

namespace DispatcherWeb.Quotes.Dto
{
    public class GetQuoteForEditInput : NullableIdDto
    {
        public GetQuoteForEditInput()
        {
        }

        public GetQuoteForEditInput(int? id, int? projectId)
            : base(id)
        {
            ProjectId = projectId;
        }

        public int? ProjectId { get; set; }
    }
}
