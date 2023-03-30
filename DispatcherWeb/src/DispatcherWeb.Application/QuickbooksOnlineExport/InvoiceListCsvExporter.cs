using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using DispatcherWeb.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.QuickbooksOnline.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.QuickbooksOnlineExport
{
    public class InvoiceListCsvExporter : CsvExporterBase, IInvoiceListCsvExporter
    {
        private readonly ISettingManager _settingManager;

        public InvoiceListCsvExporter(
            ISettingManager settingManager,
            ITempFileCacheManager tempFileCacheManager
            ) : base(tempFileCacheManager)
        {
            _settingManager = settingManager;
        }

        public async Task<FileDto> ExportToFileAsync<T>(List<InvoiceToUploadDto<T>> invoiceList, string filename)
        {
            var invoiceNumberPrefix = await _settingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix);

            return CreateCsvFile(
                filename + ".csv",
                () =>
                {
                    var flatData = invoiceList.SelectMany(x => x.InvoiceLines, (invoice, invoiceLine) => new
                    {
                        IsFirstRow = invoice.InvoiceLines.IndexOf(invoiceLine) == 0,
                        Invoice = invoice,
                        InvoiceLine = invoiceLine
                    }).ToList();

                    AddHeaderAndData(flatData,
                        ("*InvoiceNo", x => invoiceNumberPrefix + x.Invoice.InvoiceId),
                        ("*Customer", x => x.IsFirstRow ? x.Invoice.Customer.Name : ""),
                        ("*InvoiceDate", x => x.IsFirstRow ? x.Invoice.IssueDate?.ToString("d") : ""),
                        ("*DueDate", x => x.IsFirstRow ? x.Invoice.DueDate?.ToString("d") : ""),
                        ("Terms", x => x.IsFirstRow ? x.Invoice.Terms?.GetDisplayName() : ""),
                        ("Location", x => ""),
                        ("Memo", x => x.IsFirstRow ? x.Invoice.Message : ""),
                        ("Item(Product/Service)", x => x.InvoiceLine.ItemName),
                        ("ItemDescription", x => ""),
                        ("ItemQuantity", x => x.InvoiceLine.Quantity.ToString()),
                        ("ItemRate", x => (x.InvoiceLine.Subtotal / x.InvoiceLine.Quantity).ToString()),
                        ("*ItemAmount", x => x.InvoiceLine.Subtotal.ToString()),
                        ("Taxable", x => x.InvoiceLine.IsTaxable == true ? "Y" : "N"),
                        ("TaxRate", x => x.IsFirstRow ? x.Invoice.TaxRate + "%" : ""),
                        ("Service Date", x => x.Invoice.IssueDate?.ToString("d"))
                    );
                });
        }
    }
}
