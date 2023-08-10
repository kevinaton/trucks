﻿using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Abp.Web.Models;
using DispatcherWeb.DriverApp.TruckPositions.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.AzureTables;
using DispatcherWeb.TruckPositions;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DispatcherWeb.DriverApp.TruckPositions
{
    [AbpAuthorize]
    public class TruckPositionAppService : DispatcherWebDriverAppAppServiceBase, ITruckPositionAppService
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IAzureTableManager _azureTableManager;

        public TruckPositionAppService(
            IRepository<Truck> truckRepository,
            IRepository<Driver> driverRepository,
            IAzureTableManager azureTableManager
            )
        {
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _azureTableManager = azureTableManager;
        }

        [WrapResult(false)]
        [AbpAllowAnonymous]
        public async Task Post(TruckPositionRawDto input)
        {
            if (input.DriverId == null && input.UserId == null)
            {
                Logger.Error("TruckPosition.Post: DriverId and UserId values are missing: " + JsonConvert.SerializeObject(input));
                throw new UserFriendlyException("DriverId or UserId value is missing. You should provide at least one of them.");
            }

            if (input.Location == null)
            {
                Logger.Error("TruckPosition.Post: Location value is missing: " + JsonConvert.SerializeObject(input));
                throw new UserFriendlyException("Location value is missing");
            }

            if (input.AdditionalData != null && !string.IsNullOrEmpty(input.Json))
            {
                Logger.Warn($"TruckPosition.Post: Unexpected Params fields: " + JsonConvert.SerializeObject(input));
            }

            var driver = await _driverRepository.GetAll()
                .WhereIf(input.UserId.HasValue, x => x.UserId == input.UserId)
                .WhereIf(input.UserId == null, x => x.Id == input.DriverId)
                .Select(x => new
                {
                    x.Id,
                    x.TenantId,
                    x.IsInactive
                })
                .OrderByDescending(x => !x.IsInactive)
                .FirstOrDefaultAsync();

            if (driver == null)
            {
                Logger.Error($"TruckPosition.Post: Driver with id '{input.DriverId}' or UserId '{input.UserId}' wasn't found: " + JsonConvert.SerializeObject(input));
                throw new UserFriendlyException($"Driver with id '{input.DriverId}' (or UserId '{input.UserId}') wasn't found");
            }

            if (input.TruckId == null)
            {
                Logger.Warn("TruckPosition.Post: No TruckId was provided: " + JsonConvert.SerializeObject(input));
                return;
            }

            var truck = await _truckRepository.GetAll()
                .Where(x => x.Id == input.TruckId)
                .Select(x => new
                {
                    x.DtdTrackerUniqueId
                }).FirstOrDefaultAsync();

            if (truck == null)
            {
                Logger.Error($"TruckPosition.Post: Truck with id {input.TruckId} wasn't found: " + JsonConvert.SerializeObject(input));
                throw new UserFriendlyException($"Truck with id {input.TruckId} wasn't found");
            }

            if (string.IsNullOrEmpty(truck.DtdTrackerUniqueId))
            {
                Logger.Error($"TruckPosition.Post: Truck with id {input.TruckId} doesn't have DtdTrackerUniqueId filled. Request: " + JsonConvert.SerializeObject(input));
                return;
            }

            var truckPositionTableClient = await _azureTableManager.GetTruckPositionTableClient();

            foreach (var location in input.Location)
            {
                if (location.Extras?.AdditionalData != null && !string.IsNullOrEmpty(location.Extras?.Json))
                {
                    Logger.Warn($"TruckPosition.Post: Unexpected Extras fields: " + JsonConvert.SerializeObject(location));
                }

                var truckPosition = new TruckPosition
                {
                    TruckId = input.TruckId.Value,
                    DriverId = driver.Id,
                    TenantId = driver.TenantId,
                    DtdTrackerUniqueId = truck.DtdTrackerUniqueId,
                    Latitude = location.Coordinates?.Latitude,
                    Longitude = location.Coordinates?.Longitude,
                    Accuracy = location.Coordinates?.Accuracy,
                    Speed = location.Coordinates?.Speed,
                    Heading = location.Coordinates?.Heading,
                    Altitude = location.Coordinates?.Altitude,
                    ActivityType = location.Activity?.Type ?? 0,
                    ActivitiTypeRaw = location.Activity?.TypeRaw,
                    ActivityConfidence = location.Activity?.Confidence,
                    GeofenceIdentifier = location.Geofence?.Identifier,
                    GeofenceAction = location.Geofence?.Action,
                    GeofenceActionRaw = location.Geofence?.ActionRaw,
                    BatteryLevel = location.Battery?.Level,
                    BatteryIsCharging = location.Battery?.IsCharging,
                    GpsTimestamp = location.TimeStamp,
                    Uuid = location.Uuid,
                    Event = location.Event,
                    EventRaw = location.EventRaw,
                    IsMoving = location.IsMoving,
                    Odometer = location.Odometer,
                };
                await truckPositionTableClient.UpsertEntityAsync(truckPosition);
            }
        }
    }
}