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

            return CreateCsvFile(
                fileName,
                () =>
                {
                    AddHeader(
                        "Ticket Date",
                        "Order Date",
                        "Shift",
                        "Customer",
                        "Quote Name",
                        "Job Nbr",
                        "Product / Service",
                        "Ticket #",
                        "Carrier",
                        "Truck",
                        "Driver",
                        "Load At",
                        "Deliver To",
                        "UOM",
                        "Qty",
                        "Revenue",
                        "Freight Rate",
                        "Material Rate",
                        "Freight Amount",
                        "Material Amount",
                        showFuelSurcharge ? "Fuel Surcharge" : null,
                        "Price Override",
                        "Billed",
                        "Imported",
                        "Verified",
                        "Production pay",
                        "Statement"
                    );

                    AddObjects(
                        ticketDtos,
                        _ => _.Date?.ToString("g"),
                        _ => _.OrderDate?.ToString("d"),
                        _ => _.Shift,
                        _ => _.CustomerName,
                        _ => _.QuoteName,
                        _ => _.JobNumber,
                        _ => _.Product,
                        _ => _.TicketNumber,
                        _ => _.Carrier,
                        _ => _.Truck,
                        _ => _.DriverName,
                        _ => _.LoadAtName,
                        _ => _.DeliverToName,
                        _ => _.Uom,
                        _ => _.Quantity.ToString("N"),
                        _ => _.Revenue.ToString("N"),
                        _ => _.FreightRate?.ToString("N"),
                        _ => _.MaterialRate?.ToString("N"),
                        _ => _.FreightAmount?.ToString("N"),
                        _ => _.MaterialAmount?.ToString("N"),
                        showFuelSurcharge ? _ => _.FuelSurcharge?.ToString("N") : null,
                        _ => _.PriceOverride?.ToString("N2"),
                        _ => _.IsBilled.ToYesNoString(),
                        _ => _.IsImported.ToYesNoString(),
                        _ => _.IsVerified.ToYesNoString(),
                        _ => _.ProductionPay?.ToYesNoString() ?? "",
                        _ => _.PayStatementId?.ToString("N")
                    );

                }
            );
        }
    }
}
