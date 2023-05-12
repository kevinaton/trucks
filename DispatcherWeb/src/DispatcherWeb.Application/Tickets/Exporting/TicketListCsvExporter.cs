using System.Collections.Generic;
using Abp.Configuration;
using DispatcherWeb.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Storage;
using DispatcherWeb.Tickets.Dto;

namespace DispatcherWeb.Tickets.Exporting
{
    public class TicketListCsvExporter : CsvExporterBase, ITicketListCsvExporter
    {
        public TicketListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<TicketListViewDto> ticketDtos, string fileName)
        {
            var showFuelSurcharge = SettingManager.GetSettingValue<bool>(AppSettings.Fuel.ShowFuelSurcharge);
            var showFreightRateToPayDriverColumn = SettingManager.GetSettingValue<bool>(AppSettings.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate);

            return CreateCsvFile(
                fileName,
                () =>
                {
                    AddHeaderAndData(
                        ticketDtos,
                        ("Ticket Date", x => x.Date?.ToString("g")),
                        ("Order Date", x => x.OrderDate?.ToString("d")),
                        ("Shift", x => x.Shift),
                        ("Office", x => x.Office),
                        ("Customer", x => x.CustomerName),
                        ("Quote Name", x => x.QuoteName),
                        ("Job Nbr", x => x.JobNumber),
                        ("Product / Service", x => x.Product),
                        ("Ticket #", x => x.TicketNumber),
                        ("Carrier", x => x.Carrier),
                        ("Truck", x => x.Truck),
                        ("Driver", x => x.DriverName),
                        ("Load At", x => x.LoadAtName),
                        ("Deliver To", x => x.DeliverToName),
                        ("UOM", x => x.Uom),
                        ("Qty", x => x.Quantity.ToString("N")),
                        ("Revenue", x => x.Revenue.ToString("N")),
                        ("Freight Rate", x => x.FreightRate?.ToString("N")),
                        (showFreightRateToPayDriverColumn ? "Freight Rate to Pay Driver" ?? "" : null, showFreightRateToPayDriverColumn ? x => x.FreightRateToPayDrivers?.ToString("N") : null),
                        ("Material Rate", x => x.MaterialRate?.ToString("N")),
                        ("Freight Amount", x => x.FreightAmount?.ToString("N")),
                        ("Material Amount", x => x.MaterialAmount?.ToString("N")),
                        (showFuelSurcharge ? "Fuel Surcharge" : null, showFuelSurcharge ? x => x.FuelSurcharge?.ToString("N") : null),
                        ("Price Override", x => x.PriceOverride?.ToString("N2")),
                        ("Billed", x => x.IsBilled.ToYesNoString()),
                        ("Imported", x => x.IsImported.ToYesNoString()),
                        ("Verified", x => x.IsVerified.ToYesNoString()),
                        ("Production pay", x => x.ProductionPay?.ToYesNoString() ?? ""),
                        ("Statement", x => x.PayStatementId?.ToString("N"))
                    );
                }
            );
        }
    }
}
