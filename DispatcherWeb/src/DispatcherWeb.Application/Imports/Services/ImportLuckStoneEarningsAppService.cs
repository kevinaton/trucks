using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.Features;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Locations;
using DispatcherWeb.LuckStone;
using DispatcherWeb.Orders;
using DispatcherWeb.Services;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    public class ImportLuckStoneEarningsAppService : ImportDataBaseAppService<LuckStoneImportRow>, IImportLuckStoneEarningsAppService
    {
        private readonly IRepository<LuckStoneEarnings> _luckStoneEarningsRepository;
        private readonly IRepository<LuckStoneEarningsBatch> _luckStoneEarningsBatchRepository;
        private readonly IRepository<LuckStoneLocation> _luckStoneLocationRepository;
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
        private Shift? _shift;
        private bool _useForProductionPay;
        private int _luckStoneCustomerId;
        private string _haulerRef;
        private Dictionary<string, int> _loadAtLocations;
        private Dictionary<string, int> _deliverToLocations;
        private Dictionary<string, int> _services;
        private Dictionary<int, string> _uoms;
        private Dictionary<(int truckId, DateTime date), int?> _driversForTrucks;
        private int _temporaryLocationCategoryId;
        private int _timeClassificationId;
        private LuckStoneEarningsBatch _luckStoneEarningsBatch = null;
        private List<LuckStoneEarnings> _luckStoneEarnings = null;

        public ImportLuckStoneEarningsAppService(
            IRepository<LuckStoneEarnings> luckStoneEarningsRepository,
            IRepository<LuckStoneEarningsBatch> luckStoneEarningsBatchRepository,
            IRepository<LuckStoneLocation> luckStoneLocationRepository,
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
            _luckStoneEarningsRepository = luckStoneEarningsRepository;
            _luckStoneEarningsBatchRepository = luckStoneEarningsBatchRepository;
            _luckStoneLocationRepository = luckStoneLocationRepository;
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

        public ValidateLuckStoneFileResult ValidateFile(string filePath)
        {
            _filePath = filePath;
            _tenantId = Session.TenantId ?? 0;
            _userId = Session.UserId ?? 0;
            try
            {
                var result = new ValidateLuckStoneFileResult
                {
                    IsValid = true
                };
                using (var fileStream = AttachmentHelper.GetStreamFromAzureBlob(filePath))
                using (TextReader textReader = new StreamReader(fileStream))
                {
                    var reader = new ImportReader(textReader, null);

                    var header = string.Join(",", reader.GetCsvHeaders());
                    //if (header != "Job Id,Shift/Assignment,Job Name,Start Date,Truck Type,Status,Truck Id,Driver Name,Hauler Name,Punch In Datetime,Punch Out Datetime,Hours,Tons,Loads,Unit,Rate,Total")
                    if (header != "Haultickets_TicketDateTime,Haultickets_HaulerRef,Haultickets_Licenseplate,Haultickets_Site,Haultickets_ProductDescription,Haultickets_CustomerName,Haultickets_TicketID,Haultickets_HaulPaymentRate,Haultickets_HaulPaymentRateUOM,Haultickets_NetTons,Haultickets_FSCAmount,Haultickets_HaulPayment")
                    {
                        LogError("Received header doesn't match the expected header. Received: " + header);
                        throw new UserFriendlyException(L("ThisDoesntLooksLikeLuckStoneFile_PleaseVerifyAndUploadAgain"));
                    }
                }

                using (var fileStream = AttachmentHelper.GetStreamFromAzureBlob(filePath))
                using (TextReader textReader = new StreamReader(fileStream))
                {
                    var reader = new ImportReader(textReader, null);
                    var newLuckStoneEarningsIds = new List<int>();

                    int rowNumber = 0;
                    foreach (var row in reader.AsEnumerable<LuckStoneImportRow>())
                    {
                        rowNumber++;
                        if (!IsRowEmpty(row) && row.LuckStoneTicketId.HasValue)
                        {
                            newLuckStoneEarningsIds.Add(row.LuckStoneTicketId.Value);
                        }
                    }
                    result.TotalRecordCount = newLuckStoneEarningsIds.Count;

                    result.DuplicateLuckStoneTicketIds = GetDuplicateLuckStoneTicketIds(newLuckStoneEarningsIds);
                    if (result.DuplicateLuckStoneTicketIds.Any())
                    {
                        LogWarning("Records with same shift/assignment already exist: " + string.Join(", ", result.DuplicateLuckStoneTicketIds));
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
                LogError($"Error in the ImportLuckStoneEarningsAppService.ValidateFile method: {e}");
                throw new UserFriendlyException("Unknown validation error occurred");
            }
        }

        private List<int> GetDuplicateLuckStoneTicketIds(List<int> luckStoneTicketIds)
        {
            var result = new List<int>();

            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant))
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                foreach (var idChunk in luckStoneTicketIds.Chunk(900))
                {
                    var ids = idChunk.ToList();
                    var existingIds = _luckStoneEarningsRepository.GetAll()
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
            _shift = _useShifts ? Shift.Shift1 : (Shift?)null;

            _useForProductionPay = SettingManager.GetSettingValueForTenant<bool>(AppSettings.LuckStone.UseForProductionPay, _tenantId) && FeatureChecker.IsEnabled(AppFeatures.DriverProductionPayFeature);
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


            _luckStoneCustomerId = SettingManager.GetSettingValueForTenant<int>(AppSettings.LuckStone.LuckStoneCustomerId, _tenantId);
            if (!_customerRepository.GetAll().Any(x => x.Id == _luckStoneCustomerId))
            {
                _result.ResourceErrors.Add("Luck Stone Customer wasn't found, please select a Luck Stone Customer in the settings");
                return false;
            }

            _haulerRef = SettingManager.GetSettingValueForTenant(AppSettings.LuckStone.HaulerRef, _tenantId);

            _uoms = _uomRepository.GetAll()
                .Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToDictionary(x => x.Id, x => x.Name);

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

            _luckStoneEarningsBatch = new LuckStoneEarningsBatch
            {
                FilePath = _importJobArgs.File,
                TenantId = _tenantId,
            };
            _luckStoneEarningsBatchRepository.Insert(_luckStoneEarningsBatch);
            CurrentUnitOfWork.SaveChanges();

            _luckStoneEarnings = new List<LuckStoneEarnings>();

            return base.CacheResourcesBeforeImport(reader);
        }

        protected override void ImportRowAndSave(LuckStoneImportRow row, int rowNumber)
        {
            if (ImportRow(row))
            {
                //_result.ImportedNumber++;
                //CurrentUnitOfWork.SaveChanges();
            }

            if (_haulerRef != row.HaulerRef)
            {
                AddResourceError($"On importing LuckStone work we found a HaulerRef {row.HaulerRef} on line {rowNumber} that doesn’t agree with your system settings. We added the entry, but you should review the record and your “LuckStone” settings to verify these entries are correct.");
            }

            WriteRowErrors(row, rowNumber);
        }

        protected override bool ImportRow(LuckStoneImportRow row)
        {
            if (row.LuckStoneTicketId == null || row.TicketDateTime == null || string.IsNullOrEmpty(row.Site)
                || string.IsNullOrEmpty(row.CustomerName) || string.IsNullOrEmpty(row.LicensePlate)
                || row.HaulPaymentRate == null || row.NetTons == null || row.HaulPayment == null
                || row.HaulerRef == null || row.FscAmount == null || row.HaulPaymentRateUom == null || string.IsNullOrEmpty(row.ProductDescription))
            {
                LogWarning("The row was skipped because one of the required values were empty: " + Infrastructure.Utilities.Utility.Serialize(row));
                return false;
            }

            var uomId = GetUomId(row.HaulPaymentRateUom);
            if (uomId == null)
            {
                LogWarning("Unexpected UOM: " + row.HaulPaymentRateUom);
                row.AddParseErrorIfNotExist(nameof(row.HaulPaymentRateUom), row.HaulPaymentRateUom, typeof(string));
                //don't return false to stop, continue with the import instead and leave the field empty
                //return false;
            }

            var luckStoneEarnings = new LuckStoneEarnings
            {
                Id = row.LuckStoneTicketId.Value,
                TenantId = _tenantId,
                CreatorUserId = _userId,
                BatchId = _luckStoneEarningsBatch.Id,
                TicketDateTime = row.TicketDateTime.Value.ConvertTimeZoneFrom(_timeZone),
                Site = row.Site,
                HaulerRef = row.HaulerRef,
                CustomerName = row.CustomerName,
                LicensePlate = row.LicensePlate,
                HaulPaymentRate = row.HaulPaymentRate.Value,
                NetTons = row.NetTons.Value,
                HaulPayment = row.HaulPayment.Value,
                HaulPaymentRateUom = row.HaulPaymentRateUom,
                FscAmount = row.FscAmount.Value,
                ProductDescription = row.ProductDescription,
            };
            _luckStoneEarnings.Add(luckStoneEarnings);
            return true;
        }

        protected override bool IsRowEmpty(LuckStoneImportRow row)
        {
            return !(row.LuckStoneTicketId > 0);
        }

        protected override bool PostImportTasks()
        {
            if (!_luckStoneEarnings.Any())
            {
                return true;
            }

            var duplicateLuckStoneTicketIds = GetDuplicateLuckStoneTicketIds(_luckStoneEarnings.Select(x => x.Id).ToList());
            _result.SkippedNumber += duplicateLuckStoneTicketIds.Count;

            var licensePlates = _luckStoneEarnings.Select(x => x.LicensePlate).Distinct().ToList();
            var trucks = _truckRepository.GetAll()
                .Where(x => licensePlates.Contains(x.Plate))
                .Select(x => new { x.Id, LicensePlate = x.Plate, x.TruckCode, x.LocationId })
                .ToList();

            PopulateDriversForTrucks(
                trucks.Select(x => x.Id).Distinct().ToList(),
                _luckStoneEarnings.Select(x => x.TicketDateTime.ConvertTimeZoneTo(_timeZone).Date).Distinct().ToList()
            );

            var driverIds = _driversForTrucks.Values.Distinct().ToList();
            var drivers = _driverRepository.GetAll()
                .Where(x => driverIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.IsInactive
                })
                .OrderByDescending(x => !x.IsInactive)
                .ToList();

            PopulateLuckStoneLocationsFromSites(_luckStoneEarnings.Select(x => x.Site).Distinct().ToList());
            PopulateDeliverToLocationsByNames(_luckStoneEarnings.Select(x => x.CustomerName).Distinct().ToList());
            PopulateServices(_luckStoneEarnings.Select(x => x.ProductDescription).Distinct().ToList());

            foreach (var rowGroup in _luckStoneEarnings
                .Where(x => !duplicateLuckStoneTicketIds.Contains(x.Id))
                .GroupBy(x => new { x.TicketDateTime, x.CustomerName, x.Site, x.ProductDescription, x.HaulPaymentRateUom }))
            {
                using (var unitOfWork = UnitOfWorkManager.Begin())
                {
                    var date = rowGroup.Key.TicketDateTime.ConvertTimeZoneTo(_timeZone).Date;

                    var order = new Order
                    {
                        DeliveryDate = date,
                        CustomerId = _luckStoneCustomerId,
                        CreatorUserId = _userId,
                        LocationId = _officeId.Value,
                        TenantId = _tenantId,
                        IsClosed = true,
                        IsImported = true,
                        Shift = _shift
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

                    var orderLineTotal = rowGroup.Sum(x => x.HaulPayment);
                    var orderLine = new OrderLine
                    {
                        Order = order,
                        TenantId = _tenantId,
                        CreatorUserId = _userId,
                        LineNumber = nextOrderLineNumber++,
                        MaterialQuantity = null,
                        FreightQuantity = rowGroup.Sum(x => x.NetTons),
                        ServiceId = _services[rowGroup.Key.ProductDescription.ToLower()],
                        MaterialPricePerUnit = null,
                        FreightPricePerUnit = rowGroup.Average(x => x.HaulPaymentRate),
                        FreightRateToPayDrivers = rowGroup.Average(x => x.HaulPaymentRate),
                        MaterialPrice = 0,
                        FreightPrice = rowGroup.Sum(x => x.HaulPayment),
                        MaterialUomId = null,
                        FreightUomId = GetUomId(rowGroup.Key.HaulPaymentRateUom),
                        Designation = DesignationEnum.FreightOnly,
                        LoadAtId = _loadAtLocations[rowGroup.Key.Site.ToLower()],
                        DeliverToId = _deliverToLocations[rowGroup.Key.CustomerName.ToLower()],
                        FuelSurchargeRate = rowGroup.Sum(x => x.FscAmount) / (orderLineTotal == 0 ? null : orderLineTotal),
                        IsComplete = true,
                        NumberOfTrucks = 1,
                        ProductionPay = _useForProductionPay,
                    };
                    _orderLineRepository.Insert(orderLine);

                    var addedOrderLineTrucks = new List<OrderLineTruck>();

                    foreach (var row in rowGroup)
                    {
                        _luckStoneEarningsRepository.Insert(row);
                        _result.ImportedNumber++;

                        var truck = trucks.FirstOrDefault(x => x.LicensePlate.ToLower() == row.LicensePlate.ToLower());
                        var driverId = truck == null ? null : _driversForTrucks[(truck.Id, date)];
                        var driver = drivers.FirstOrDefault(x => x.Id == driverId);

                        //var driver = _driverRepository.GetAll()
                        //    .Where(x => x.FirstName + " " + x.LastName == row.DriverName)
                        //    .Select(x => new
                        //    {
                        //        x.Id,
                        //        x.UserId
                        //    }).FirstOrDefault();

                        //if (driver == null)
                        //{
                        //    AddResourceError($"Driver {row.DriverName} wasn’t found. You’ll need to fix the driver on the ticket view");
                        //}
                        //else if (driver.UserId == null)
                        //{
                        //    AddResourceError($"Driver {row.DriverName} doesn't have a user linked. Employee Time records won't be created");
                        //}

                        var ticket = new Ticket
                        {
                            OrderLine = orderLine,
                            TenantId = _tenantId,
                            CreatorUserId = _userId,
                            TicketNumber = row.Id.ToString(),
                            Quantity = row.NetTons,
                            FuelSurcharge = row.FscAmount,
                            TruckId = truck?.Id,
                            TruckCode = truck?.TruckCode,
                            CustomerId = order.CustomerId,
                            TicketDateTime = row.TicketDateTime,
                            ServiceId = orderLine.ServiceId,
                            UnitOfMeasureId = orderLine.FreightUomId,
                            OfficeId = truck?.LocationId ?? _officeId,
                            DriverId = driverId,
                            DeliverToId = orderLine.DeliverToId,
                            LoadAtId = orderLine.LoadAtId,
                            IsImported = true,
                            IsBilled = true
                        };
                        _ticketRepository.Insert(ticket);

                        //if (driver?.UserId != null && _useForProductionPay)
                        //{
                        //    var employeeTime = new Drivers.EmployeeTime
                        //    {
                        //        TenantId = _tenantId,
                        //        UserId = driver.UserId.Value,
                        //        StartDateTime = row.TicketDateTime,
                        //        TimeClassificationId = _timeClassificationId,
                        //        EquipmentId = truck?.Id,
                        //        DriverId = driver.Id,
                        //        IsImported = true
                        //    };
                        //    _employeeTimeRepository.Insert(employeeTime);
                        //}

                        if (truck == null)
                        {
                            AddResourceError($"Truck with license plate {row.LicensePlate} wasn’t found. You’ll need to fix the truck on the ticket view");
                        }
                        else if (!addedOrderLineTrucks.Any(x => x.TruckId == truck.Id))
                        {
                            var orderLineTruck = new OrderLineTruck
                            {
                                IsDone = true,
                                OrderLine = orderLine,
                                TenantId = _tenantId,
                                TruckId = truck.Id,
                                DriverId = driverId,
                            };
                            _orderLineTruckRepository.Insert(orderLineTruck);
                            addedOrderLineTrucks.Add(orderLineTruck);
                        }
                    }


                    CurrentUnitOfWork.SaveChanges();
                    unitOfWork.Complete();
                }
            }

            return true;
        }

        private int? GetUomId(string rowUom)
        {
            if (rowUom?.ToLower() == "ld")
            {
                rowUom = "Load";
            }

            foreach (var uom in _uoms)
            {
                if (uom.Value.Equals(rowUom, StringComparison.InvariantCultureIgnoreCase))
                {
                    return uom.Key;
                }
            }

            foreach (var uom in _uoms)
            {
                if (uom.Value.ToLower().TrimEnd('s').Equals(rowUom.ToLower().TrimEnd('s'), StringComparison.InvariantCultureIgnoreCase))
                {
                    return uom.Key;
                }
            }

            //return _uoms.First().Key;
            return null;
        }

        private void PopulateDriversForTrucks(List<int> truckIds, List<DateTime> dateList)
        {
            _driversForTrucks ??= new();
            var driverAssignments = _driverAssignmentRepository.GetAll()
                .Where(x => x.Shift == _shift
                    && dateList.Contains(x.Date)
                    && truckIds.Contains(x.TruckId))
                .Select(x => new
                {
                    x.Date,
                    x.TruckId,
                    x.DriverId
                })
                .ToList();

            var defaultDrivers = _truckRepository.GetAll()
                .Where(x => truckIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.TruckCode,
                    x.DefaultDriverId
                }).ToList();

            foreach (var truckId in truckIds)
            {
                foreach (var date in dateList)
                {
                    int? driverId;
                    var driverAssignmentGroup = driverAssignments
                        .Where(x => x.Date == date && x.TruckId == truckId)
                        .ToList();
                    var truckCode = defaultDrivers.FirstOrDefault(x => x.Id == truckId)?.TruckCode;

                    if (driverAssignmentGroup.Count == 1)
                    {
                        driverId = driverAssignmentGroup[0].DriverId;
                        if (driverId == null)
                        {
                            AddResourceError($"Truck {truckCode} has no driver assigned on {date:d}. You’ll need to fix the driver on the ticket view");
                        }
                    }
                    else if (driverAssignmentGroup.Count == 0)
                    {
                        driverId = defaultDrivers.FirstOrDefault(x => x.Id == truckId)?.DefaultDriverId;
                        if (driverId == null)
                        {
                            AddResourceError($"Truck {truckCode} has no default driver and no driver assigned on {date:d}. You’ll need to fix the driver on the ticket view");
                        }
                    }
                    else
                    {
                        driverId = null;
                        AddResourceError($"Truck {truckCode} has more than one driver assigned on {date:d}. You’ll need to fix the driver on the ticket view");
                    }

                    _driversForTrucks.Add((truckId, date), driverId);
                }
            }
        }

        private void PopulateLuckStoneLocationsFromSites(List<string> sites)
        {
            _loadAtLocations ??= new();

            var locationGroups =
                (from luckStoneLocation in _luckStoneLocationRepository.GetAll()
                    .Where(x => sites.Contains(x.Site))
                 join existingLocation in _locationRepository.GetAll()
                     on new { luckStoneLocation.Name, luckStoneLocation.StreetAddress, luckStoneLocation.City, luckStoneLocation.State, luckStoneLocation.ZipCode }
                     equals new { existingLocation.Name, existingLocation.StreetAddress, existingLocation.City, existingLocation.State, existingLocation.ZipCode }
                     into existingLocationLeftJoin
                 from existingLocation in existingLocationLeftJoin.DefaultIfEmpty()

                 select new
                 {
                     LuckStoneLocation = new
                     {
                         luckStoneLocation.Site,
                         luckStoneLocation.Name,
                         luckStoneLocation.StreetAddress,
                         luckStoneLocation.City,
                         luckStoneLocation.State,
                         luckStoneLocation.ZipCode,
                         luckStoneLocation.CountryCode,
                         luckStoneLocation.Latitude,
                         luckStoneLocation.Longitude,
                     },
                     ExistingLocation = existingLocation == null ? null : new
                     {
                         existingLocation.Id,
                     },
                 }
                ).ToList();

            foreach (var site in sites.Distinct())
            {
                var locationGroup = locationGroups.FirstOrDefault(x => x.LuckStoneLocation.Site.ToLower() == site.ToLower());
                if (locationGroup != null)
                {
                    if (locationGroup.ExistingLocation != null)
                    {
                        _loadAtLocations.Add(site.ToLower(), locationGroup.ExistingLocation.Id);
                    }
                    else
                    {
                        var location = new Location
                        {
                            Name = locationGroup.LuckStoneLocation.Name,
                            StreetAddress = locationGroup.LuckStoneLocation.StreetAddress,
                            City = locationGroup.LuckStoneLocation.City,
                            State = locationGroup.LuckStoneLocation.State,
                            ZipCode = locationGroup.LuckStoneLocation.ZipCode,
                            CountryCode = locationGroup.LuckStoneLocation.CountryCode,
                            Latitude = locationGroup.LuckStoneLocation.Latitude,
                            Longitude = locationGroup.LuckStoneLocation.Longitude,
                            IsActive = true,
                        };
                        _locationRepository.InsertAndGetId(location);
                        _loadAtLocations.Add(site.ToLower(), location.Id);
                    }
                }
                else
                {
                    var locationName = "Luck Stone " + site;
                    var location = _locationRepository.GetAll()
                        .AsNoTracking()
                        .FirstOrDefault(x => x.Name == locationName);

                    if (location == null)
                    {
                        location = new Location
                        {
                            Name = locationName,
                            IsActive = true,
                        };
                        _locationRepository.InsertAndGetId(location);
                    }

                    _loadAtLocations.Add(site.ToLower(), location.Id);
                }
            }
        }

        public void PopulateDeliverToLocationsByNames(List<string> customerNames)
        {
            _deliverToLocations ??= new();
            var locations = _locationRepository.GetAll()
                .Where(x => customerNames.Contains(x.Name))
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                });

            var newLocations = new List<Location>();
            foreach (var customerName in customerNames.Distinct())
            {
                var existingLocation = locations.FirstOrDefault(x => x.Name.ToLower() == customerName.ToLower());
                if (existingLocation != null)
                {
                    _deliverToLocations.Add(customerName.ToLower(), existingLocation.Id);
                }
                else
                {
                    var location = new Location
                    {
                        Name = customerName,
                        IsActive = true,
                        CategoryId = _temporaryLocationCategoryId
                    };
                    _locationRepository.Insert(location);
                    newLocations.Add(location);
                }
            }

            if (newLocations.Any())
            {
                CurrentUnitOfWork.SaveChanges();
                newLocations.ForEach(location => _deliverToLocations.Add(location.Name.ToLower(), location.Id));
            }
        }

        private void PopulateServices(List<string> serviceNames)
        {
            _services ??= new();
            var services = _serviceRepository.GetAll()
                .Where(x => serviceNames.Contains(x.Service1))
                .Select(x => new { x.Id, x.Service1 })
                .ToList();

            var newServices = new List<Service>();

            foreach (var serviceName in serviceNames.Distinct())
            {
                var existing = services.FirstOrDefault(x => x.Service1.ToLower() == serviceName.ToLower());
                if (existing != null)
                {
                    _services.Add(serviceName.ToLower(), existing.Id);
                }
                else
                {
                    var service = new Service
                    {
                        Service1 = serviceName,
                        IsActive = true,
                    };
                    _serviceRepository.Insert(service);
                    newServices.Add(service);
                    AddResourceError($"A service {service.Service1} wasn’t set up in your products and services. To be able to create these entries, we added this item. Please review to be sure it is set up correctly.");
                }
            }

            if (newServices.Any())
            {
                CurrentUnitOfWork.SaveChanges();
                newServices.ForEach(service => _services.Add(service.Service1.ToLower(), service.Id));
            }
        }

        private void LogWarning(string text)
        {
            Logger.Warn($"LuckStone Earnings Import warning (tenantId: {_tenantId}, userId: {_userId}, file:{_filePath}): " + text);
        }

        private void LogError(string text)
        {
            Logger.Error($"LuckStone Earnings Import error (tenantId: {_tenantId}, userId: {_userId}, file:{_filePath}): " + text);
        }
    }
}
