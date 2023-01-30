using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Invoices.Dto;
using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Invoices
{
    public interface IInvoiceAppService : IApplicationService
    {
        Task<PagedResultDto<InvoiceDto>> GetInvoices(GetInvoicesInput input);

        Task<InvoiceEditDto> GetInvoiceForEdit(NullableIdDto input);

        Task<Document> GetInvoicePrintOut(GetInvoicePrintOutInput input);
        Task<EmailInvoicePrintOutDto> GetEmailInvoicePrintOutModel(EntityDto input);
    }
}
