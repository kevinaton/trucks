using Abp.Configuration;
using DispatcherWeb.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulerStatements.Dto;
using DispatcherWeb.Storage;
using System;
using System.Collections.Generic;
using System.Text;

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

                    AddHeader(
                        "Order Date",
                        "Shift",
                        "Customer",
                        "Product / Service",
                        "Ticket #",
                        "Ticket Date Time",
                        "Carrier",
                        "Truck",
                        "Driver",
                        "Load At",
                        "Deliver To",
                        "UOM",
                        "Quantity",
                        "Rate",
                        "BrokerFee",
                        showFuelSurcharge ? "Fuel" : null,
                        "ExtendedAmount"
                    );

                    AddObjects(
                        data.Tickets,
                        _ => _.OrderDate?.ToString("d"),
                        _ => _.ShiftName,
                        _ => _.CustomerName,
                        _ => _.ServiceName,
                        _ => _.TicketNumber,
                        _ => _.TicketDateTime?.ToString("g"),
                        _ => _.CarrierName,
                        _ => _.TruckCode,
                        _ => _.DriverName,
                        _ => _.LoadAtName,
                        _ => _.DeliverToName,
                        _ => _.UomName,
                        _ => _.Quantity.ToString(),
                        _ => _.Rate?.ToString(),
                        _ => _.BrokerFee.ToString(),
                        showFuelSurcharge ? _ => _.FuelSurcharge.ToString("N2") : null,
                        _ => _.ExtendedAmount.ToString()
                    );

                }
            );
        }
    }
}
