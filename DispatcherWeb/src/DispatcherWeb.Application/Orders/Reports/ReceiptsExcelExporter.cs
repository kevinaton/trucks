using System.Collections.Generic;
using System.Linq;
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
                    var flatData = orderList.SelectMany(x => x.Items, (order, orderLine) => new
                    {
                        IsFirstRow = order.Items.IndexOf(orderLine) == 0,
                        Order = order,
                        OrderLine = orderLine
                    }).ToList();

                    AddHeaderAndData(
                        flatData,
                        ("Delivery Date", x => x.IsFirstRow ? x.Order.DeliveryDate?.ToString("d") : ""),
                        ("Customer", x => x.IsFirstRow ? x.Order.CustomerName : ""),
                        ("Sales Tax", x => x.IsFirstRow ? x.Order.SalesTax.ToString("C2", currencyCulture) : ""),
                        ("Total", x => x.IsFirstRow ? x.Order.CODTotal.ToString("C2", currencyCulture) : ""),
                        ("Item name", x => x.OrderLine.Name),
                        ("Load At", x => x.OrderLine.LoadAtName),
                        ("Deliver To", x => x.OrderLine.DeliverToName),
                        ("Material UOM", x => x.OrderLine.MaterialUomName),
                        ("Freight UOM", x => x.OrderLine.FreightUomName),
                        ("Designation", x => x.OrderLine.DesignationName),
                        ("Actual Material Quantity", x => x.OrderLine.ActualMaterialQuantity?.ToString(Utilities.NumberFormatWithoutRounding)),
                        ("Actual Freight Quantity", x => x.OrderLine.ActualFreightQuantity?.ToString(Utilities.NumberFormatWithoutRounding)),
                        ("Freight Amount", x => x.OrderLine.FreightPrice.ToString("C2", currencyCulture)),
                        ("Material Amount", x => x.OrderLine.MaterialPrice.ToString("C2", currencyCulture))
                    );
                });
        }
    }
}
