using System.Collections.Generic;
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
                    AddHeader(
                        "*InvoiceNo",
                        "*Customer",
                        "*InvoiceDate",
                        "*DueDate",
                        "Terms",
                        "Location",
                        "Memo",
                        "Item(Product/Service)",
                        "ItemDescription",
                        "ItemQuantity",
                        "ItemRate",
                        "*ItemAmount",
                        "Taxable",
                        "TaxRate",
                        "Service Date"
                    );

                    foreach (var invoice in invoiceList)
                    {
                        var firstRow = true;
                        foreach (var invoiceLine in invoice.InvoiceLines)
                        {
                            AddObjects(
                                new[] {
                                    new
                                    {
                                        InvoiceNumber = invoiceNumberPrefix + invoice.InvoiceId,
                                        CustomerName = invoice.Customer.Name,
                                        invoice.IssueDate,
                                        invoice.DueDate,
                                        invoice.Terms,
                                        invoice.Message,
                                        invoiceLine.ItemName,
                                        invoiceLine.Quantity,
                                        invoiceLine.Subtotal,
                                        invoiceLine.IsTaxable,
                                        invoice.TaxRate,
                                    }
                                },
                                _ => _.InvoiceNumber,
                                _ => firstRow ? _.CustomerName : "",
                                _ => firstRow ? _.IssueDate?.ToString("d") : "",
                                _ => firstRow ? _.DueDate?.ToString("d") : "",
                                _ => firstRow ? _.Terms?.GetDisplayName() : "",
                                _ => "",
                                _ => firstRow ? _.Message : "",
                                _ => _.ItemName,
                                _ => "",
                                _ => _.Quantity.ToString(),
                                _ => (_.Subtotal / _.Quantity).ToString(),
                                _ => _.Subtotal.ToString(),
                                _ => _.IsTaxable == true ? "Y" : "N",
                                _ => firstRow ? _.TaxRate + "%" : "",
                                _ => _.IssueDate?.ToString("d")
                            );

                            firstRow = false;
                        }
                    }
                });
        }
    }
}
