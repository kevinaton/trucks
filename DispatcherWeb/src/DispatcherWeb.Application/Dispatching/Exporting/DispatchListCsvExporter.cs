using System.Collections.Generic;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Dispatching.Exporting
{
    public class DispatchListCsvExporter : CsvExporterBase, IDispatchListCsvExporter
	{
		public DispatchListCsvExporter(
			ITempFileCacheManager tempFileCacheManager
		) : base(tempFileCacheManager)
		{
			
		}

		public FileDto ExportToFile(List<DispatchListDto> dispatchDtos)
		{
			return CreateCsvFile(
				"LoadHistoryList.csv",
				() =>
				{
					AddHeader(
						"Truck",
						"Driver",
						"Sent",
						"Acknowledged",
						"Loaded",
						"Delivered",
                        "Customer",
						"Quote Name",
						"Job Nbr",
                        "Load At",
						"Deliver To",
						"Item",
                        "Quantity",
                        "UOM"
                    );

					AddObjects(
						dispatchDtos,
						_ => _.TruckCode,
						_ => _.DriverLastFirstName,
						_ => _.Sent?.ToString("g"),
						_ => _.Acknowledged?.ToString("g"),
						_ => _.Loaded?.ToString("g"),
						_ => _.Delivered?.ToString("g"),
                        _ => _.CustomerName,
						_ => _.QuoteName,
						_ => _.JobNumber,
                        _ => _.LoadAtName,
                        _ => _.DeliverToName,
						_ => _.Item,
                        _ => _.Quantity?.ToString("N"),
                        _ => _.Uom
                    );

				}
			);
		}

	}
}
