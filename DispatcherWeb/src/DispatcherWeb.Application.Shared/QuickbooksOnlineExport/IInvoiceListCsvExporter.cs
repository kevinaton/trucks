using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;

namespace DispatcherWeb.QuickbooksOnlineExport
{
    public interface IInvoiceListCsvExporter
    {
        Task<FileDto> ExportToFileAsync<T>(List<QuickbooksOnline.Dto.InvoiceToUploadDto<T>> recordsList, string filename);
    }
}
