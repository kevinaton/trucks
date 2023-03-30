using Abp.Configuration;
using DispatcherWeb.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulerStatements.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.LeaseHaulerStatements.Exporting
{
    public class LeaseHaulerStatementCsvExporter : CsvExporterBase, ILeaseHaulerStatementCsvExporter
    {
        public LeaseHaulerStatementCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(LeaseHaulerStatementReportDto data)
        {
            var showFuelSurcharge = SettingManager.GetSettingValue<bool>(AppSettings.Fuel.ShowFuelSurcharge);

            return CreateCsvFile(
                $"LeaseHaulerStatement{data.Id}.csv",
                () =>
                {
                    AddHeader(
                        $"Statement Id: {data.Id}",
                        $"Statement Date: {data.StatementDate:d}",
                        $"Start Date: {data.StartDate:d}",
                        $"End Date: {data.EndDate:d}"
                    );

                    AddHeaderAndData(
                        data.Tickets,
                        ("Order Date", x => x.OrderDate?.ToString("d")),
                        ("Shift", x => x.ShiftName),
                        ("Customer", x => x.CustomerName),
                        ("Product / Service", x => x.ServiceName),
                        ("Ticket #", x => x.TicketNumber),
                        ("Ticket Date Time", x => x.TicketDateTime?.ToString("g")),
                        ("Carrier", x => x.CarrierName),
                        ("Truck", x => x.TruckCode),
                        ("Driver", x => x.DriverName),
                        ("Load At", x => x.LoadAtName),
                        ("Deliver To", x => x.DeliverToName),
                        ("UOM", x => x.UomName),
                        ("Quantity", x => x.Quantity.ToString()),
                        ("Rate", x => x.Rate?.ToString()),
                        ("BrokerFee", x => x.BrokerFee.ToString()),
                        (showFuelSurcharge ? "Fuel" : null, showFuelSurcharge ? x => x.FuelSurcharge.ToString("N2") : null),
                        ("ExtendedAmount", x => x.ExtendedAmount.ToString())
                    );
                }
            );
        }
    }
}
