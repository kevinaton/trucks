using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Invoices.Dto;
using MigraDoc.DocumentObjectModel;

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
