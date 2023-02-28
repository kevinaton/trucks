﻿using System.Linq;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Runtime.Validation;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    [RemoteService(false)]
    public class UpdateTruckFromImportAppService : DispatcherWebAppServiceBase, IUpdateTruckFromImportAppService
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<VehicleUsage> _vehicleUsageRepository;

        public UpdateTruckFromImportAppService(
            IRepository<Truck> truckRepository,
            IRepository<VehicleUsage> vehicleUsageRepository
        )
        {
            _truckRepository = truckRepository;
            _vehicleUsageRepository = vehicleUsageRepository;
        }

        [DisableValidation]
        public void UpdateMileageAndHours(
            int tenantId,
            long userId
        )
        {
            CurrentUnitOfWork.SetTenantId(tenantId);
            using (AbpSession.Use(tenantId, userId))
            {
                var trucks = _truckRepository.GetAll()
                    .Where(t => t.VehicleUsages.Any(vu =>
                            vu.ReadingType == ReadingType.Miles && t.CurrentMileage < vu.Reading ||
                            vu.ReadingType == ReadingType.Hours && t.CurrentHours < vu.Reading
                        )
                    )
                    .ToList();
                var readings = _vehicleUsageRepository.GetAll()
                    .Where(vu => vu.ReadingType == ReadingType.Miles && vu.Truck.CurrentMileage < vu.Reading ||
                                 vu.ReadingType == ReadingType.Hours && vu.Truck.CurrentHours < vu.Reading
                    )
                    .GroupBy(vu => new { vu.TruckId, vu.ReadingType })
                    .Select(g => g.OrderByDescending(vu => vu.ReadingDateTime).FirstOrDefault())
                    .AsNoTracking()
                    .ToList();
                foreach (var vehicleUsage in readings)
                {
                    var truck = trucks.First(t => t.Id == vehicleUsage.TruckId);
                    switch (vehicleUsage.ReadingType)
                    {
                        case ReadingType.Miles:
                            truck.CurrentMileage = (int)vehicleUsage.Reading;
                            break;
                        case ReadingType.Hours:
                            truck.CurrentHours = vehicleUsage.Reading;
                            break;
                    }
                }
            }
        }
    }
}
