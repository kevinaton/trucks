using System.Collections.Generic;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Locations.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Locations.Exporting
{
    public class LocationListCsvExporting : CsvExporterBase, ILocationListCsvExporting
    {
        public LocationListCsvExporting(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<LocationDto> locationDtos)
        {
            return CreateCsvFile(
                "LocationList.csv",
                () =>
                {
                    AddHeader(
                        "Name",
                        "Category",
                        "Street Address",
                        "City",
                        "State",
                        "Zip Code",
                        "Country Code",
                        "Active",
                        "Abbreviation",
                        "Notes"
                    );

                    AddObjects(
                        locationDtos,
                        _ => _.Name,
                        _ => _.CategoryName,
                        _ => _.StreetAddress,
                        _ => _.City,
                        _ => _.State,
                        _ => _.ZipCode,
                        _ => _.CountryCode,
                        _ => _.IsActive.ToYesNoString(),
                        _ => _.Abbreviation,
                        _ => _.Notes
                    );

                }
            );
        }

    }
}
