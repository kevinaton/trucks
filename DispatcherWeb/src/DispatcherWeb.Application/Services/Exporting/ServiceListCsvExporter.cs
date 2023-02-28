using System.Collections.Generic;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Services.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Services.Exporting
{
    public class ServiceListCsvExporter : CsvExporterBase, IServiceListCsvExporter
    {
        public ServiceListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<ServiceDto> serviceDtos)
        {
            return CreateCsvFile(
                "ServiceList.csv",
                () =>
                {
                    AddHeader(
                        "Name",
                        "Description",
                        "Active"
                    );

                    AddObjects(
                        serviceDtos,
                        _ => _.Service1,
                        _ => _.Description,
                        _ => _.IsActive.ToYesNoString()
                    );

                }
            );
        }

    }
}
