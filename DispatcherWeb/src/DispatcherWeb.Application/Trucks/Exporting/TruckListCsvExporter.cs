using System.Collections.Generic;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Storage;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Trucks.Exporting
{
    public class TruckListCsvExporter : CsvExporterBase, ITruckListCsvExporter
    {
        public TruckListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<TruckEditDto> truckEditDtos)
        {
            return CreateCsvFile(
                "TruckList.csv",
                () =>
                {
                    AddHeader(
                        L("TruckCode"),
                        L("Office"),
                        L("Category"),
                        L("Apportioned"),
                        L("BedConstruction"),
                        L("CanPullTrailer"),
                        L("DefaultDriver"),
                        L("DefaultTrailer"),
                        L("IsActive"),
                        L("InactivationDate"),
                        L("OutOfService"),
                        L("Reason"),
                        L("Year"),
                        L("Make"),
                        L("Model"),
                        L("InServiceDate"),
                        L("VIN"),
                        L("Plate"),
                        L("PlateExpiration"),
                        "Ave Load(Tons)",
                        "Ave Load(Cyds)",
                        L("InsurancePolicyNumber"),
                        L("InsuranceValidUntil"),
                        L("PurchaseDate"),
                        L("PurchasePrice"),
                        L("SoldDate"),
                        L("SoldPrice"),

                        L("FuelType"),
                        L("FuelCapacity"),
                        L("SteerTires"),
                        L("DriveAxleTires"),
                        L("DropAxleTires"),
                        L("TrailerTires"),
                        L("Transmission"),
                        L("Engine"),
                        L("RearEnd"),
                        L("CurrentMileage"),
                        L("CurrentHours"),
                        L("TruxTruckId"),
                        L("UniqueId")
                    );

                    AddObjects(
                        truckEditDtos,
                        _ => _.TruckCode,
                        _ => _.OfficeName,
                        _ => _.VehicleCategoryName,
                        _ => _.IsApportioned.ToYesNoString(),
                        _ => _.BedConstruction.GetDisplayName(),
                        _ => _.CanPullTrailer.ToYesNoString(),
                        _ => _.DefaultDriverName,
                        _ => _.DefaultTrailerCode,
                        _ => _.IsActive.ToYesNoString(),
                        _ => _.InactivationDate?.ToShortDateString(),
                        _ => _.IsOutOfService.ToYesNoString(),
                        _ => _.Reason,
                        _ => _.Year?.ToString("N0"),
                        _ => _.Make,
                        _ => _.Model,
                        _ => _.InServiceDate.ToShortDateString(),
                        _ => _.Vin,
                        _ => _.Plate,
                        _ => _.PlateExpiration?.ToShortDateString(),
                        _ => _.CargoCapacity?.ToString("N0"),
                        _ => _.CargoCapacityCyds?.ToString("N0"),
                        _ => _.InsurancePolicyNumber,
                        _ => _.InsuranceValidUntil?.ToShortDateString(),
                        _ => _.PurchaseDate?.ToShortDateString(),
                        _ => _.PurchasePrice?.ToString("N"),
                        _ => _.SoldDate?.ToShortDateString(),
                        _ => _.SoldPrice?.ToString("N"),
                        _ => _.FuelType?.GetDisplayName(),
                        _ => _.FuelCapacity?.ToString("N0"),
                        _ => _.SteerTires,
                        _ => _.DriveAxleTires,
                        _ => _.DropAxleTires,
                        _ => _.TrailerTires,
                        _ => _.Transmission,
                        _ => _.Engine,
                        _ => _.RearEnd,
                        _ => _.CurrentMileage.ToString("N0"),
                        _ => _.CurrentHours.ToString("N0"),
                        _ => _.TruxTruckId,
                        _ => _.DtdTrackerUniqueId
                    );

                }
            );
        }

    }
}
