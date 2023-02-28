using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Abp.Application.Services;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Imports.Services
{
    [RemoteService(false)]
    public class ImportFuelUsageAppService : ImportTruckDataBaseAppService<ImportFuelUsageRow>, IImportFuelUsageAppService
    {
        private readonly IRepository<FuelPurchase> _fuelPurchaseRepository;
        private readonly IFuelSurchargeCalculator _fuelSurchargeCalculator;
        private List<DateTime> _affectedDateList = new List<DateTime>();

        public ImportFuelUsageAppService(
            IRepository<Truck> truckRepository,
            IRepository<FuelPurchase> fuelPurchaseRepository,
            IFuelSurchargeCalculator fuelSurchargeCalculator,
            IOfficeResolver officeResolver
        ) : base(truckRepository, officeResolver)
        {
            _fuelPurchaseRepository = fuelPurchaseRepository;
            _fuelSurchargeCalculator = fuelSurchargeCalculator;
        }

        protected override bool IsRowEmpty(ImportFuelUsageRow row) =>
            row.TruckNumber.IsNullOrWhiteSpace() || !row.FuelDateTime.HasValue || !row.Amount.HasValue;

        protected override bool ImportRow(ImportFuelUsageRow row, int truckId)
        {
            Debug.Assert(row.FuelDateTime != null, "row.FuelDateTime != null");
            var utcFuelDateTime = ConvertLocalDateTimeToUtcDateTime(row.FuelDateTime.Value);

            FuelPurchase entity = _fuelPurchaseRepository.GetAll()
                .FirstOrDefault(fp => fp.TruckId == truckId && fp.FuelDateTime == utcFuelDateTime);
            if (entity == null)
            {
                entity = new FuelPurchase
                {
                    TruckId = truckId,
                    FuelDateTime = utcFuelDateTime,
                };
                UpdateFields();
                _fuelPurchaseRepository.Insert(entity);
            }
            else
            {
                UpdateFields();
            }

            _affectedDateList.AddIfNotContains(row.FuelDateTime.Value.Date);

            return true;

            void UpdateFields()
            {
                entity.Odometer = row.Odometer;
                entity.Rate = row.FuelRate;
                entity.Amount = row.Amount;
                entity.TicketNumber = row.TicketNumber;
            }
        }
    }
}
