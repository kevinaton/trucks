using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Orders.Reports
{
    public class ReceiptsExcelExporter : CsvExporterBase, IReceiptsExcelExporter
    {
        private readonly ISettingManager _settingManager;

        public ReceiptsExcelExporter(
            ITempFileCacheManager tempFileCacheManager,
            ISettingManager settingManager)
            : base(tempFileCacheManager)
        {
            _settingManager = settingManager;
        }
        public async Task<FileDto> ExportToFileAsync(List<ReceiptExcelReportDto> orderList)
        {
            var currencyCulture = await _settingManager.GetCurrencyCultureAsync();

            return CreateCsvFile(
                "Receipts.csv",
                () =>
                {
                    AddHeader(
                        "Delivery Date",
                        "Customer",
                        "Sales Tax",
                        "Total",
                        "Item name",
                        "Load At",
                        "Deliver To",
                        "Material UOM",
                        "Freight UOM",
                        "Designation",
                        "Actual Material Quantity",
                        "Actual Freight Quantity",
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
                                        orderLine.ActualMaterialQuantity,
                                        orderLine.ActualFreightQuantity,
                                        orderLine.FreightPrice,
                                        orderLine.MaterialPrice
                                    }
                                },
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
                                _ => _.ActualMaterialQuantity?.ToString(Utilities.NumberFormatWithoutRounding),
                                _ => _.ActualFreightQuantity?.ToString(Utilities.NumberFormatWithoutRounding),
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
