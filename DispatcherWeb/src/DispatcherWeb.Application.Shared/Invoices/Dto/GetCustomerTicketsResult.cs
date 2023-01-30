using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Invoices.Dto
{
    public class GetCustomerTicketsResult : PagedResultDto<CustomerTicketDto>
    {
        public GetCustomerTicketsResult()
        {
        }

        public GetCustomerTicketsResult(int totalCount, IReadOnlyList<CustomerTicketDto> items) : base(totalCount, items)
        {
        }
    }
}
