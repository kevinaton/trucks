using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Abp.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.PayStatements.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.PayStatements.Exporting
{
    public class PayStatementDetailsCsvExporter : CsvExporterBase, IPayStatementDetailsCsvExporter
    {
        private readonly ISettingManager _settingManager;

        public PayStatementDetailsCsvExporter(
            ITempFileCacheManager tempFileCacheManager,
            ISettingManager settingManager
            ) : base(tempFileCacheManager)
        {
            _settingManager = settingManager;
        }

        public async Task<FileDto> ExportToFileAsync(IList<PayStatementItemEditDto> rows, PayStatementEditDto details)
        {
            var currencyCulture = await _settingManager.GetCurrencyCultureAsync();

            return CreateCsvFile(
                $"DriverPayStatement{details.Id}.csv",
                () =>
                {
                    AddHeader("Statement Id", details.Id.ToString());
                    AddHeader("Statement Date", details.StatementDate.ToString("d"));
                    AddHeader("Start Date", details.StartDate.ToString("d"));
                    AddHeader("End Date", details.EndDate.ToString("d"));

                    AddHeaderAndData(
                        rows,
                        ("Driver", x => x.DriverName),
                        ("Date", x => FormatItemDate(x)),
                        ("Item", x => x.Item),
                        ("Customer", x => x.CustomerName),
                        ("Job #", x => x.JobNumber),
                        ("Deliver To", x => x.DeliverToName),
                        ("Load At", x => x.LoadAtName),
                        ("Time Class", x => x.TimeClassificationName),
                        ("Freight Rate", x => x.FreightRate.ToString("C2", currencyCulture)),
                        ("Driver Pay Rate", x => FormatDriverRate(x, currencyCulture)),
                        ("Quantity", x => x.Quantity.ToString()),
                        ("Ext. Amount", x => x.Total.ToString("C2", currencyCulture))
                    );
                }
            );
        }

        private static string FormatItemDate(PayStatementItemEditDto item)
        {
            return item.ItemKind switch
            {
                PayStatementItemKind.Time => item.Date?.ToString("yyyy'-'MM'-'dd"),
                PayStatementItemKind.Ticket => item.Date?.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
                _ => "",
            };
        }

        private static string FormatDriverRate(PayStatementItemEditDto item, CultureInfo currencyCulture)
        {
            return item.IsProductionPay
                ? item.DriverPayRate + "%"
                : item.DriverPayRate?.ToString("C2", currencyCulture);
        }
    }
}
