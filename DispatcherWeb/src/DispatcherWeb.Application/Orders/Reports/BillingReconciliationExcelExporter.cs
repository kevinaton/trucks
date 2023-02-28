using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Orders.Reports
{
    public class BillingReconciliationExcelExporter : CsvExporterBase, IBillingReconciliationExcelExporter
    {
        private readonly ISettingManager _settingManager;

        public BillingReconciliationExcelExporter(
            ITempFileCacheManager tempFileCacheManager,
            ISettingManager settingManager)
            : base(tempFileCacheManager)
        {
            _settingManager = settingManager;
        }
        public async Task<FileDto> ExportToFileAsync(List<BillingReconciliationReportDto> orderList)
        {
            var currencyCulture = await _settingManager.GetCurrencyCultureAsync();

            return CreateCsvFile(
                "BillingReconciliation.csv",
                () =>
                {
                    AddHeader(
                        "Billed",
                        "Delivery Date",
                        "Customer",
                        "Sales Tax",
                        "Total",
                        "Item name",
                        "Load at",
                        "Deliver to",
                        "Material UOM",
                        "Freight UOM",
                        "Designation",
                        "Actual Quantity",
                        "Freight Amount",
                        "Material Amount"
                    );

                    foreach (var order in orderList)
                    {
                        var firstRow = true;
                        foreach (var orderLine in order.Items)
                        {
                            AddObjects(
                                new[] {
                                    new
                                    {
                                        IsBilled = order.IsBilled ? "X" : "",
                                        order.DeliveryDate,
                                        order.CustomerName,
                                        order.SalesTax,
                                        order.CODTotal,
                                        orderLine.Name,
                                        orderLine.LoadAtName,
                                        orderLine.DeliverToName,
                                        orderLine.MaterialUomName,
                                        orderLine.FreightUomName,
                                        orderLine.DesignationName,
                                        orderLine.TicketQuantity,
                                        orderLine.FreightPrice,
                                        orderLine.MaterialPrice
                                    }
                                },
                                _ => firstRow ? _.IsBilled : "",
                                _ => firstRow ? _.DeliveryDate?.ToString("d") : "",
                                _ => firstRow ? _.CustomerName : "",
                                _ => firstRow ? _.SalesTax.ToString("C2", currencyCulture) : "",
                                _ => firstRow ? _.CODTotal.ToString("C2", currencyCulture) : "",
                                _ => _.Name,
                                _ => _.LoadAtName,
                                _ => _.DeliverToName,
                                _ => _.MaterialUomName,
                                _ => _.FreightUomName,
                                _ => _.DesignationName,
                                _ => _.TicketQuantity?.ToString(Utilities.NumberFormatWithoutRounding),
                                _ => _.FreightPrice.ToString("C2", currencyCulture),
                                _ => _.MaterialPrice.ToString("C2", currencyCulture)
                            );

                            firstRow = false;
                        }
                    }
                });
        }
    }
}
