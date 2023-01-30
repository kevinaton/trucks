using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Offices;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    [RemoteService(false)]
    public class ImportVehicleUsageAppService : ImportTruckDataBaseAppService<ImportVehicleUsageRow>, IImportVehicleUsageAppService
    {
        private readonly IRepository<VehicleUsage> _vehicleUsageRepository;

        public ImportVehicleUsageAppService(
            IRepository<Truck> truckRepository,
            IRepository<VehicleUsage> vehicleUsageRepository,
            IOfficeResolver officeResolver
        ) : base(truckRepository, officeResolver)
        {
            _vehicleUsageRepository = vehicleUsageRepository;
        }

        protected override bool IsRowEmpty(ImportVehicleUsageRow row) => 
            row.TruckNumber.IsNullOrWhiteSpace() || row.ReadingDateTime == null || 
                row.EngineHours == null && row.OdometerReading == null;

        protected override bool ImportRow(ImportVehicleUsageRow row, int truckId)
        {
            bool isImported = false;
            if (row.OdometerReading.HasValue)
            {
                CreateOrUpdateVehicleUsage(ReadingType.Miles);
                isImported = true;
            }
            if (row.EngineHours.HasValue)
            {
                CreateOrUpdateVehicleUsage(ReadingType.Hours);
                isImported = true;
            }

            return isImported;

            // Local functions
            void CreateOrUpdateVehicleUsage(ReadingType readingType)
            {
                Debug.Assert(row.ReadingDateTime != null, "row.ReadingDateTime != null");
                var utcReadingDateTime = ConvertLocalDateTimeToUtcDateTime(row.ReadingDateTime.Value);

                VehicleUsage entity = _vehicleUsageRepository.GetAll()
                    .FirstOrDefault(vu => 
                        vu.TruckId == truckId && 
                        vu.ReadingDateTime == utcReadingDateTime &&
                        vu.ReadingType == readingType
                    );
                if (entity == null)
                {
                    entity = new VehicleUsage
                    {
                        TruckId = truckId,
                        ReadingDateTime = utcReadingDateTime,
                    };
                }
                UpdateFields(entity, GetReadingByType(readingType), readingType);
                _vehicleUsageRepository.InsertOrUpdate(entity);
            }

            decimal GetReadingByType(ReadingType readingType)
            {
                switch (readingType)
                {
                    case ReadingType.Miles:
                        Debug.Assert(row.OdometerReading != null, "row.OdometerReading != null");
                        return row.OdometerReading.Value;
                    case ReadingType.Hours:
                        Debug.Assert(row.EngineHours != null, "row.EngineHours != null");
                        return row.EngineHours.Value;
                    default:
                        throw new ArgumentException($"Usupported ReadingType: {readingType}");
                }
            }

            void UpdateFields(VehicleUsage entity, decimal reaingValue, ReadingType readingType)
            {
                Debug.Assert(row.OdometerReading != null || row.EngineHours != null);
                Debug.Assert(readingType == ReadingType.Miles && row.OdometerReading != null || readingType == ReadingType.Hours && row.EngineHours != null);
                entity.ReadingType = readingType;
                entity.Reading = reaingValue;
            }
        }
    }
}
