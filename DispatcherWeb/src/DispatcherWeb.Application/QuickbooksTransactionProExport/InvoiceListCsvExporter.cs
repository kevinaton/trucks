using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Storage;

namespace DispatcherWeb.QuickbooksTransactionProExport
{
    public class InvoiceListCsvExporter : CsvExporterBase, IInvoiceListCsvExporter
    {
        public InvoiceListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }
    }
}
