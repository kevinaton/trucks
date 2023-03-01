using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Drivers;
using DispatcherWeb.Offices;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DemoData
{
    [AbpAuthorize(AppPermissions.Pages_Tenants)]
    public class DemoDataAppService : DispatcherWebAppServiceBase, IDemoDataAppService
    {
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<FuelPurchase> _fuelPurchaseRepository;
        private readonly IRepository<VehicleUsage> _vehicleUsageRepository;
        private readonly IRepository<TimeClassification> _jobRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;

        public DemoDataAppService(IRepository<Driver> driverRepository,
            IRepository<Truck> truckRepository,
            IRepository<VehicleCategory> vehicleCategoryRepository,
            IRepository<Office> officeRepository,
            IRepository<FuelPurchase> fuelPurchaseRepository,
            IRepository<VehicleUsage> vehicleUsageRepository,
            IRepository<TimeClassification> jobRepository,
            IRepository<User, long> userRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository)
        {
            _driverRepository = driverRepository;
            _truckRepository = truckRepository;
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _officeRepository = officeRepository;
            _fuelPurchaseRepository = fuelPurchaseRepository;
            _vehicleUsageRepository = vehicleUsageRepository;
            _jobRepository = jobRepository;
            _userRepository = userRepository;
            _employeeTimeRepository = employeeTimeRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_AddDemoData)]
        public async Task CreateDemoData(EntityDto input)
        {
            var adminUser = await GetAdminUser(input.Id);
            var lstDrivers = await AddDrivers(input.Id, adminUser.OfficeId);
            var lstTrucks = await AddTrucks(input.Id, lstDrivers);
            await AddFuelPurchases(input.Id, lstTrucks);
            await AddVehicleUsage(input.Id, lstTrucks, ReadingType.Hours);
            await AddVehicleUsage(input.Id, lstTrucks, ReadingType.Miles);
            var lstJobs = await AddTimeClassifications(input.Id);
            await AddEmployeeTime(input.Id, lstJobs.Where(x => x.IsProductionBased).First(), lstDrivers);
        }

        private async Task<User> GetAdminUser(int tenantId)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var user = await _userRepository.GetAll().Where(x => x.TenantId == tenantId && x.UserName.Equals("admin")).FirstOrDefaultAsync();
                return user;
            }
        }

        private async Task<List<Driver>> AddDrivers(int tenantId, int? officeId)
        {
            List<string> lstDriverNames = new List<string>() {
            "Driver-1","Driver-2","Driver-3","Driver-7"
            };
            var driversList = await _driverRepository.GetAll().Where(x => x.TenantId == tenantId && x.FirstName != null
              && lstDriverNames.Contains(x.FirstName)
            ).ToListAsync();
            foreach (var item in lstDriverNames)
            {
                if (driversList.Where(x => x.FirstName.Equals(item)).Count() == 0)
                {
                    //the user should not be assigned directly, instead we should be using DriverUserLinkService.UpdateUser(driver, username, false)
                    //var user = new User() { EmailAddress = item.Replace("-", "").Trim() + "@demo.com", Surname = item.Replace("-", ""), UserName = item.Replace("-", ""), Password = item.Replace("-", ""), IsActive = true, Name = item, OfficeId = officeId, TenantId = tenantId };
                    //await UserManager.CreateAsync(user);

                    var driver = new Driver();
                    driver.FirstName = item;
                    driver.LastName = "Demo";
                    driver.TenantId = tenantId;
                    driver.OfficeId = officeId;
                    //driver.UserId = user.Id; //the user should not be assigned directly, instead we should be using DriverUserLinkService.UpdateUser(driver, username, false)
                    await _driverRepository.InsertAndGetIdAsync(driver);
                }
            }

            driversList = await _driverRepository.GetAll().Where(x => x.TenantId == tenantId && x.FirstName != null
             && lstDriverNames.Contains(x.FirstName)
           ).ToListAsync();

            return driversList;
        }

        private async Task<List<Truck>> AddTrucks(int tenantId, List<Driver> lstDrivers)
        {
            var officesList = await _officeRepository.GetAll().Where(x => x.TenantId == tenantId).ToListAsync();
            var vehicleCategory = _vehicleCategoryRepository.Get(1);
            List<string> lstTruckCodes = new List<string>() {
            "Truck-1","Truck-2","Truck-3"
            };
            var trucksList = await _truckRepository.GetAll().Where(x => x.TenantId == tenantId && x.TruckCode != null
              && lstTruckCodes.Contains(x.TruckCode)
            ).ToListAsync();
            int index = 0;
            foreach (var item in lstTruckCodes)
            {
                foreach (var office in officesList)
                {
                    if (trucksList.Where(x => x.TruckCode.Equals(item) && x.Office != null && x.Office.Id == office.Id).Count() == 0)
                    {

                        Truck entity = new Truck();
                        entity.TruckCode = item;
                        entity.VehicleCategory = vehicleCategory;
                        entity.IsActive = true;
                        entity.DefaultDriver = lstDrivers[index];
                        entity.TenantId = tenantId;
                        entity.LocationId = office.Id;
                        await _truckRepository.InsertOrUpdateAndGetIdAsync(entity);
                    }
                }
                index++;
            }

            trucksList = await _truckRepository.GetAll().Where(x => x.TenantId == tenantId && x.TruckCode != null
              && lstTruckCodes.Contains(x.TruckCode)
            ).ToListAsync();

            return trucksList;
        }

        private async Task AddFuelPurchases(int tenantId, List<Truck> lstTrucks)
        {
            var timezone = await GetTimezone();
            var datetime = System.DateTime.Now.ConvertTimeZoneFrom(timezone);

            for (int i = 0; i < 3; i++)
            {
                if (i > 0)
                    datetime = datetime.AddDays(1);
                foreach (var item in lstTrucks)
                {
                    var obj = await _fuelPurchaseRepository.GetAll().Where(x => x.TenantId == tenantId && x.FuelDateTime.Date == datetime.Date && x.TruckId == item.Id).FirstOrDefaultAsync();
                    if (obj == null)
                    {
                        FuelPurchase entity = new FuelPurchase();
                        entity.TruckId = item.Id;
                        entity.FuelDateTime = datetime;
                        entity.Amount = 100;
                        entity.Rate = 5;
                        entity.TenantId = tenantId;
                        _fuelPurchaseRepository.InsertOrUpdateAndGetId(entity);
                    }
                }
            }
        }

        private async Task AddVehicleUsage(int tenantId, List<Truck> lstTrucks, ReadingType readingType)
        {
            var timezone = await GetTimezone();
            var datetime = System.DateTime.Now.ConvertTimeZoneFrom(timezone);

            int reading = 50;

            for (int i = 0; i < 3; i++)
            {
                if (i > 0)
                    datetime = datetime.AddDays(2);
                foreach (var item in lstTrucks)
                {
                    var obj = await _vehicleUsageRepository.GetAll().Where(x => x.TenantId == tenantId && x.Truck.Id == item.Id && x.ReadingDateTime.Date == datetime.Date && x.ReadingType == readingType).FirstOrDefaultAsync();

                    if (obj == null)
                    {
                        VehicleUsage entity = new VehicleUsage();
                        entity.TruckId = item.Id;
                        entity.ReadingDateTime = datetime;
                        entity.Reading = reading;
                        entity.ReadingType = readingType;
                        entity.TenantId = tenantId;
                        _vehicleUsageRepository.InsertOrUpdateAndGetId(entity);
                        reading += 50;

                        entity = new VehicleUsage();
                        entity.TruckId = item.Id;
                        entity.ReadingDateTime = datetime.AddDays(1);
                        entity.Reading = reading;
                        entity.ReadingType = readingType;
                        entity.TenantId = tenantId;
                        _vehicleUsageRepository.InsertOrUpdateAndGetId(entity);
                        reading += 50;
                    }
                }
            }
        }

        private async Task<List<TimeClassification>> AddTimeClassifications(int tenantId)
        {
            List<string> jobNames = new List<string>() {
            "Drive Truck",
            "Production Pay"
            };
            var lstJobs = await _jobRepository.GetAll().Where(x => x.TenantId == tenantId && x.Name != null && jobNames.Contains(x.Name)).ToListAsync();

            foreach (var job in jobNames)
            {
                if (lstJobs.Where(x => x.Name.Equals(job)).Count() == 0)
                {
                    var entity = new TimeClassification();
                    entity.Name = job;
                    entity.DefaultRate = 5;
                    entity.TenantId = tenantId;
                    if (job.Equals("Production Pay"))
                    {
                        entity.IsProductionBased = true;
                    }
                    await _jobRepository.InsertOrUpdateAndGetIdAsync(entity);
                }
            }
            lstJobs = await _jobRepository.GetAll().Where(x => x.TenantId == tenantId && x.Name != null && jobNames.Contains(x.Name)).ToListAsync();
            return lstJobs;
        }

        private Task AddEmployeeTime(int tenantId, TimeClassification timeClassification, List<Driver> drivers)
        {
            //driver.UserId is not available yet, see above
            return Task.CompletedTask;
            //var timezone = await GetTimezone();
            //foreach (var driver in drivers)
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        var startDateTime = System.DateTime.Now.AddDays(i).ConvertTimeZoneFrom(timezone);
            //        var endDateTime = System.DateTime.Now.AddDays(i).AddHours(6).ConvertTimeZoneFrom(timezone);
            //        var employeeTime = await _employeeTimeRepository.GetAll()
            //                          .Where(x => x.StartDateTime.Date == startDateTime.Date && x.DriverId == driver.Id && x.TimeClassification == timeClassification.Id)
            //                          .FirstOrDefaultAsync();
            //        if (employeeTime == null)
            //        {
            //            var entity = new Drivers.EmployeeTime();
            //            entity.StartDateTime = startDateTime;
            //            entity.EndDateTime = endDateTime;
            //            entity.TimeClassificationId = timeClassification.Id;
            //            entity.Description = "";
            //            entity.TenantId = tenantId;
            //            entity.DriverId = driver.Id;
            //            entity.UserId = driver.UserId ?? 0;
            //            await _employeeTimeRepository.InsertOrUpdateAndGetIdAsync(entity);
            //        }
            //    }
            //}

        }
    }
}
