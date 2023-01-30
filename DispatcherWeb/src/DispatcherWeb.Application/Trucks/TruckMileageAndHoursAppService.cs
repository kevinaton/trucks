using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Features;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Notifications;
using Abp.Configuration;
using Abp.Threading;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.BackgroundJobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Telematics;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Notifications;
using Microsoft.EntityFrameworkCore;
using DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Trucks.Dto;
using Abp.Extensions;
using DispatcherWeb.Infrastructure.Utilities;
using DispatcherWeb.Orders;

namespace DispatcherWeb.Trucks
{
    public class TruckMileageAndHoursAppService : DispatcherWebAppServiceBase, ITruckMileageAndHoursAppService
    {        
        private readonly IRepository<VehicleUsage> _vehicleUsageRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Tenant> _tenantRepository;        

        public TruckMileageAndHoursAppService(            
            IRepository<VehicleUsage> vehicleUsageRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Tenant> tenantRepository            
        )
        {                        
            _vehicleUsageRepository = vehicleUsageRepository;
            _orderLineRepository = orderLineRepository;
            _tenantRepository = tenantRepository;            
        }

        [RemoteService(false)]
        [UnitOfWork]
        public async Task AddTrucksMileageAndHourForDayBeforeTickets()
        {
            var demoTenant = await _tenantRepository.GetAll()
                .Where(t => t.IsActive && t.Name != null && t.TenancyName != null && t.Name.Equals("demo") && t.TenancyName.Equals("demo"))
                .FirstAsync();

            if (demoTenant != null)
            {
                using (CurrentUnitOfWork.SetTenantId(demoTenant.Id))
                using (AbpSession.Use(demoTenant.Id, null))
                {
                    bool isGPSConfigured = false;
                    if (await FeatureChecker.IsEnabledAsync(demoTenant.Id, AppFeatures.GpsIntegrationFeature))
                    {
                        isGPSConfigured = true;
                    }
                    var platform = (GpsPlatform)await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.GpsIntegration.Platform, demoTenant.Id);
                    if (platform == GpsPlatform.Geotab)
                    {
                        var geotabSettings = await SettingManager.GetGeotabSettingsAsync();
                        if (geotabSettings.IsEmpty())
                        {
                            Logger.Warn($"There are no geotab settings for TenantId={demoTenant.Id}");
                            isGPSConfigured = false;
                        }
                        else
                            isGPSConfigured = true;
                    }

                    if (platform == GpsPlatform.Samsara)
                    {
                        var samsaraSettings = await SettingManager.GetSamsaraSettingsAsync();
                        if (samsaraSettings.IsEmpty())
                        {
                            Logger.Warn($"There are no samsara settings for TenantId={demoTenant.Id}");
                            isGPSConfigured = false;
                        }
                        else
                            isGPSConfigured = true;
                    }

                    if (platform == GpsPlatform.DtdTracker)
                    {
                        var dtdTrackerSettings = await SettingManager.GetDtdTrackerSettingsAsync();
                        if (dtdTrackerSettings.IsEmpty())
                        {
                            Logger.Warn($"There are no DTD Tracker settings for TenantId={demoTenant.Id}");
                            isGPSConfigured = false;
                        }
                        else
                            isGPSConfigured = true;
                    }

                    if (isGPSConfigured)
                    {
                        Logger.Info($"Updating mileage and hours for TenantId={demoTenant.Id}");

                        var timezone = await GetTimezone();
                        var readingDateTime = DateTime.Now.AddDays(-2).ConvertTimeZoneFrom(timezone);
                        //var readingDateTime = new DateTime(2022, 06, 07); // For Testing Purpose

                        var trucksWithTotalTicketsCountList = await _orderLineRepository.GetAll()
                        .SelectMany(ol => ol.Tickets)
                        .Where(t => t.TruckId != null && t.CarrierId == null && t.TicketDateTime != null && t.TicketDateTime.Value.Date == readingDateTime.Date)
                        .GroupBy(x => x.TruckId)
                        .Select(x => new
                        {
                            TruckID = x.Key ?? 0,
                            TotalTickets = x.Count()
                        })
                        .ToListAsync();

                        int defaultMileage = 20;
                        int defaultHours = 2;

                        foreach (var truck in trucksWithTotalTicketsCountList)
                        {
                            AddVehicleUsageRecord(truck.TruckID, defaultMileage * truck.TotalTickets, defaultHours * truck.TotalTickets, readingDateTime);
                        }

                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

                // Local functions
                void AddVehicleUsageRecord(int truckId, double mileage, double hours, DateTime readingDateTime)
                {
                    readingDateTime = new DateTime(readingDateTime.Year, readingDateTime.Month, readingDateTime.Day, 09, 00, 00);
                    readingDateTime = DateTime.SpecifyKind(readingDateTime, DateTimeKind.Utc);
                    _vehicleUsageRepository.Insert(new VehicleUsage()
                    {
                        TruckId = truckId,
                        ReadingDateTime = readingDateTime,
                        ReadingType = ReadingType.Miles,
                        Reading = (decimal)mileage,
                    });
                    _vehicleUsageRepository.Insert(new VehicleUsage()
                    {
                        TruckId = truckId,
                        ReadingDateTime = readingDateTime,
                        ReadingType = ReadingType.Hours,
                        Reading = (decimal)hours,
                    });
                }
            }      
        }      
    }
}
