using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Features;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Orders;
using DispatcherWeb.Services;
using DispatcherWeb.Locations;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trux;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Imports.Services
{
    public class ImportTruxEarningsAppService : ImportDataBaseAppService<TruxImportRow>, IImportTruxEarningsAppService
    {
        private readonly IRepository<TruxEarnings> _truxEarningsRepository;
        private readonly IRepository<TruxEarningsBatch> _truxEarningsBatchRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<UnitOfMeasure> _uomRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<LocationCategory> _locationCategoryRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly UserManager _userManager;
        private string _filePath = null;
        private int? _officeId = null;
        private bool _useShifts;
        private bool _useForProductionPay;
        private int _truxCustomerId;
        private Dictionary<int, string> _uoms;
        private int _serviceId;
        private int _temporaryLocationCategoryId;
        private int _timeClassificationId;
        private TruxEarningsBatch _truxEarningsBatch = null;
        private List<TruxEarnings> _truxEarnings = null;

        public ImportTruxEarningsAppService(
            IRepository<TruxEarnings> truxEarningsRepository,
            IRepository<TruxEarningsBatch> truxEarningsBatchRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<Customer> customerRepository,
            IRepository<UnitOfMeasure> uomRepository,
            IRepository<Service> serviceRepository,
            IRepository<Location> locationRepository,
            IRepository<LocationCategory> locationCategoryRepository,
            IRepository<Truck> truckRepository,
            IRepository<Driver> driverRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            UserManager userManager
        )
        {
            _truxEarningsRepository = truxEarningsRepository;
            _truxEarningsBatchRepository = truxEarningsBatchRepository;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _ticketRepository = ticketRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _customerRepository = customerRepository;
            _uomRepository = uomRepository;
            _serviceRepository = serviceRepository;
            _locationRepository = locationRepository;
            _locationCategoryRepository = locationCategoryRepository;
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _userManager = userManager;
        }

        public bool LogImportWarning(LogImportWarningInput input)
        {
            _tenantId = Session.TenantId ?? 0;
            _userId = Session.UserId ?? 0;
            LogWarning(input.Text + "; Location: " + input.Location);
            return true;
        }

        public ValidateTruxFileResult ValidateFile(string filePath)
        {
            _filePath = filePath;
            _tenantId = Session.TenantId ?? 0;
            _userId = Session.UserId ?? 0;
            try
            {
                var result = new ValidateTruxFileResult
                {
                    IsValid = true
                };
                using (var fileStream = AttachmentHelper.GetStreamFromAzureBlob(filePath))
                using (TextReader textReader = new StreamReader(fileStream))
                {
                    var reader = new ImportReader(textReader, null);

                    var header = string.Join(",", reader.GetCsvHeaders());
                    if (header != "Job Id,Shift/Assignment,Job Name,Start Date,Truck Type,Status,Truck Id,Driver Name,Hauler Name,Punch In Datetime,Punch Out Datetime,Hours,Tons,Loads,Unit,Rate,Total")
                    {
                        LogError("Received header doesn't match the expected header. Received: " + header);
                        throw new UserFriendlyException(L("ThisDoesntLooksLikeTruxFile_PleaseVerifyAndUploadAgain"));
                    }
                }

                using (var fileStream = AttachmentHelper.GetStreamFromAzureBlob(filePath))
                using (TextReader textReader = new StreamReader(fileStream))
                {
                    var reader = new ImportReader(textReader, null);
                    var newTruxEarningsIds = new List<int>();

                    int rowNumber = 0;
                    foreach (var row in reader.AsEnumerable<TruxImportRow>())
                    {
                        rowNumber++;
                        if (!IsRowEmpty(row) && row.ShiftAssignment.HasValue)
                        {
                            newTruxEarningsIds.Add(row.ShiftAssignment.Value);
                        }
                    }
                    result.TotalRecordCount = newTruxEarningsIds.Count;

                    result.DuplicateShiftAssignments = GetDuplicateShiftAssignments(newTruxEarningsIds);
                    if (result.DuplicateShiftAssignments.Any())
                    {
                        LogWarning("Records with same shift/assignment already exist: " + string.Join(", ", result.DuplicateShiftAssignments));
                    }

                    return result;
                }
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception e)
            {
                LogError($"Error in the ImportTruxEarningsAppService.ValidateFile method: {e}");
                throw new UserFriendlyException("Unknown validation error occurred");
            }
        }

        private List<int> GetDuplicateShiftAssignments(List<int> shiftAssignments)
        {
            var result = new List<int>();

            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant))
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                foreach (var idChunk in shiftAssignments.Chunk(900))
                {
                    var ids = idChunk.ToList();
                    var existingIds = _truxEarningsRepository.GetAll()
                        .Where(x => ids.Contains(x.Id))
                        .Select(x => x.Id)
                        .ToList();
                    result.AddRange(existingIds);
                }
            }

            return result;
        }

        protected override bool CacheResourcesBeforeImport(IImportReader reader)
        {
            _filePath = _importJobArgs.File;
            _officeId = _userManager.Users.Where(x => x.Id == _userId).Select(x => x.OfficeId).FirstOrDefault();
            if (_officeId == null)
            {
                _result.NotFoundOffices.Add(_userId.ToString());
                return false;
            }

            _useShifts = SettingManager.GetSettingValueForTenant<bool>(AppSettings.General.UseShifts, _tenantId);

            _useForProductionPay = SettingManager.GetSettingValueForTenant<bool>(AppSettings.Trux.UseForProductionPay, _tenantId) && FeatureChecker.IsEnabled(AppFeatures.DriverProductionPayFeature);
            if (_useForProductionPay)
            {
                _timeClassificationId = _timeClassificationRepository.GetAll().Where(x => x.IsProductionBased).Select(x => x.Id).FirstOrDefault();
                if (_timeClassificationId == 0)
                {
                    LogError("ProductionBased time classification wasn't found");
                    _result.ResourceErrors.Add("ProductionBased time classification wasn't found");
                    return false;
                }
            }
            else
            {
                _timeClassificationId = _timeClassificationRepository.GetAll().Where(x => x.Name == "Drive Truck").Select(x => x.Id).FirstOrDefault();
                if (_timeClassificationId == 0)
                {
                    LogError("'Drive Truck' time classification wasn't found");
                    _result.ResourceErrors.Add("Time classification named 'Drive Truck' wasn't found");
                    return false;
                }
            }
            

            _truxCustomerId = SettingManager.GetSettingValueForTenant<int>(AppSettings.Trux.TruxCustomerId, _tenantId);
            if (!_customerRepository.GetAll().Any(x => x.Id == _truxCustomerId))
            {
                _result.ResourceErrors.Add("Trux Customer wasn't found, please select a Trux Customer in the settings");
                return false;
            }

            _uoms = _uomRepository.GetAll()
                .Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToDictionary(x => x.Id, x => x.Name);

            _serviceId = _serviceRepository.GetAll()
                .Where(x => x.Service1 == "Trux Unknown")
                .Select(x => x.Id)
                .FirstOrDefault();

            if (_serviceId == 0)
            {
                var service = new Service
                {
                    Service1 = "Trux Unknown",
                    IsActive = true
                };
                _serviceRepository.Insert(service);
                CurrentUnitOfWork.SaveChanges();
                _serviceId = service.Id;
            }

            _temporaryLocationCategoryId = _locationCategoryRepository.GetAll()
                .Where(x => x.PredefinedLocationCategoryKind == PredefinedLocationCategoryKind.Temporary)
                .Select(x => x.Id)
                .FirstOrDefault();

            if (_temporaryLocationCategoryId == 0)
            {
                var temporaryLocationCategory = new LocationCategory
                {
                    Name = "Temporary",
                    PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.Temporary
                };
                _locationCategoryRepository.Insert(temporaryLocationCategory);
                CurrentUnitOfWork.SaveChanges();
                _temporaryLocationCategoryId = temporaryLocationCategory.Id;
            }

            _truxEarningsBatch = new TruxEarningsBatch
            {
                FilePath = _importJobArgs.File,
                TenantId = _tenantId,
            };
            _truxEarningsBatchRepository.Insert(_truxEarningsBatch);
            CurrentUnitOfWork.SaveChanges();

            _truxEarnings = new List<TruxEarnings>();

            return base.CacheResourcesBeforeImport(reader);
        }

        protected override void ImportRowAndSave(TruxImportRow row, int rowNumber)
        {
            if (ImportRow(row))
            {
                //_result.ImportedNumber++;
                //CurrentUnitOfWork.SaveChanges();
            }

            WriteRowErrors(row, rowNumber);
        }

        protected override bool ImportRow(TruxImportRow row)
        {
            if (row.Hours == null || row.JobId == null || row.ShiftAssignment == null || row.Loads == null || row.PunchInDatetime == null 
                || row.PunchOutDatetime == null || row.Rate == null || row.StartDateTime == null || row.Tons == null || row.Total == null 
                || row.Unit == null || string.IsNullOrEmpty(row.JobName))
            {
                LogWarning("The row was skipped because one of the required values were empty: " + Infrastructure.Utilities.Utility.Serialize(row));
                return false;
            }

            if (!row.Unit.ToLower().TrimEnd('s').IsIn("ton", "load", "hour"))
            {
                LogWarning("Unexpected Unit: " + row.Unit);
                row.AddParseErrorIfNotExist("Unit", row.Unit, typeof(string));
                return false;
            }

            if (!row.JobName.Contains(" to "))
            {
                LogWarning("Unexpected JobName, ' to ' is missing: " + row.JobName);
                row.AddParseErrorIfNotExist("JobName", row.JobName, typeof(string));
                return false;
            }

            var truxEarnings = new TruxEarnings
            {
                Id = row.ShiftAssignment.Value,
                TenantId = _tenantId,
                CreatorUserId = _userId,
                BatchId = _truxEarningsBatch.Id,
                DriverName = row.DriverName,
                HaulerName = row.HaulerName,
                Hours = row.Hours.Value,
                JobId = row.JobId.Value,
                JobName = row.JobName,
                Loads = row.Loads.Value,
                PunchInDatetime = row.PunchInDatetime.Value,
                PunchOutDatetime = row.PunchOutDatetime.Value,
                Rate = row.Rate.Value,
                StartDateTime = row.StartDateTime.Value,
                Status = row.Status,
                Tons = row.Tons.Value,
                Total = row.Total.Value,
                TruckType = row.TruckType,
                TruxTruckId = row.TruxTruckId,
                Unit = row.Unit,
            };
            _truxEarnings.Add(truxEarnings);
            return true;
        }

        protected override bool IsRowEmpty(TruxImportRow row)
        {
            return !(row.JobId > 0) && !(row.ShiftAssignment > 0);
        }

        protected override bool PostImportTasks()
        {
            if (!_truxEarnings.Any())
            {
                return true;
            }

            var duplicateShiftAssignments = GetDuplicateShiftAssignments(_truxEarnings.Select(x => x.Id).ToList());
            _result.SkippedNumber += duplicateShiftAssignments.Count;

            var addedDriverAssignments = new List<DriverAssignment>();

            foreach (var truxGroup in _truxEarnings.Where(x => !duplicateShiftAssignments.Contains(x.Id)).GroupBy(x => x.JobId))
            {
                using (var unitOfWork = UnitOfWorkManager.Begin())
                {
                    var jobId = truxGroup.Key;
                    var date = truxGroup.First().StartDateTime.ConvertTimeZoneTo(_timeZone).Date;

                    var order = new Order
                    {
                        DeliveryDate = date,
                        CustomerId = _truxCustomerId,
                        CreatorUserId = _userId,
                        LocationId = _officeId.Value,
                        TenantId = _tenantId,
                        IsClosed = true,
                        IsImported = true,
                        Shift = _useShifts ? Shift.Shift1 : (Shift?)null
                    };
                    int nextOrderLineNumber;
                    var existingOrder = _orderRepository.GetAll()
                        .Include(x => x.OrderLines)
                        .Where(x => x.DeliveryDate == order.DeliveryDate
                            && x.CustomerId == order.CustomerId
                            && x.LocationId == order.LocationId
                            && x.IsClosed
                            && x.IsImported).FirstOrDefault();
                    if (existingOrder == null)
                    {
                        nextOrderLineNumber = 1;
                        _orderRepository.Insert(order);
                    }
                    else
                    {
                        nextOrderLineNumber = existingOrder.OrderLines.Count() + 1;
                        order = existingOrder;
                    }


                    foreach (var row in truxGroup)
                    {
                        _truxEarningsRepository.Insert(row);
                        _result.ImportedNumber++;

                        //var isFreight = row.Unit.ToLower() == "ton";
                        var isFreight = true; //per #11360, the orders being processed via the Trux import will always be freight only.
                        var orderLine = new OrderLine
                        {
                            Order = order,
                            TenantId = _tenantId,
                            CreatorUserId = _userId,
                            LineNumber = nextOrderLineNumber++,
                            MaterialQuantity = isFreight ? (decimal?)null : GetQuantity(row),
                            FreightQuantity = isFreight ? GetQuantity(row) : (decimal?)null,
                            ServiceId = _serviceId,
                            MaterialPricePerUnit = isFreight ? (decimal?)null : row.Rate,
                            FreightPricePerUnit = isFreight ? row.Rate : (decimal?)null,
                            MaterialPrice = isFreight ? 0 : row.Total,
                            FreightPrice = isFreight ? row.Total : 0,
                            MaterialUomId = isFreight ? (int?)null : GetUomId(row),
                            FreightUomId = isFreight ? GetUomId(row): (int?)null,
                            Designation = isFreight ? DesignationEnum.FreightOnly : DesignationEnum.MaterialOnly,
                            LoadAtId = GetLoadAtId(row),
                            DeliverToId = GetDeliverToId(row),
                            JobNumber = jobId.ToString(),
                            IsComplete = true,
                            NumberOfTrucks = 1,
                            ProductionPay = _useForProductionPay && isFreight,
                        };
                        _orderLineRepository.Insert(orderLine);

                        var truck = _truckRepository.GetAll()
                            .Where(x => x.TruxTruckId == row.TruxTruckId)
                            .Select(x => new
                            {
                                x.Id,
                                x.TruckCode
                            }).FirstOrDefault();

                        var driver = _driverRepository.GetAll()
                            .Where(x => x.FirstName + " " + x.LastName == row.DriverName)
                            .Select(x => new
                            {
                                x.Id,
                                x.UserId
                            }).FirstOrDefault();

                        if (driver == null)
                        {
                            AddResourceError($"Driver {row.DriverName} wasn’t found. You’ll need to fix the driver on the ticket view");
                        }
                        else if (driver.UserId == null)
                        {
                            AddResourceError($"Driver {row.DriverName} doesn't have a user linked. Employee Time records won't be created");
                        }

                        var ticket = new Ticket
                        {
                            OrderLine = orderLine,
                            TenantId = _tenantId,
                            CreatorUserId = _userId,
                            TicketNumber = row.Id.ToString(),
                            Quantity = GetQuantity(row),
                            TruckId = truck?.Id,
                            TruckCode = truck?.TruckCode ?? row.TruxTruckId,
                            CustomerId = order.CustomerId,
                            TicketDateTime = row.StartDateTime,
                            ServiceId = orderLine.ServiceId,
                            UnitOfMeasureId = GetUomId(row),
                            OfficeId = _officeId,
                            DriverId = driver?.Id,
                            DeliverToId = orderLine.DeliverToId,
                            LoadAtId = orderLine.LoadAtId,
                            IsImported = true,
                            IsBilled = true
                        };
                        _ticketRepository.Insert(ticket);

                        if (driver?.UserId != null)
                        {
                            var employeeTime = new Drivers.EmployeeTime
                            {
                                TenantId = _tenantId,
                                UserId = driver.UserId.Value,
                                StartDateTime = row.PunchInDatetime,
                                EndDateTime = row.PunchOutDatetime,
                                TimeClassificationId = _timeClassificationId,
                                EquipmentId = truck?.Id,
                                DriverId = driver.Id,
                                IsImported = true
                            };
                            _employeeTimeRepository.Insert(employeeTime);
                        }

                        if (truck != null && driver != null)
                        {
                            var driverAssignment = new DriverAssignment
                            {
                                Date = date,
                                DriverId = driver.Id,
                                OfficeId = _officeId,
                                Shift = order.Shift,
                                StartTime = null,
                                TruckId = truck.Id,
                                TenantId = _tenantId,
                            };

                            var existing = addedDriverAssignments
                                .FirstOrDefault(x => x.Date == driverAssignment.Date
                                                    && x.TruckId == driverAssignment.TruckId
                                                    && x.Shift == driverAssignment.Shift
                                );

                            if (existing == null)
                            {
                                existing = _driverAssignmentRepository
                                    .FirstOrDefault(x => x.Date == driverAssignment.Date
                                                        && x.TruckId == driverAssignment.TruckId
                                                        && x.Shift == driverAssignment.Shift
                                    );
                            }
                            
                            if (existing == null)
                            {
                                _driverAssignmentRepository.Insert(driverAssignment);
                                addedDriverAssignments.Add(driverAssignment);
                            }
                            else
                            {
                                if (existing.DriverId != driverAssignment.DriverId || existing.OfficeId != driverAssignment.OfficeId)
                                {
                                    AddResourceError($"Driver assignment for truck {truck.TruckCode} on {date:d} already exists with a driver or office different than expected (Expected driver: {row.DriverName}, officeId: {_officeId})");
                                }
                            }
                        }

                        if (truck == null)
                        {
                            AddResourceError($"Truck with TruxId {row.TruxTruckId} wasn’t found. You’ll need to add the OrderLineTruck on the schedule view");
                        }
                        else
                        {
                            var orderLineTruck = new OrderLineTruck
                            {
                                IsDone = true,
                                OrderLine = orderLine,
                                TenantId = _tenantId,
                                TruckId = truck.Id,
                                DriverId = driver?.Id,
                            };
                            _orderLineTruckRepository.Insert(orderLineTruck);
                        }
                    }

                    CurrentUnitOfWork.SaveChanges();
                    unitOfWork.Complete();
                }
            }

            return true;
        }

        private int GetUomId(TruxEarnings row)
        {
            foreach (var uom in _uoms)
            {
                if (uom.Value.Equals(row.Unit, StringComparison.InvariantCultureIgnoreCase))
                {
                    return uom.Key;
                }
            }

            foreach (var uom in _uoms)
            {
                if (uom.Value.ToLower().TrimEnd('s').Equals(row.Unit.ToLower().TrimEnd('s'), StringComparison.InvariantCultureIgnoreCase))
                {
                    return uom.Key;
                }
            }

            return _uoms.First().Key;
        }

        private decimal GetQuantity(TruxEarnings row)
        {
            switch (row.Unit.ToLower().TrimEnd('s'))
            {
                case "ton": return row.Tons;
                case "hour": return row.Hours;
                case "load": return row.Loads;
            }
            return row.Tons;
        }

        private int? GetLoadAtId(TruxEarnings row)
        {
            var parts = row.JobName.Split(" to ").SkipLast(1);
            int? locationId;
            
            if (parts.Count() > 1)
            {
                locationId = GetLocationIdByNameOrNull(string.Join(" to ", parts));
                if (locationId != null)
                {
                    return locationId;
                }
            }
            
            locationId = GetLocationIdByNameOrNull(parts.First());
            if (locationId != null)
            {
                return locationId;
            }

            return CreateLocationByName(parts.First());
        }

        private int? GetDeliverToId(TruxEarnings row)
        {
            var parts = row.JobName.Split(" to ").Skip(1);
            int? locationId;
            
            locationId = GetLocationIdByNameOrNull(parts.Last());
            if (locationId != null)
            {
                return locationId;
            }

            if (parts.Count() > 1)
            {
                locationId = GetLocationIdByNameOrNull(string.Join(" to ", parts));
                if (locationId != null)
                {
                    return locationId;
                }
            }

            return CreateLocationByName(parts.Last());
        }

        private int? GetLocationIdByNameOrNull(string name)
        {
            return _locationRepository.GetAll()
                .Where(x => x.Name == name)
                .Select(x => new
                {
                    x.Id
                }).FirstOrDefault()?.Id;
        }

        private int? CreateLocationByName(string name)
        {
            var location = new Location
            {
                Name = name,
                IsActive = true,
                CategoryId = _temporaryLocationCategoryId
            };
            _locationRepository.Insert(location);
            CurrentUnitOfWork.SaveChanges();
            return location.Id;
        }

        private void LogWarning(string text)
        {
            Logger.Warn($"Trux Earnings Import warning (tenantId: {_tenantId}, userId: {_userId}, file:{_filePath}): " + text);
        }

        private void LogError(string text)
        {
            Logger.Error($"Trux Earnings Import error (tenantId: {_tenantId}, userId: {_userId}, file:{_filePath}): " + text);
        }
    }
}
