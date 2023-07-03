using System.Threading.Tasks;
using DispatcherWeb.Dto;

namespace DispatcherWeb.SbtCsvExport
{
    public interface IInvoiceListCsvExporter
    {
        Task<FileDto> ExportToFileAsync<T>(System.Collections.Generic.List<QuickbooksOnline.Dto.InvoiceToUploadDto<T>> invoiceList, string filename);
    }
}
