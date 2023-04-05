using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Configuration;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Locations;
using DispatcherWeb.Locations.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb
{
    public static class AppServiceExtensions
    {
        public static async Task<PagedResultDto<SelectListDto>> GetSelectListResult(this IQueryable<SelectListDto> query, GetSelectListInput input)
        {
            return await query.GetSelectListResult(input, x => x);
        }

        public static async Task<PagedResultDto<TOut>> GetSelectListResult<TOut, TIn>(this IQueryable<TIn> query, GetSelectListInput input, Func<TIn, TOut> resultConverter) where TIn : SelectListDto
        {
            query = query
                .OrderBy(x => x.Name)
                .WhereIf(!input.Term.IsNullOrEmpty(),
                            x => x.Name.ToLower().Contains(input.Term.ToLower()));

            var startsWithQuery = query
                .WhereIf(!input.Term.IsNullOrEmpty(),
                            x => x.Name.ToLower().StartsWith(input.Term.ToLower()));
            var containsQuery = query
                .WhereIf(!input.Term.IsNullOrEmpty(),
                            x => !x.Name.ToLower().StartsWith(input.Term.ToLower()) && x.Name.Contains(input.Term.ToLower()));

            var startsWithCount = await startsWithQuery.CountAsync();
            var containsCount = input.Term.IsNullOrEmpty() ? 0 : await containsQuery.CountAsync();
            var totalCount = containsCount + startsWithCount;

            var items = startsWithQuery.PageBy(input).ToList();
            if (items.Count < input.MaxResultCount && !input.Term.IsNullOrEmpty())
            {
                input.SkipCount = input.SkipCount - (startsWithCount - items.Count);
                input.MaxResultCount = input.MaxResultCount - items.Count;
                items.AddRange(containsQuery.PageBy(input).ToList());
            }

            return new PagedResultDto<TOut>(
                totalCount,
                items.Select(resultConverter).ToList());
        }

        public static IQueryable<SelectListDto<LocationSelectListInfoDto>> ToSelectListDto(this IQueryable<Location> query)
        {
            return query.Select(x => new SelectListDto<LocationSelectListInfoDto>
            {
                Id = x.Id.ToString(),
                Name = x.Name + ", " + x.StreetAddress + ", " + x.City + ", " + x.State,
                Item = new LocationSelectListInfoDto
                {
                    Name = x.Name,
                    StreetAddress = x.StreetAddress,
                    City = x.City,
                    State = x.State
                }
            });
        }

        public static async Task<PagedResultDto<SelectListDto>> GetSelectListResult(this IQueryable<SelectListDto<LocationSelectListInfoDto>> query, GetSelectListInput input)
        {
            return await GetSelectListResult(query, input, ConvertLocationSelectListResult);
        }

        public static SelectListDto ConvertLocationSelectListResult(SelectListDto<LocationSelectListInfoDto> item)
        {
            return new SelectListDto
            {
                Id = item.Id,
                Name = Utilities.FormatAddress(item.Item.Name, item.Item.StreetAddress, item.Item.City, item.Item.State, null)
            };
        }

        public static IQueryable<WorkOrderReportDto> GetWorkOrderReportDtoQuery(this IQueryable<Receipt> query, GetWorkOrderReportInput input, int officeId)
        {
            var newQuery = query
                .Select(r => new
                {
                    Order = r.Order,
                    Receipt = r,
                    Payment = r.OrderPayments
                        .Where(x => x.OfficeId == officeId)
                        .Select(x => x.Payment).FirstOrDefault(x => !x.IsCancelledOrRefunded)
                })
                .Select(o => new WorkOrderReportDto
                {
                    Id = o.Order.Id,
                    ContactEmail = o.Order.CustomerContact.Email,
                    ContactPhoneNumber = o.Order.CustomerContact.PhoneNumber,
                    ContactName = o.Order.CustomerContact.Name,
                    CustomerName = o.Order.Customer.Name,
                    CustomerAccountNumber = o.Order.Customer.AccountNumber,
                    ProjectName = o.Order.Project.Name,
                    ChargeTo = o.Order.ChargeTo,
                    CodTotal = o.Receipt.Total,
                    OrderDeliveryDate = o.Order.DeliveryDate,
                    OrderShift = o.Order.Shift,
                    OrderIsPending = o.Order.IsPending,
                    OfficeName = o.Order.Office.Name,
                    Directions = o.Order.Directions,
                    FreightTotal = o.Receipt.FreightTotal,
                    MaterialTotal = o.Receipt.MaterialTotal,
                    PoNumber = o.Order.PONumber,
                    SpectrumNumber = o.Order.SpectrumNumber,
                    SalesTaxRate = o.Receipt.SalesTaxRate,
                    SalesTax = o.Receipt.SalesTax,
                    AuthorizationDateTime = o.Payment.AuthorizationDateTime,
                    AuthorizationCaptureDateTime = o.Payment.AuthorizationCaptureDateTime,
                    AuthorizationCaptureSettlementAmount = o.Payment.AuthorizationCaptureAmount,
                    AuthorizationCaptureTransactionId = o.Payment.AuthorizationCaptureTransactionId,
                    IsShared = o.Order.SharedOrders.Any(so => so.OfficeId != o.Order.LocationId) || o.Order.OrderLines.Any(ol => ol.SharedOrderLines.Any(sol => sol.OfficeId != ol.Order.LocationId)),
                    AllTrucksNonDistinct = o.Order.OrderLines.SelectMany(ol => ol.OrderLineTrucks).Select(olt =>
                        new WorkOrderReportDto.TruckDriverDto
                        {
                            TruckId = olt.TruckId,
                            TruckCode = olt.Truck.TruckCode,
                            DriverName = olt.Driver.FirstName + " " + olt.Driver.LastName,
                            AssetType = olt.Truck.VehicleCategory.AssetType,
                            IsPowered = olt.Truck.VehicleCategory.IsPowered,
                            IsLeased = olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule || olt.Truck.LocationId == null
                        }
                    ).ToList(),
                    Items = o.Receipt.ReceiptLines
                        .Select(s => new WorkOrderReportItemDto
                        {
                            LineNumber = s.LineNumber,
                            MaterialQuantity = s.MaterialQuantity,
                            FreightQuantity = s.FreightQuantity,
                            MaterialUomName = s.MaterialUom.Name,
                            FreightUomName = s.FreightUom.Name,
                            FreightPricePerUnit = s.FreightRate,
                            MaterialPricePerUnit = s.MaterialRate,
                            FreightPrice = s.FreightAmount,
                            MaterialPrice = s.MaterialAmount,
                            IsFreightTotalOverridden = s.IsFreightAmountOverridden,
                            IsMaterialTotalOverridden = s.IsMaterialAmountOverridden,
                            Designation = s.Designation,
                            LoadAt = s.LoadAt == null ? null : new LocationNameDto
                            {
                                Name = s.LoadAt.Name,
                                StreetAddress = s.LoadAt.StreetAddress,
                                City = s.LoadAt.City,
                                State = s.LoadAt.State
                            },
                            DeliverTo = s.DeliverTo == null ? null : new LocationNameDto
                            {
                                Name = s.DeliverTo.Name,
                                StreetAddress = s.DeliverTo.StreetAddress,
                                City = s.DeliverTo.City,
                                State = s.DeliverTo.State
                            },
                            ServiceName = s.Service.Service1,
                            IsTaxable = s.Service.IsTaxable,
                            JobNumber = s.JobNumber,
                            Note = s.OrderLine.Note,
                            NumberOfTrucks = s.OrderLine.NumberOfTrucks ?? 0,
                            TimeOnJob = s.OrderLine.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? s.OrderLine.FirstStaggeredTimeOnJob : s.OrderLine.TimeOnJob,
                            IsTimeStaggered = s.OrderLine.StaggeredTimeKind != StaggeredTimeKind.None || s.OrderLine.OrderLineTrucks.Any(olt => olt.TimeOnJob != null)
                        }).ToList(),
                    DeliveryInfoItems = o.Order.OrderLines
                        .SelectMany(x => x.Tickets)
                        .Select(x => new WorkOrderReportDeliveryInfoDto
                        {
                            TicketNumber = x.TicketNumber,
                            TruckNumber = x.TruckCode,
                            DriverName = x.Driver == null ? null : x.Driver.LastName + ", " + x.Driver.FirstName,
                            Quantity = x.Quantity,
                            UomName = x.UnitOfMeasure.Name,
                            TicketPhotoId = x.TicketPhotoId,
                            Load = x.Load == null ? null : new WorkOrderReportLoadDto
                            {
                                DeliveryTime = x.Load.LastModificationTime,
                                SignatureName = x.Load.SignatureName,
                                SignatureId = x.Load.SignatureId
                            }
                        }).ToList()
                });

            return newQuery;
        }

        public static IQueryable<WorkOrderReportDto> GetWorkOrderReportDtoQuery(this IQueryable<Order> query, GetWorkOrderReportInput input, int officeId)
        {
            var newQuery = query
                .Select(o => new
                {
                    Order = o,
                    Payment = o.OrderPayments
                        .Where(x => x.OfficeId == officeId)
                        .Select(x => x.Payment)
                        .FirstOrDefault(x => !x.IsCancelledOrRefunded)
                })
                .Select(o => new WorkOrderReportDto
                {
                    Id = o.Order.Id,
                    ContactEmail = o.Order.CustomerContact.Email,
                    ContactPhoneNumber = o.Order.CustomerContact.PhoneNumber,
                    ContactName = o.Order.CustomerContact.Name,
                    CustomerName = o.Order.Customer.Name,
                    CustomerAccountNumber = o.Order.Customer.AccountNumber,
                    ProjectName = o.Order.Project.Name,
                    ChargeTo = o.Order.ChargeTo,
                    CodTotal = o.Order.CODTotal,
                    OrderDeliveryDate = o.Order.DeliveryDate,
                    OrderShift = o.Order.Shift,
                    OrderIsPending = o.Order.IsPending,
                    OfficeName = o.Order.Office.Name,
                    Directions = o.Order.Directions,
                    FreightTotal = o.Order.FreightTotal,
                    MaterialTotal = o.Order.MaterialTotal,
                    PoNumber = o.Order.PONumber,
                    SpectrumNumber = o.Order.SpectrumNumber,
                    SalesTaxRate = o.Order.SalesTaxRate,
                    SalesTax = o.Order.SalesTax,
                    AuthorizationDateTime = o.Payment.AuthorizationDateTime,
                    AuthorizationCaptureDateTime = o.Payment.AuthorizationCaptureDateTime,
                    AuthorizationCaptureSettlementAmount = o.Payment.AuthorizationCaptureAmount,
                    AuthorizationCaptureTransactionId = o.Payment.AuthorizationCaptureTransactionId,
                    IsShared = o.Order.SharedOrders.Any(so => so.OfficeId != o.Order.LocationId) || o.Order.OrderLines.Any(ol => ol.SharedOrderLines.Any(sol => sol.OfficeId != ol.Order.LocationId)),
                    AllTrucksNonDistinct = o.Order.OrderLines.SelectMany(ol => ol.OrderLineTrucks).Select(olt =>
                        new WorkOrderReportDto.TruckDriverDto
                        {
                            TruckId = olt.TruckId,
                            TruckCode = olt.Truck.TruckCode,
                            DriverName = olt.Driver.FirstName + " " + olt.Driver.LastName,
                            AssetType = olt.Truck.VehicleCategory.AssetType,
                            IsPowered = olt.Truck.VehicleCategory.IsPowered,
                            IsLeased = olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule || olt.Truck.LocationId == null
                        }
                    ).ToList(),
                    Items = o.Order.OrderLines
                        .Select(s => new WorkOrderReportItemDto
                        {
                            LineNumber = s.LineNumber,
                            MaterialQuantity = s.MaterialQuantity,
                            FreightQuantity = s.FreightQuantity,
                            ActualQuantity = s.Tickets.Sum(t => t.Quantity),
                            MaterialUomName = s.MaterialUom.Name,
                            FreightUomName = s.FreightUom.Name,
                            FreightPricePerUnit = s.FreightPricePerUnit,
                            MaterialPricePerUnit = s.MaterialPricePerUnit,
                            FreightPrice = s.FreightPrice,
                            MaterialPrice = s.MaterialPrice,
                            IsFreightTotalOverridden = s.IsFreightPriceOverridden,
                            IsMaterialTotalOverridden = s.IsMaterialPriceOverridden,
                            Designation = s.Designation,
                            LoadAt = s.LoadAt == null ? null : new LocationNameDto
                            {
                                Name = s.LoadAt.Name,
                                StreetAddress = s.LoadAt.StreetAddress,
                                City = s.LoadAt.City,
                                State = s.LoadAt.State
                            },
                            DeliverTo = s.DeliverTo == null ? null : new LocationNameDto
                            {
                                Name = s.DeliverTo.Name,
                                StreetAddress = s.DeliverTo.StreetAddress,
                                City = s.DeliverTo.City,
                                State = s.DeliverTo.State
                            },
                            ServiceName = s.Service.Service1,
                            IsTaxable = s.Service.IsTaxable,
                            JobNumber = s.JobNumber,
                            Note = s.Note,
                            NumberOfTrucks = s.NumberOfTrucks ?? 0,
                            TimeOnJob = s.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? s.FirstStaggeredTimeOnJob : s.TimeOnJob,
                            IsTimeStaggered = s.StaggeredTimeKind != StaggeredTimeKind.None || s.OrderLineTrucks.Any(olt => olt.TimeOnJob != null)
                        }).ToList(),
                    DeliveryInfoItems = o.Order.OrderLines
                        .SelectMany(x => x.Tickets)
                        .Select(x => new WorkOrderReportDeliveryInfoDto
                        {
                            TicketNumber = x.TicketNumber,
                            TruckNumber = x.TruckCode,
                            DriverName = x.Driver == null ? null : x.Driver.LastName + ", " + x.Driver.FirstName,
                            Quantity = x.Quantity,
                            UomName = x.UnitOfMeasure.Name,
                            TicketPhotoId = x.TicketPhotoId,
                            Load = x.Load == null ? null : new WorkOrderReportLoadDto
                            {
                                DeliveryTime = x.Load.LastModificationTime,
                                SignatureName = x.Load.SignatureName,
                                SignatureId = x.Load.SignatureId
                            }
                        }).ToList()
                });

            return newQuery;
        }

        public static async Task<List<OrderSummaryReportItemDto>> GetOrderSummaryReportItems(this IQueryable<OrderLine> query, IDictionary<Shift, string> shiftDictionary, OrderTaxCalculator taxCalculator)
        {
            var items = await query.Select(o => new OrderSummaryReportItemDto
            {
                OrderId = o.OrderId,
                OrderLineId = o.Id,
                CustomerName = o.Order.Customer.Name,
                SalesTaxRate = o.Order.SalesTaxRate,
                OrderDeliveryDate = o.Order.DeliveryDate,
                OrderShift = o.Order.Shift,
                Trucks = o.OrderLineTrucks.Select(s => s.Truck.TruckCode).Distinct().ToList(),
                NumberOfTrucks = o.NumberOfTrucks,
                LoadAt = o.LoadAt == null ? null : new LocationNameDto
                {
                    Name = o.LoadAt.Name,
                    StreetAddress = o.LoadAt.StreetAddress,
                    City = o.LoadAt.City,
                    State = o.LoadAt.State
                },
                DeliverTo = o.DeliverTo == null ? null : new LocationNameDto
                {
                    Name = o.DeliverTo.Name,
                    StreetAddress = o.DeliverTo.StreetAddress,
                    City = o.DeliverTo.City,
                    State = o.DeliverTo.State
                },
                Item = new OrderSummaryReportItemDto.ItemOrderLine
                {
                    MaterialQuantity = o.MaterialQuantity,
                    FreightQuantity = o.FreightQuantity,
                    MaterialPrice = o.MaterialPrice,
                    FreightPrice = o.FreightPrice,
                    ServiceName = o.Service.Service1,
                    IsTaxable = o.Service.IsTaxable,
                    NumberOfTrucks = o.NumberOfTrucks ?? 0,
                    Designation = o.Designation,
                    MaterialUom = o.MaterialUom.Name,
                    FreightUom = o.FreightUom.Name
                }
            })
            .OrderBy(item => item.CustomerName)
            .ToListAsync();

            var taxCalculationType = await taxCalculator.GetTaxCalculationTypeAsync();

            items.ForEach(x =>
            {
                //if (x.Items.Count > 1)
                //    // ReSharper disable once CompareOfFloatsByEqualityOperator
                //    x.Items.RemoveAll(s => (s.MaterialQuantity ?? 0) == 0 && (s.FreightQuantity ?? 0) == 0 && s.NumberOfTrucks == 0);
                x.OrderShiftName = x.OrderShift.HasValue && shiftDictionary.ContainsKey(x.OrderShift.Value) ? shiftDictionary[x.OrderShift.Value] : "";
                OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, x.Item, x.SalesTaxRate);
            });

            return items;
        }

        public static async Task<List<ScheduleTruckDto>> GetScheduleTrucks(this IQueryable<Truck> truckQuery, IGetScheduleInput input, bool useShifts, bool useLeaseHaulers, bool skipTruckFiltering = false)
        {
            var sharedTrucks = await truckQuery
                .SelectMany(x => x.SharedTrucks)
                .Where(x => x.Date == input.Date)
                .WhereIf(useShifts, st => st.Shift == input.Shift)
                .Select(x => new
                {
                    x.TruckId,
                    x.OfficeId,
                    TruckOfficeId = x.Truck.LocationId
                })
                .ToListAsync();

            var truckIdsSharedWithThisOffice = sharedTrucks.Where(x => x.OfficeId == input.OfficeId).Select(x => x.TruckId).ToList();
            var trucksSharedWithOtherOffices = sharedTrucks.Where(x => x.OfficeId != x.TruckOfficeId).ToList();

            var leaseHaulerTrucks = await truckQuery
                .Where(x => useLeaseHaulers && x.LocationId == null)
                .SelectMany(x => x.AvailableLeaseHaulerTrucks)
                .Where(a => a.OfficeId == input.OfficeId
                        && (!useShifts || a.Shift == input.Shift)
                        && a.Date == input.Date)
                .Select(x => new
                {
                    x.Id,
                    x.TruckId,
                    x.DriverId,
                    DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                    x.OfficeId,
                })
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var leaseHaulerTruckIds = leaseHaulerTrucks.Select(x => x.TruckId).ToList();

            var trucks = await truckQuery
                .WhereIf(!skipTruckFiltering, t => t.IsActive
                    && (t.LocationId == input.OfficeId || truckIdsSharedWithThisOffice.Contains(t.Id) || leaseHaulerTruckIds.Contains(t.Id)))
                .Select(t => new ScheduleTruckDto
                {
                    Id = t.Id,
                    TruckCode = t.TruckCode,
                    OfficeId = t.LocationId,
                    VehicleCategory = new VehicleCategoryDto
                    {
                        Id = t.VehicleCategory.Id,
                        Name = t.VehicleCategory.Name,
                        AssetType = t.VehicleCategory.AssetType,
                        IsPowered = t.VehicleCategory.IsPowered,
                        SortOrder = t.VehicleCategory.SortOrder,
                    },
                    BedConstruction = t.BedConstruction,
                    IsApportioned = t.IsApportioned,
                    AlwaysShowOnSchedule = t.LeaseHaulerTruck.AlwaysShowOnSchedule == true,
                    CanPullTrailer = t.CanPullTrailer,
                    IsOutOfService = t.IsOutOfService,
                    IsActive = t.IsActive,
                    DefaultDriverId = t.DefaultDriverId,
                    DefaultDriverName = t.DefaultDriver.FirstName + " " + t.DefaultDriver.LastName,
                    UtilizationList = t.OrderLineTrucks
                        .Where(olt => !olt.OrderLine.IsComplete
                                && olt.OrderLine.Order.DeliveryDate == input.Date
                                && olt.OrderLine.Order.Shift == input.Shift
                                && !olt.OrderLine.Order.IsPending
                                && (olt.OrderLine.Order.LocationId == input.OfficeId || olt.OrderLine.SharedOrderLines.Any(s => s.OfficeId == input.OfficeId)))
                        .Select(olt => olt.Utilization)
                        .ToList()
                })
                .ToListAsync();

            var driverAssignments = await truckQuery
                .SelectMany(x => x.DriverAssignments)
                .Where(da => da.Date == input.Date && da.Shift == input.Shift)
                .Select(x => new
                {
                    x.Id,
                    x.TruckId,
                    x.DriverId,
                    DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                })
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            foreach (var truck in trucks)
            {
                var driverAssignment = driverAssignments.FirstOrDefault(x => x.TruckId == truck.Id);
                var leaseHaulerTruck = leaseHaulerTrucks.FirstOrDefault(x => x.TruckId == truck.Id);
                if (leaseHaulerTruck != null)
                {
                    truck.DriverId = leaseHaulerTruck.DriverId;
                    truck.DriverName = leaseHaulerTruck.DriverName;
                    truck.OfficeId = leaseHaulerTruck.OfficeId;
                    truck.IsExternal = true;
                }
                else if (driverAssignment != null)
                {
                    if (driverAssignment.DriverId != null)
                    {
                        truck.DriverId = driverAssignment.DriverId;
                        truck.DriverName = driverAssignment.DriverName;
                    }
                    else
                    {
                        //we intentionally don't fall back to defaultDriverId here. If driverId is explicitly set to null in a driver assignment, use that instead of default driver
                        truck.DriverId = null;
                        truck.DriverName = "[No driver]";
                    }
                }
                else if (truck.DefaultDriverId.HasValue)
                {
                    truck.DriverId = truck.DefaultDriverId;
                    truck.DriverName = truck.DefaultDriverName;
                }
                else
                {
                    truck.DriverId = null;
                    truck.DriverName = "[No driver]";
                }

                truck.SharedWithOfficeId = trucksSharedWithOtherOffices.FirstOrDefault(y => y.TruckId == truck.Id)?.OfficeId;
                truck.SharedFromOfficeId = sharedTrucks.Where(st => st.TruckId == truck.Id && st.OfficeId == input.OfficeId).Select(st => (int?)st.OfficeId).FirstOrDefault();

                truck.Utilization = !truck.VehicleCategory.IsPowered
                        //previous trailer utilization logic
                        //? (t.UtilizationList.Any() ? 1 : 0)
                        ? 0
                        : truck.UtilizationList.Sum();

                truck.ActualUtilization = truck.Utilization;

                //'skip truck filtering' is set on adding new trucks, when the actual filter values (Date and OfficeId) are not sent and the values from the order are used instead.
                if (!skipTruckFiltering)
                {
                    if (truck.SharedWithOfficeId != null && truck.SharedWithOfficeId != input.OfficeId)
                    {
                        truck.Utilization = 1;
                    }
                }
            }

            return trucks.OrderByTruck();
        }
        private static List<ScheduleTruckDto> OrderByTruck(this List<ScheduleTruckDto> query)
        {
            return query
                .OrderByDescending(x => !x.IsExternal)
                .ThenByDescending(x => x.VehicleCategory.IsPowered)
                //.ThenBy(x => x.VehicleCategory.SortOrder)
                .ThenBy(x => x.TruckCode)
                .ToList();
        }


        public static async Task<List<ScheduleTruckFullDto>> PopulateScheduleTruckFullFields(this List<ScheduleTruckDto> trucksLite, IGetScheduleInput input, IQueryable<DriverAssignment> driverAssignmentsQuery)
        {
            var driverAssignments = await driverAssignmentsQuery
                .Where(x => x.Date == input.Date && x.Shift == input.Shift)
                .Select(x => new
                {
                    x.Id,
                    x.TruckId,
                    x.DriverId
                })
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var trucks = trucksLite.Select(ScheduleTruckFullDto.GetFrom).ToList();

            trucks.ForEach(x =>
            {
                if (x.IsExternal)
                {
                    return;
                }
                var driverAssignment = driverAssignments.FirstOrDefault(y => y.TruckId == x.Id);
                x.HasNoDriver = driverAssignment != null && driverAssignment.DriverId == null;
                x.HasDriverAssignment = driverAssignment?.DriverId != null;
            });

            return trucks;
        }

        public static IQueryable<ScheduleOrderLineDto> GetScheduleOrders(this IQueryable<OrderLine> query)
        {
            return query
                .Select(ol => new ScheduleOrderLineDto
                {
                    Id = ol.Id,
                    Date = ol.Order.DeliveryDate ?? new DateTime(),
                    Shift = ol.Order.Shift,
                    OrderId = ol.OrderId,
                    Priority = ol.Order.Priority,
                    OfficeId = ol.Order.LocationId,
                    CustomerIsCod = ol.Order.Customer.IsCod,
                    CustomerId = ol.Order.CustomerId,
                    CustomerName = ol.Order.Customer.Name,
                    IsTimeStaggered = ol.StaggeredTimeKind != StaggeredTimeKind.None || ol.OrderLineTrucks.Any(olt => olt.TimeOnJob != null),
                    IsTimeEditable = ol.StaggeredTimeKind == StaggeredTimeKind.None,
                    Time = ol.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? ol.FirstStaggeredTimeOnJob : ol.TimeOnJob,
                    StaggeredTimeKind = ol.StaggeredTimeKind,
                    FirstStaggeredTimeOnJob = ol.FirstStaggeredTimeOnJob,
                    LoadAtId = ol.LoadAtId,
                    LoadAtNamePlain = ol.LoadAt.Name + ol.LoadAt.StreetAddress + ol.LoadAt.City + ol.LoadAt.State, //for sorting
                    LoadAt = ol.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = ol.LoadAt.Name,
                        StreetAddress = ol.LoadAt.StreetAddress,
                        City = ol.LoadAt.City,
                        State = ol.LoadAt.State
                    },
                    DeliverToId = ol.DeliverToId,
                    DeliverToNamePlain = ol.DeliverTo.Name + ol.DeliverTo.StreetAddress + ol.DeliverTo.City + ol.DeliverTo.State, //for sorting
                    DeliverTo = ol.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = ol.DeliverTo.Name,
                        StreetAddress = ol.DeliverTo.StreetAddress,
                        City = ol.DeliverTo.City,
                        State = ol.DeliverTo.State
                    },
                    JobNumber = ol.JobNumber,
                    Note = ol.Note,
                    Item = ol.Service.Service1,
                    MaterialUom = ol.MaterialUom.Name,
                    FreightUom = ol.FreightUom.Name,
                    MaterialQuantity = ol.MaterialQuantity,
                    FreightQuantity = ol.FreightQuantity,
                    IsFreightPriceOverridden = ol.IsFreightPriceOverridden,
                    IsMaterialPriceOverridden = ol.IsMaterialPriceOverridden,
                    Designation = ol.Designation,
                    NumberOfTrucks = ol.NumberOfTrucks,
                    ScheduledTrucks = ol.ScheduledTrucks,
                    IsClosed = ol.IsComplete,
                    IsCancelled = ol.IsCancelled,
                    IsShared = ol.SharedOrderLines.Any(sol => sol.OfficeId != ol.Order.LocationId),
                    HaulingCompanyOrderLineId = ol.HaulingCompanyOrderLineId,
                    MaterialCompanyOrderLineId = ol.MaterialCompanyOrderLineId,
                    SharedOfficeIds = ol.SharedOrderLines.Select(sol => sol.OfficeId).ToArray(),
                    Utilization = ol.OrderLineTrucks.Where(t => t.Truck.VehicleCategory.IsPowered).Select(t => t.Utilization).Sum(),
                    Trucks = ol.OrderLineTrucks.Select(olt => new ScheduleOrderLineTruckDto
                    {
                        Id = olt.Id,
                        ParentId = olt.ParentOrderLineTruckId,
                        TruckId = olt.TruckId,
                        TruckCode = olt.Truck.TruckCode,
                        DriverId = olt.DriverId,
                        OrderId = ol.OrderId,
                        OrderLineId = ol.Id,
                        IsExternal = olt.Truck.LocationId == null,
                        OfficeId = olt.Truck.LocationId ?? ol.Order.LocationId /* Available Lease Hauler Truck */,
                        SharedOfficeId = olt.Truck.SharedTrucks.Where(st => st.Date == ol.Order.DeliveryDate && st.Shift == ol.Order.Shift).Select(st => st.OfficeId).FirstOrDefault(),
                        Utilization = olt.Utilization,
                        VehicleCategory = new VehicleCategoryDto
                        {
                            Id = olt.Truck.VehicleCategory.Id,
                            Name = olt.Truck.VehicleCategory.Name,
                            AssetType = olt.Truck.VehicleCategory.AssetType,
                            IsPowered = olt.Truck.VehicleCategory.IsPowered,
                            SortOrder = olt.Truck.VehicleCategory.SortOrder
                        },
                        AlwaysShowOnSchedule = olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule == true,
                        CanPullTrailer = olt.Truck.CanPullTrailer,
                        IsDone = olt.IsDone,
                        TimeOnJob = ol.TimeOnJob
                    }).ToList(),
                });
        }

        public static bool IsQuantityValid(this OrderLine orderLine)
        {
            return orderLine.MaterialQuantity > 0 || orderLine.FreightQuantity > 0 || !(orderLine.FreightPrice > 0 || orderLine.MaterialPrice > 0);
        }

        public static void RemoveStaggeredTimeIfNeeded(this OrderLine orderLine)
        {
            if (orderLine.Id == 0 || orderLine.StaggeredTimeKind == StaggeredTimeKind.None || orderLine.NumberOfTrucks > 1)
            {
                return;
            }

            orderLine.StaggeredTimeKind = StaggeredTimeKind.None;
            orderLine.StaggeredTimeInterval = null;
            orderLine.FirstStaggeredTimeOnJob = null;
        }

        public static async Task<EnsureCanEditTruckOrSharedTruckResult> EnsureCanEditTruckOrSharedTruckAsync(this IRepository<Truck> truckRepository, int truckId, int officeId, DateTime date)
        {
            return await EnsureCanEditTruckOrSharedTruckAsync(truckRepository, truckId, officeId, date, date);
        }

        public static async Task<EnsureCanEditTruckOrSharedTruckResult> EnsureCanEditTruckOrSharedTruckAsync(this IRepository<Truck> truckRepository, int truckId, int officeId, DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new UserFriendlyException("End Date should be greater than Start Date");
            }

            if (truckId == 0)
                return new EnsureCanEditTruckOrSharedTruckResult { SharedTrucks = new List<SharedTruckDto>() };

            var truck = await truckRepository.GetAll()
                .Where(x => x.Id == truckId)
                .Select(x => new
                {
                    x.LocationId,
                    SharedTrucks = x.SharedTrucks
                        .Where(s => s.Date >= startDate && s.Date <= endDate)
                        .Select(s => new SharedTruckDto
                        {
                            OfficeId = s.OfficeId,
                            Date = s.Date,
                            Shift = s.Shift,
                            TruckId = s.TruckId
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            var result = new EnsureCanEditTruckOrSharedTruckResult
            {
                SharedTrucks = truck.SharedTrucks,
                TruckOfficeId = truck.LocationId
            };

            if (truck == null)
            {
                return result;
            }

            if (truck.LocationId == officeId)
            {
                return result;
            }

            //var date = startDate;
            //while (date <= endDate)
            //{
            //    if (!truck.SharedTrucks.Any(s => s.Date == date && s.OfficeId == officeId))
            //    {
            //        throw new UserFriendlyException("You can't edit a truck assigned to another office if it's not shared with you");
            //    }
            //    date = date.AddDays(1);
            //}

            return result;
        }

        public static async Task EnsureCanEditTicket(this IRepository<Ticket> ticketRepository, int? ticketId)
        {
            var cannotEditReason = await ticketRepository.GetCannotEditTicketReason(ticketId);
            if (!string.IsNullOrEmpty(cannotEditReason))
            {
                throw new UserFriendlyException(cannotEditReason);
            }
        }

        public static async Task<string> GetCannotEditTicketReason(this IRepository<Ticket> ticketRepository, int? ticketId)
        {
            if (ticketId > 0 && await ticketRepository.GetAll()
                .AnyAsync(x => x.Id == ticketId
                    && x.InvoiceLine != null))
            {
                return "You can't edit already invoiced tickets";
            }

            if (ticketId > 0 && await ticketRepository.GetAll()
                .AnyAsync(x => x.Id == ticketId
                    && x.PayStatementTickets.Any()))
            {
                return "You can't edit tickets that were added to pay statements";
            }

            if (ticketId > 0 && await ticketRepository.GetAll()
                .AnyAsync(x => x.Id == ticketId
                    && x.LeaseHaulerStatementTicket != null))
            {
                return "You can't edit tickets that were added to lease hauler statements";
            }

            if (ticketId > 0 && await ticketRepository.GetAll()
                .AnyAsync(x => x.Id == ticketId
                    && x.ReceiptLineId != null))
            {
                return "You can't edit tickets that were added to receipts";
            }

            return null;
        }

        public static async Task<bool> CanOverrideTotals(this IRepository<OrderLine> orderLineRepository, int orderLineId, int officeId)
        {
            return !await orderLineRepository.GetAll()
                .AnyAsync(x => x.Id == orderLineId
                    && x.Tickets.Any(a => a.OfficeId != officeId));
        }

        public static async Task<bool> IsEntityDeleted<T>(this IRepository<T> repository, EntityDto input, IActiveUnitOfWork uow) where T : Entity<int>, ISoftDelete
        {
            ISoftDelete entity;
            using (uow.DisableFilter(AbpDataFilters.SoftDelete))
            {
                entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == input.Id);
            }

            if (entity != null && entity.IsDeleted)
            {
                return true;
            }

            return false;
        }

        public static async Task<T> GetDeletedEntity<T>(this IRepository<T> repository, EntityDto input, IActiveUnitOfWork uow) where T : Entity<int>, ISoftDelete
        {
            T entity;
            using (uow.DisableFilter(AbpDataFilters.SoftDelete))
            {
                entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == input.Id);
            }

            if (entity != null && entity.IsDeleted)
            {
                return entity;
            }

            return null;
        }

        public static async Task<CultureInfo> GetCurrencyCultureAsync(this ISettingManager settingManager)
        {
            var currencySymbol = await settingManager.GetSettingValueAsync(AppSettings.General.CurrencySymbol);

            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.CurrencySymbol = currencySymbol;
            return culture;
        }
    }
}
