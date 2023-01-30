using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Trucks;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;
using Abp.Timing;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Imports.Services
{
    public class ImportTrucksAppService : ImportDataBaseAppService<TruckImportRow>, IImportTrucksAppService
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private HashSet<string> _existingTruckNames;
        private readonly IOfficeResolver _officeResolver;
        private int? _officeId = null;
        private List<VehicleCategoryDto> _vehicleCategories;

        public ImportTrucksAppService(
            IRepository<Truck> truckRepository,
            IRepository<VehicleCategory> vehicleCategoryRepository,
            IOfficeResolver officeResolver
        )
        {
            _truckRepository = truckRepository;
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _officeResolver = officeResolver;
        }

        protected override bool CacheResourcesBeforeImport(IImportReader reader)
        {
            _existingTruckNames = new HashSet<string>(_truckRepository.GetAll().Select(x => x.TruckCode));

            _officeId = _officeResolver.GetOfficeId(_userId.ToString());
            if (_officeId == null)
            {
                _result.NotFoundOffices.Add(_userId.ToString());
                return false;
            }

            _vehicleCategories = _vehicleCategoryRepository.GetAll()
                .Select(x => new VehicleCategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    //AssetType = x.AssetType,
                    //IsPowered = x.IsPowered,
                    //SortOrder = x.SortOrder
                }).ToList();

            return base.CacheResourcesBeforeImport(reader);
        }

        protected override bool ImportRow(TruckImportRow row)
        {
            if (_existingTruckNames.Contains(row.TruckCode))
            {
                return false;
            }

            var vehicleCategoryId = GetVehicleCategory(row);
            if (vehicleCategoryId == null)
            {
                return false;
            }

            var truck = new Truck
            {
                TruckCode = row.TruckCode,
                VehicleCategoryId = vehicleCategoryId.Value,
                LocationId = _officeId,
                IsActive = true,
                //InactivationDate
                //IsOutOfService
                CurrentMileage = row.CurrentMileage,
                CurrentHours = row.CurrentHours,
                Year = row.Year,
                Make = row.Make,
                Model = row.Model,
                Vin = row.Vin,
                Plate = row.Plate,
                PlateExpiration = row.PlateExpiration,
                CargoCapacity = row.CargoCapacity,
                CargoCapacityCyds = row.CargoCapacityCyds,
                FuelType = row.FuelType,
                FuelCapacity = row.FuelCapacity,
                SteerTires = row.SteerTires,
                DriveAxleTires = row.DriveAxleTires,
                DropAxleTires = row.DropAxleTires,
                TrailerTires = row.TrailerTires,
                Transmission = row.Transmission,
                Engine = row.Engine,
                RearEnd = row.RearEnd,
                InsurancePolicyNumber = row.InsurancePolicyNumber,
                InsuranceValidUntil = row.InsuranceValidUntil,
                PurchaseDate = row.PurchaseDate,
                PurchasePrice = row.PurchasePrice,
                InServiceDate = row.InServiceDate ?? Clock.Now.ConvertTimeZoneTo(_timeZone).Date,
                SoldDate = row.SoldDate,
                SoldPrice = row.SoldPrice
            };
            _truckRepository.Insert(truck);

            _existingTruckNames.Add(row.TruckCode);
            return true;
        }

        private int? GetVehicleCategory(TruckImportRow row)
        {
            VehicleCategoryDto vehicleCategory;
            var name = row.VehicleCategoryName;

            if (string.IsNullOrEmpty(name))
            {
                vehicleCategory = _vehicleCategories.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).FirstOrDefault();
                if (vehicleCategory == null)
                {
                    row.AddParseErrorIfNotExist(Columns.TruckColumn.Category, "No vehicle categories were found in DB to fallback on", typeof(string));
                }
            }

            vehicleCategory = _vehicleCategories.FirstOrDefault(x => name.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (vehicleCategory != null)
            {
                return vehicleCategory.Id;
            }

            if (name.ToLower().EndsWith('s'))
            {
                vehicleCategory = _vehicleCategories.FirstOrDefault(x => name.TrimEnd('s').Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                if (vehicleCategory != null)
                {
                    return vehicleCategory.Id;
                }
            }

            row.AddParseErrorIfNotExist(Columns.TruckColumn.Category, name, typeof(string));
            return null;
        }

        protected override bool IsRowEmpty(TruckImportRow row)
        {
            return row.TruckCode.IsNullOrEmpty();
        }
    }
}
