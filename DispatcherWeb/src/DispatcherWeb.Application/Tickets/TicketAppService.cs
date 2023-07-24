using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.DailyFuelCosts;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Invoices;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Services;
using DispatcherWeb.Services.Dto;
using DispatcherWeb.Sessions;
using DispatcherWeb.Storage;
using DispatcherWeb.Tickets.Dto;
using DispatcherWeb.Tickets.Exporting;
using DispatcherWeb.Tickets.Reports;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitsOfMeasure;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraDoc.DocumentObjectModel;

namespace DispatcherWeb.Tickets
{
    [AbpAuthorize]
    public class TicketAppService : DispatcherWebAppServiceBase, ITicketAppService
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<LeaseHauler> _leaseHaulerRepository;
        private readonly IRepository<DailyFuelCost> _dailyFuelCostRepository;
        private readonly IRepository<UnitOfMeasure> _uomRepository;
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly ITicketListCsvExporter _ticketListCsvExporter;
        private readonly IServiceAppService _serviceAppService;
        private readonly IRepository<TimeOff> _timeOffRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IFuelSurchargeCalculator _fuelSurchargeCalculator;
        private readonly TicketPrintOutGenerator _ticketPrintOutGenerator;

        public TicketAppService(
            IRepository<Ticket> ticketRepository,
            IRepository<Truck> truckRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Driver> driverRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<LeaseHauler> leaseHaulerRepository,
            IRepository<DailyFuelCost> dailyFuelCostRepository,
            IRepository<UnitOfMeasure> uomRepository,
            OrderTaxCalculator orderTaxCalculator,
            IBinaryObjectManager binaryObjectManager,
            ITicketListCsvExporter ticketListCsvExporter,
            IServiceAppService serviceAppService,
            IRepository<TimeOff> timeOffRepository,
            IRepository<Invoice> invoiceRepository,
            IFuelSurchargeCalculator fuelSurchargeCalculator,
            TicketPrintOutGenerator ticketPrintOutGenerator
        )
        {
            _ticketRepository = ticketRepository;
            _truckRepository = truckRepository;
            _orderLineRepository = orderLineRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _dispatchRepository = dispatchRepository;
            _driverRepository = driverRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _leaseHaulerRepository = leaseHaulerRepository;
            _dailyFuelCostRepository = dailyFuelCostRepository;
            _uomRepository = uomRepository;
            _orderTaxCalculator = orderTaxCalculator;
            _binaryObjectManager = binaryObjectManager;
            _ticketListCsvExporter = ticketListCsvExporter;
            _serviceAppService = serviceAppService;
            _timeOffRepository = timeOffRepository;
            _invoiceRepository = invoiceRepository;
            _fuelSurchargeCalculator = fuelSurchargeCalculator;
            _ticketPrintOutGenerator = ticketPrintOutGenerator;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task<EditOrderTicketOutput> EditOrderTicket(OrderTicketEditDto model)
        {
            await _ticketRepository.EnsureCanEditTicket(model.Id);
            string resultWarningText = null;

            Ticket ticket;
            if (model.Id != 0)
            {
                ticket = await _ticketRepository.GetAsync(model.Id);
            }
            else
            {
                await EnsureCanAddTickets(model.OrderLineId, OfficeId);
                ticket = new Ticket()
                {
                    Id = model.Id,
                };
            }

            var truckWasChanged = model.TruckId != ticket.TruckId;
            var driverWasChanged = ticket.DriverId != model.DriverId;
            var trailerWasChanged = ticket.TrailerId != model.TrailerId;
            var originalDriverId = ticket.DriverId;
            var quantityWasChanged = ticket.Quantity != model.Quantity;

            ticket.OrderLineId = model.OrderLineId;
            ticket.OfficeId = OfficeId;
            ticket.TicketNumber = model.TicketNumber;
            ticket.Quantity = model.Quantity;
            ticket.TruckId = model.TruckId;
            ticket.TruckCode = model.TruckCode;
            ticket.TrailerId = model.TrailerId;
            ticket.DriverId = model.DriverId;

            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);

            var orderLineData = await _orderLineRepository.GetAll()
                .Where(ol => ol.Id == model.OrderLineId)
                .Select(ol => new
                {
                    ol.OrderId,
                    ol.Order.DeliveryDate,
                    ol.Order.Shift,
                    ol.Order.CustomerId,
                    ol.ServiceId,
                    ol.Designation,
                    ol.MaterialUomId,
                    ol.FreightUomId,
                    ol.LoadAtId,
                    ol.DeliverToId,
                    ol.Id,
                    ProductionPay = ol.ProductionPay && allowProductionPay,
                })
                .FirstAsync();

            var validateDriverAndTruckOnTickets = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ValidateDriverAndTruckOnTickets);
            if (truckWasChanged && ticket.TruckId.HasValue)
            {
                var ticketDriverAndTrailer = await GetDriverAndTrailerForTicketTruck(new GetDriverAndTrailerForTicketTruckInput
                {
                    OrderLineId = model.OrderLineId,
                    TruckId = ticket.TruckId
                });

                if (validateDriverAndTruckOnTickets)
                {
                    if (ticketDriverAndTrailer.DriverId.HasValue)
                    {
                        ticket.DriverId = ticketDriverAndTrailer.DriverId;
                        model.DriverId = ticketDriverAndTrailer.DriverId;
                        model.DriverName = ticketDriverAndTrailer.DriverName;
                    }
                    if (ticketDriverAndTrailer.TrailerId.HasValue)
                    {
                        ticket.TrailerId = ticketDriverAndTrailer.TrailerId;
                        model.TrailerId = ticketDriverAndTrailer.TrailerId;
                        model.TrailerTruckCode = ticketDriverAndTrailer.TrailerTruckCode;
                    }

                    driverWasChanged = originalDriverId != ticket.DriverId;
                }

                ticket.CarrierId = ticketDriverAndTrailer.CarrierId;
            }
            else if (driverWasChanged && ticket.DriverId.HasValue)
            {
                var ticketTruckAndTrailer = await GetTruckAndTrailerForTicketDriver(new GetTruckAndTrailerForTicketDriverInput
                {
                    OrderLineId = model.OrderLineId,
                    DriverId = ticket.DriverId.Value
                });

                if (validateDriverAndTruckOnTickets)
                {
                    if (ticketTruckAndTrailer.TruckId.HasValue)
                    {
                        ticket.TruckId = ticketTruckAndTrailer.TruckId;
                        model.TruckId = ticketTruckAndTrailer.TruckId;
                        model.TruckCode = ticketTruckAndTrailer.TruckCode;
                        ticket.CarrierId = ticketTruckAndTrailer.CarrierId;
                    }
                    if (ticketTruckAndTrailer.TrailerId.HasValue)
                    {
                        ticket.TrailerId = ticketTruckAndTrailer.TrailerId;
                        model.TrailerId = ticketTruckAndTrailer.TrailerId;
                        model.TrailerTruckCode = ticketTruckAndTrailer.TrailerTruckCode;
                    }
                }
            }
            else if (trailerWasChanged && ticket.TrailerId.HasValue)
            {
                var ticketTruckAndDriver = await GetTruckAndDriverForTicketTrailer(new GetTruckAndDriverForTicketTrailerInput
                {
                    OrderLineId = model.OrderLineId,
                    TrailerId = ticket.TrailerId.Value
                });

                if (validateDriverAndTruckOnTickets)
                {
                    if (ticketTruckAndDriver.TruckId.HasValue)
                    {
                        ticket.TruckId = ticketTruckAndDriver.TruckId;
                        model.TruckId = ticketTruckAndDriver.TruckId;
                        model.TruckCode = ticketTruckAndDriver.TruckCode;
                        ticket.CarrierId = ticketTruckAndDriver.CarrierId;
                    }

                    if (ticketTruckAndDriver.DriverId.HasValue)
                    {
                        ticket.DriverId = ticketTruckAndDriver.DriverId;
                        model.DriverId = ticketTruckAndDriver.DriverId;
                        model.DriverName = ticketTruckAndDriver.DriverName;
                    }
                }
            }

            var timezone = await GetTimezone();
            if ((ticket.TicketDateTime.HasValue || orderLineData.DeliveryDate.HasValue) && model.TicketDateTime.HasValue)
            {
                DateTime dateToUse;
                if (ticket.TicketDateTime.HasValue)
                {
                    dateToUse = ticket.TicketDateTime.Value.ConvertTimeZoneTo(timezone).Date;
                }
                else
                {
                    dateToUse = orderLineData.DeliveryDate.Value.Date;
                }
                var newDateWithTime = dateToUse.Add(model.TicketDateTime.Value.TimeOfDay);
                ticket.TicketDateTime = newDateWithTime.ConvertTimeZoneFrom(timezone);
            }
            else
            {
                ticket.TicketDateTime = orderLineData.DeliveryDate?.ConvertTimeZoneFrom(timezone);
            }
            ticket.CustomerId = orderLineData.CustomerId;
            ticket.ServiceId = orderLineData.ServiceId;
            if (orderLineData.Designation.MaterialOnly() || orderLineData.Designation == DesignationEnum.FreightAndMaterial)
            {
                ticket.UnitOfMeasureId = orderLineData.MaterialUomId;
            }
            else
            {
                ticket.UnitOfMeasureId = orderLineData.FreightUomId;
            }
            ticket.OrderLineId = orderLineData.Id;
            ticket.LoadAtId = orderLineData.LoadAtId;
            ticket.DeliverToId = orderLineData.DeliverToId;

            //if (driverHasChanged && ticket.DriverId.HasValue)
            //{
            //    if (orderLineData.ProductionPay || ticket.TimeClassificationId.HasValue)
            //    {
            //        var timeClassification = await _driverAppService.GetDriverPayRate(new Drivers.Dto.GetDriverPayRateInput
            //        {
            //            DriverId = ticket.DriverId.Value,
            //            ProductionPay = orderLineData.ProductionPay ? true : (bool?)null,
            //            TimeClassificationId = orderLineData.ProductionPay ? null : ticket.TimeClassificationId
            //        });

            //        if (timeClassification?.IsProductionBased == false)
            //        {
            //            ticket.DriverPayRate = null;
            //        }
            //        else
            //        {
            //            if (!(timeClassification?.PayRate > 0))
            //            {
            //                resultWarningText = L("Driver{0}DoesntHavePayRateSet", timeClassification.DriverName);
            //            }
            //            ticket.DriverPayRate = timeClassification.PayRate;
            //            ticket.TimeClassificationId = timeClassification.TimeClassificationId;
            //        }
            //    }
            //    else
            //    {
            //        ticket.DriverPayRate = null;
            //    }
            //}

            model.Id = await _ticketRepository.InsertOrUpdateAndGetIdAsync(ticket);

            await CurrentUnitOfWork.SaveChangesAsync();
            var taxDetails = await _orderTaxCalculator.CalculateTotalsAsync(orderLineData.OrderId);
            if (quantityWasChanged)
            {
                await _fuelSurchargeCalculator.RecalculateTicket(ticket.Id);
            }

            return new EditOrderTicketOutput
            {
                OrderTaxDetails = new OrderTaxDetailsDto(taxDetails),
                Ticket = model,
                WarningText = resultWarningText
            };
        }

        public async Task<GetDriverAndTrailerForTicketTruckResult> GetDriverAndTrailerForTicketTruck(GetDriverAndTrailerForTicketTruckInput input)
        {
            if (!input.ValidateInput())
            {
                return new GetDriverAndTrailerForTicketTruckResult();
            }

            var order = input.OrderLineId.HasValue ? _orderLineRepository.GetAll()
                .Where(x => x.Id == input.OrderLineId)
                .Select(x => new
                {
                    x.Order.DeliveryDate,
                    x.Order.Shift,
                    x.Order.LocationId
                }).FirstOrDefault() : null;

            var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                .WhereIf(order != null, x =>
                    x.OrderLine.Order.DeliveryDate == order.DeliveryDate
                    && x.OrderLine.Order.Shift == order.Shift
                    && x.OrderLine.Order.LocationId == order.LocationId)
                .WhereIf(input.OrderDate.HasValue, x => x.OrderLine.Order.DeliveryDate == input.OrderDate)
                .WhereIf(input.TruckId.HasValue, x => x.TruckId == input.TruckId)
                .WhereIf(!string.IsNullOrEmpty(input.TruckCode), x => x.Truck.TruckCode == input.TruckCode)
                .Where(x => x.DriverId.HasValue)
                .Select(x => new
                {
                    x.DriverId,
                    DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                    x.TrailerId,
                    TrailerTruckCode = x.Trailer.TruckCode,
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    LeaseHaulerName = x.Truck.LeaseHaulerTruck.LeaseHauler.Name
                })
                .ToListAsync();

            var result = new GetDriverAndTrailerForTicketTruckResult();

            if (orderLineTrucks.Select(x => x.TrailerId).Distinct().Count() == 1)
            {
                result.TrailerId = orderLineTrucks[0].TrailerId;
                result.TrailerTruckCode = orderLineTrucks[0].TrailerTruckCode;
            }

            if (orderLineTrucks.Select(x => x.DriverId).Distinct().Count() == 1)
            {
                result.DriverId = orderLineTrucks[0].DriverId;
                result.DriverName = orderLineTrucks[0].DriverName;
                result.CarrierId = orderLineTrucks[0].LeaseHaulerId;
                result.CarrierName = orderLineTrucks[0].LeaseHaulerName;
                result.TruckCodeIsCorrect = true;
            }
            else
            {
                var truckDetails = await _truckRepository.GetAll()
                    .WhereIf(input.TruckId.HasValue, x => x.Id == input.TruckId)
                    .WhereIf(!string.IsNullOrEmpty(input.TruckCode), x => x.TruckCode == input.TruckCode)
                    .Select(x => new
                    {
                        //DefaultDriverId = x.DefaultDriverId,
                        //DefaultDriverName = x.DefaultDriver.FirstName + " " + x.DefaultDriver.LastName,
                        LeaseHaulerId = (int?)x.LeaseHaulerTruck.LeaseHaulerId,
                        LeaseHaulerName = x.LeaseHaulerTruck.LeaseHauler.Name
                    }).FirstOrDefaultAsync();

                result.CarrierId = truckDetails?.LeaseHaulerId;
                result.CarrierName = truckDetails?.LeaseHaulerName;
                result.TruckCodeIsCorrect = truckDetails != null;
            }

            return result;
        }

        public async Task<GetTruckAndTrailerForTicketDriverResult> GetTruckAndTrailerForTicketDriver(GetTruckAndTrailerForTicketDriverInput input)
        {
            var order = input.OrderLineId.HasValue ? _orderLineRepository.GetAll()
                .Where(x => x.Id == input.OrderLineId)
                .Select(x => new
                {
                    x.Order.DeliveryDate,
                    x.Order.Shift,
                    x.Order.LocationId
                }).FirstOrDefault() : null;

            var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                .WhereIf(input.OrderLineId.HasValue, x => x.OrderLineId == input.OrderLineId)
                .WhereIf(input.OrderDate.HasValue, x => x.OrderLine.Order.DeliveryDate == input.OrderDate)
                .Where(x => x.DriverId == input.DriverId)
                .Select(x => new
                {
                    x.TruckId,
                    TruckCode = x.Truck.TruckCode,
                    x.TrailerId,
                    TrailerTruckCode = x.Trailer.TruckCode,
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    LeaseHaulerName = x.Truck.LeaseHaulerTruck.LeaseHauler.Name
                })
                .ToListAsync();

            var result = new GetTruckAndTrailerForTicketDriverResult();

            if (orderLineTrucks.Select(x => x.TruckId).Distinct().Count() == 1)
            {
                result.TruckId = orderLineTrucks[0].TruckId;
                result.TruckCode = orderLineTrucks[0].TruckCode;
                result.CarrierId = orderLineTrucks[0].LeaseHaulerId;
                result.CarrierName = orderLineTrucks[0].LeaseHaulerName;
            }

            if (orderLineTrucks.Select(x => x.TrailerId).Distinct().Count() == 1)
            {
                result.TrailerId = orderLineTrucks[0].TrailerId;
                result.TrailerTruckCode = orderLineTrucks[0].TrailerTruckCode;
            }

            return result;
        }

        public async Task<GetTruckAndDriverForTicketTrailerResult> GetTruckAndDriverForTicketTrailer(GetTruckAndDriverForTicketTrailerInput input)
        {
            var order = input.OrderLineId.HasValue ? _orderLineRepository.GetAll()
                .Where(x => x.Id == input.OrderLineId)
                .Select(x => new
                {
                    x.Order.DeliveryDate,
                    x.Order.Shift,
                    x.Order.LocationId
                }).FirstOrDefault() : null;

            var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                .WhereIf(input.OrderLineId.HasValue, x => x.OrderLineId == input.OrderLineId)
                .WhereIf(input.OrderDate.HasValue, x => x.OrderLine.Order.DeliveryDate == input.OrderDate)
                .Where(x => x.TrailerId == input.TrailerId)
                .Select(x => new
                {
                    x.TruckId,
                    TruckCode = x.Truck.TruckCode,
                    x.DriverId,
                    DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    LeaseHaulerName = x.Truck.LeaseHaulerTruck.LeaseHauler.Name
                })
                .ToListAsync();

            var result = new GetTruckAndDriverForTicketTrailerResult();

            if (orderLineTrucks.Select(x => x.TruckId).Distinct().Count() == 1)
            {
                result.TruckId = orderLineTrucks[0].TruckId;
                result.TruckCode = orderLineTrucks[0].TruckCode;
                result.CarrierId = orderLineTrucks[0].LeaseHaulerId;
                result.CarrierName = orderLineTrucks[0].LeaseHaulerName;
            }

            if (orderLineTrucks.Select(x => x.DriverId).Distinct().Count() == 1)
            {
                result.DriverId = orderLineTrucks[0].DriverId;
                result.DriverName = orderLineTrucks[0].DriverName;
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task<TicketEditDto> GetTicketEditDto(NullableIdDto input)
        {
            TicketEditDto ticket;
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
            if (input.Id.HasValue)
            {
                ticket = await _ticketRepository.GetAll()
                    .Select(t => new TicketEditDto
                    {
                        Id = t.Id,
                        OrderLineId = t.OrderLineId,
                        OrderLineDesignation = t.OrderLine.Designation,
                        OrderLineIsProductionPay = t.OrderLine.ProductionPay && allowProductionPay,
                        OrderDate = t.OrderLine.Order.DeliveryDate,
                        TicketNumber = t.TicketNumber,
                        TicketDateTime = t.TicketDateTime,
                        Shift = t.Shift ?? t.OrderLine.Order.Shift,
                        CustomerId = t.CustomerId,
                        CustomerName = t.Customer != null ? t.Customer.Name : "",
                        CarrierId = t.CarrierId,
                        CarrierName = t.Carrier != null ? t.Carrier.Name : "",
                        Quantity = t.Quantity,
                        ServiceId = t.ServiceId,
                        ServiceName = t.Service != null ? t.Service.Service1 : "",
                        TruckCode = t.Truck.TruckCode ?? t.TruckCode,
                        TruckId = t.TruckId,
                        TrailerId = t.TrailerId,
                        TrailerTruckCode = t.Trailer.TruckCode,
                        DriverId = t.DriverId,
                        DriverName = t.Driver.FirstName + " " + t.Driver.LastName,
                        UomId = t.UnitOfMeasureId,
                        UomName = t.UnitOfMeasure != null ? t.UnitOfMeasure.Name : "",
                        LoadAtId = t.LoadAtId,
                        LoadAt = t.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = t.LoadAt.Name,
                            StreetAddress = t.LoadAt.StreetAddress,
                            City = t.LoadAt.City,
                            State = t.LoadAt.State
                        },
                        DeliverToId = t.DeliverToId,
                        DeliverTo = t.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = t.DeliverTo.Name,
                            StreetAddress = t.DeliverTo.StreetAddress,
                            City = t.DeliverTo.City,
                            State = t.DeliverTo.State
                        },
                        IsVerified = t.IsVerified,
                        IsBilled = t.IsBilled,
                        ReceiptLineId = t.ReceiptLineId,
                        TicketPhotoId = t.TicketPhotoId,
                        IsReadOnly = t.InvoiceLine != null //already invoiced
                                || t.PayStatementTickets.Any() //already added to pay statements
                                || t.ReceiptLineId != null //already added to receipts
                                || t.LeaseHaulerStatementTicket != null, //added to lease hauler statements
                        HasPayStatements = t.PayStatementTickets.Any(),
                        HasLeaseHaulerStatements = t.LeaseHaulerStatementTicket != null
                    })
                    .FirstAsync(t => t.Id == input.Id.Value);

                ticket.TicketDateTime = ticket.TicketDateTime?.ConvertTimeZoneTo(await GetTimezone());
                ticket.CannotEditReason = await _ticketRepository.GetCannotEditTicketReason(input.Id.Value);
            }
            else
            {
                ticket = new TicketEditDto();
            }
            return ticket;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task<TicketEditDto> EditTicket(TicketEditDto model)
        {
            if (model.OrderLineId.HasValue)
            {
                await EnsureCanAddTickets(model.OrderLineId.Value, OfficeId);
            }
            await _ticketRepository.EnsureCanEditTicket(model.Id);

            Ticket ticket;
            if (model.Id != 0)
            {
                ticket = await _ticketRepository.GetAsync(model.Id);
            }
            else
            {
                ticket = new Ticket()
                {
                    Id = model.Id,
                };
            }
            ticket.OrderLineId = model.OrderLineId;
            ticket.OfficeId = OfficeId;
            ticket.TicketNumber = model.TicketNumber;
            ticket.TicketDateTime = model.TicketDateTime?.ConvertTimeZoneFrom(await GetTimezone());
            ticket.Shift = model.OrderLineId.HasValue ? null : model.Shift;
            ticket.CustomerId = model.CustomerId;
            ticket.CarrierId = model.CarrierId;
            var quantityWasChanged = ticket.Quantity != model.Quantity;
            ticket.Quantity = model.Quantity;
            ticket.ServiceId = model.ServiceId;
            ticket.TruckCode = model.TruckCode;
            ticket.TruckId = await GetTruckId(model.TruckCode);
            ticket.TrailerId = model.TrailerId;
            ticket.DriverId = model.DriverId;
            if (!model.OrderLineId.HasValue)
            {
                ticket.UnitOfMeasureId = model.UomId;
            }
            ticket.DeliverToId = model.DeliverToId;
            ticket.LoadAtId = model.LoadAtId;
            ticket.IsVerified = model.IsVerified;
            ticket.IsBilled = model.IsBilled;

            if (ticket.TruckId == null && !(model.CarrierId > 0) && !await SettingManager.AllowCounterSalesForTenant())
            {
                throw new UserFriendlyException($"Invalid truck number");
            }
            if (ticket.DriverId == null && !(model.CarrierId > 0) && !await SettingManager.AllowCounterSalesForTenant())
            {
                throw new UserFriendlyException("Driver is required");
            }
            model.Id = await _ticketRepository.InsertOrUpdateAndGetIdAsync(ticket);

            if (model.OrderLineId.HasValue)
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                await _orderTaxCalculator.CalculateTotalsForOrderLineAsync(model.OrderLineId.Value);
                if (quantityWasChanged)
                {
                    await _fuelSurchargeCalculator.RecalculateTicket(ticket.Id);
                }
            }

            return model;
        }

        public async Task<AddTicketPhotoResult> AddTicketPhoto(AddTicketPhotoInput input)
        {
            var dataId = await _binaryObjectManager.UploadDataUriStringAsync(input.TicketPhoto, AbpSession.TenantId ?? 0);
            if (dataId == null)
            {
                throw new UserFriendlyException("Ticket photo is required");
            }

            await _ticketRepository.EnsureCanEditTicket(input.TicketId);

            var ticket = await _ticketRepository.GetAsync(input.TicketId);
            if (ticket.TicketPhotoId.HasValue)
            {
                //throw new UserFriendlyException("Ticket already has a photo associated with it");
                await _binaryObjectManager.DeleteAsync(ticket.TicketPhotoId.Value);
            }
            ticket.TicketPhotoId = dataId;
            ticket.TicketPhotoFilename = input.TicketPhotoFilename;

            return new AddTicketPhotoResult
            {
                TicketPhotoId = dataId.Value
            };
        }

        public async Task DeleteTicketPhoto(DeleteTicketPhotoInput input)
        {
            await _ticketRepository.EnsureCanEditTicket(input.TicketId);

            var ticket = await _ticketRepository.GetAsync(input.TicketId);
            if (ticket.TicketPhotoId.HasValue)
            {
                await _binaryObjectManager.DeleteAsync(ticket.TicketPhotoId.Value);
            }
            ticket.TicketPhotoId = null;
            ticket.TicketPhotoFilename = null;
        }

        public async Task<TicketPhotoDto> GetTicketPhoto(int ticketId)
        {
            var ticket = await _ticketRepository.GetAll()
                .Where(x => x.Id == ticketId)
                .Select(x => new
                {
                    x.TicketNumber,
                    x.TicketPhotoFilename,
                    x.TicketPhotoId
                }).FirstOrDefaultAsync();

            if (ticket?.TicketPhotoId == null)
            {
                return new TicketPhotoDto();
            }

            var image = await _binaryObjectManager.GetOrNullAsync(ticket.TicketPhotoId.Value);
            if (image?.Bytes?.Length > 0)
            {
                return new TicketPhotoDto
                {
                    FileBytes = image.Bytes,
                    Filename = ticket.TicketPhotoFilename ?? (ticket.TicketPhotoId + ".jpg")
                };
            }
            return new TicketPhotoDto();
        }

        public async Task<bool> InvoiceHasTicketPhotos(int invoiceId)
        {
            return await _ticketRepository.GetAll()
                .AnyAsync(x => x.InvoiceLine.InvoiceId == invoiceId && x.TicketPhotoId != null);
        }

        public async Task<TicketPhotoDto> GetTicketPhotosForInvoice(int invoiceId)
        {
            var ticketList = await _ticketRepository.GetAll()
                .Where(x => x.InvoiceLine.InvoiceId == invoiceId && x.TicketPhotoId != null)
                .Select(x => new
                {
                    x.TicketNumber,
                    x.TicketPhotoFilename,
                    x.TicketPhotoId
                }).ToListAsync();

            if (!ticketList.Any())
            {
                return new TicketPhotoDto();
            }

            using (var zipStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Update, true))
                {
                    foreach (var ticket in ticketList)
                    {
                        var image = await _binaryObjectManager.GetOrNullAsync(ticket.TicketPhotoId.Value);
                        if (image?.Bytes?.Length > 0)
                        {
                            var filename = ticket.TicketPhotoFilename ?? (ticket.TicketPhotoId + ".jpg");
                            var entry = archive.CreateEntry(filename, CompressionLevel.NoCompression);
                            using (var entryStream = entry.Open())
                            {
                                entryStream.Write(image.Bytes);
                            }
                        }
                    }
                }

                return new TicketPhotoDto
                {
                    FileBytes = zipStream.ToArray(),
                    Filename = $"TicketsForInvoice{invoiceId}.zip"
                };
            }
        }

        public async Task<string> CheckTruckIsOutofServiceOrInactive(TicketEditDto model)
        {
            string result = string.Empty;
            var truck = await _truckRepository.GetAll()
                .FirstOrDefaultAsync(t => t.TruckCode == model.TruckCode);

            if (truck != null && model.IsBilled)
            {
                if (truck.Id > 0 && truck.IsOutOfService)
                {
                    result = "out of service";
                }

                if (truck.Id > 0 && !truck.IsActive)
                {
                    result = result != "" ? (result + " and inactive") : "inactive";
                }
            }

            return result;
        }

        private async Task<int?> GetTruckId(string truckCode)
        {
            return (await _truckRepository.GetAll()
                .Where(t => t.TruckCode == truckCode)
                .Select(t => new { t.Id })
                .FirstOrDefaultAsync())?.Id;
        }


        public async Task<IList<TicketOrderLineDto>> LookForExistingOrderLines(LookForExistingOrderLinesInput input)
        {
            var orderLines = await _orderLineRepository.GetAll()
                .Where(ol => ol.Order.DeliveryDate == input.TicketDateTime.Date)
                .Where(ol => ol.Order.CustomerId == input.CustomerId)
                .Where(ol => ol.ServiceId == input.ServiceId)
                .Where(ol => ol.OrderLineTrucks.Any(olt => olt.Truck.TruckCode == input.TruckCode))
                .Select(ol => new
                {
                    ol.Id,
                    ol.Order.DeliveryDate,
                    ol.Order.Customer.Name,
                })
                .ToListAsync();

            return orderLines
                .Select(ol => new TicketOrderLineDto
                {
                    Id = ol.Id,
                    Title = $"{ol.DeliveryDate:d} {ol.Name}",
                }).ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_View)]
        public async Task<PagedResultDto<TicketListItemViewModel>> LoadTicketsByOrderLineId(int orderLineId)
        {
            var query = _ticketRepository.GetAll()
                .Where(t => t.OrderLineId == orderLineId)
                .WhereIf(await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.SplitBillingByOffices), t => t.OfficeId == OfficeId)
                ;
            int totalCount = await query.CountAsync();
            var items = await query
                .Select(t => new TicketListItemViewModel
                {
                    Id = t.Id,
                    OrderLineId = t.OrderLineId,
                    TicketNumber = t.TicketNumber,
                    TicketDateTime = t.TicketDateTime,
                    Quantity = t.Quantity,
                    UomName = t.UnitOfMeasure.Name,
                    TruckId = t.TruckId,
                    TruckCode = t.Truck.TruckCode != null ? t.Truck.TruckCode : t.TruckCode,
                    TruckCanPullTrailer = t.Truck.CanPullTrailer,
                    TrailerId = t.TrailerId,
                    TrailerTruckCode = t.Trailer.TruckCode,
                    DriverId = t.DriverId,
                    DriverName = t.Driver.FirstName + " " + t.Driver.LastName,
                    TicketPhotoId = t.TicketPhotoId,
                    ReceiptLineId = t.ReceiptLineId
                })
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var timeZone = await GetTimezone();
            foreach (var item in items)
            {
                item.TicketDateTime = item.TicketDateTime?.ConvertTimeZoneTo(timeZone);
            }

            return new PagedResultDto<TicketListItemViewModel>(
                totalCount,
                items);
        }

        public async Task<bool> CanDeleteTicket(EntityDto input)
        {

            var ticketDetails = await _ticketRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    HasInvoiceLines = x.InvoiceLine != null,
                    HasReceiptLines = x.ReceiptLineId != null,
                    HasPayStatements = x.PayStatementTickets.Any(),
                    HasLeaseHaulerStatements = x.LeaseHaulerStatementTicket != null
                }).FirstAsync();

            if (ticketDetails.HasInvoiceLines || ticketDetails.HasReceiptLines || ticketDetails.HasPayStatements || ticketDetails.HasLeaseHaulerStatements)
            {
                return false;
            }

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task<DeleteTicketOutput> DeleteTicket(EntityDto input)
        {
            var canDelete = await CanDeleteTicket(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }

            var ticket = await _ticketRepository.GetAsync(input.Id);

            await _ticketRepository.DeleteAsync(ticket);

            IOrderTaxDetails orderTaxDetails = null;

            if (ticket.OrderLineId.HasValue)
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                orderTaxDetails = await _orderTaxCalculator.CalculateTotalsForOrderLineAsync(ticket.OrderLineId.Value);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            if (ticket.MaterialCompanyTicketId != null
                && ticket.MaterialCompanyTenantId != null)
            {
                using (CurrentUnitOfWork.SetTenantId(ticket.MaterialCompanyTenantId.Value))
                {
                    var destinationTicket = await _ticketRepository.GetAll()
                        .Where(x => x.Id == ticket.MaterialCompanyTicketId.Value)
                        .FirstOrDefaultAsync();
                    if (destinationTicket != null)
                    {
                        destinationTicket.HaulingCompanyTicketId = null;
                        destinationTicket.HaulingCompanyTenantId = null;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }
            if (ticket.HaulingCompanyTicketId != null
                && ticket.HaulingCompanyTenantId != null)
            {
                using (CurrentUnitOfWork.SetTenantId(ticket.HaulingCompanyTenantId.Value))
                {
                    var destinationTicket = await _ticketRepository.GetAll()
                        .Where(x => x.Id == ticket.HaulingCompanyTicketId.Value)
                        .FirstOrDefaultAsync();
                    if (destinationTicket != null)
                    {
                        destinationTicket.MaterialCompanyTicketId = null;
                        destinationTicket.MaterialCompanyTenantId = null;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }

            return new DeleteTicketOutput
            {
                OrderTaxDetails = orderTaxDetails != null ? new OrderTaxDetailsDto(orderTaxDetails) : null
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task MarkAsBilledTicket(EntityDto input)
        {
            var ticket = await _ticketRepository.GetAsync(input.Id);
            ticket.IsBilled = true;
            await _ticketRepository.UpdateAsync(ticket);
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task EditTicketFromList(EditTicketFromListInput input)
        {
            var ticket = await _ticketRepository.GetAsync(input.Id);
            ticket.IsVerified = input.IsVerified;
            await _ticketRepository.UpdateAsync(ticket);
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_View, AppPermissions.CustomerPortal_TicketList)]
        public async Task<PagedResultDto<TicketListViewDto>> TicketListView(TicketListInput input)
        {
            var permissions = new
            {
                ViewAnyTickets = await IsGrantedAsync(AppPermissions.Pages_Tickets_View),
                ViewCustomerTicketsOnly = await IsGrantedAsync(AppPermissions.CustomerPortal_TicketList),
            };

            if (permissions.ViewAnyTickets)
            {
                //do not additionally filter the data
            }
            else if (permissions.ViewCustomerTicketsOnly)
            {
                input.CustomerId = Session.GetCustomerIdOrThrow(this);
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            var query = GetTicketListQuery(input, await GetTimezone());

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            var shiftDictionary = await SettingManager.GetShiftDictionary();
            items.ForEach(x => x.Shift = x.ShiftRaw.HasValue && shiftDictionary.ContainsKey(x.ShiftRaw.Value) ? shiftDictionary[x.ShiftRaw.Value] : "");

            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();
            items.ForEach(x =>
            {
                OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, x, x.SalesTaxRate ?? 0);
            });

            if (!permissions.ViewAnyTickets && permissions.ViewCustomerTicketsOnly)
            {
                items.ForEach(x =>
                {
                    x.Revenue = 0; //revenue is hidden from customer portal users.
                });
            }

            return new PagedResultDto<TicketListViewDto>(
                totalCount,
                items);
        }



        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        [HttpPost]
        public async Task<FileDto> GetTicketsToCsv(TicketListInput input)
        {
            var timezone = await GetTimezone();
            var query = GetTicketListQuery(input, timezone);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (items.Count == 0)
            {
                throw new UserFriendlyException("There is no data to export!");
            }

            var shiftDictionary = await SettingManager.GetShiftDictionary();
            items.ForEach(x => x.Shift = x.ShiftRaw.HasValue && shiftDictionary.ContainsKey(x.ShiftRaw.Value) ? shiftDictionary[x.ShiftRaw.Value] : "");
            items.ForEach(x => x.Date = x.Date?.ConvertTimeZoneTo(timezone));

            var filename = "TicketList.csv";

            if (input.InvoiceId.HasValue)
            {
                var invoiceDetails = await _invoiceRepository.GetAll()
                    .Where(x => x.Id == input.InvoiceId)
                    .Select(x => new
                    {
                        CustomerName = x.Customer.Name
                    }).FirstOrDefaultAsync();
                filename = $"{invoiceDetails?.CustomerName}InvoiceNumber{input.InvoiceId}.csv";
            }

            return _ticketListCsvExporter.ExportToFile(items, filename);
        }

        private IQueryable<TicketListViewDto> GetTicketListQuery(TicketListInput input, string timezone)
        {
            var ticketDateRangeBegin = input.TicketDateRangeBegin?.ConvertTimeZoneFrom(timezone);
            var ticketDateRangeEnd = input.TicketDateRangeEnd?.AddDays(1).ConvertTimeZoneFrom(timezone);
            var orderDateRangeBegin = input.OrderDateRangeBegin;
            var orderDateRangeEnd = input.OrderDateRangeEnd?.AddDays(1);

            var query = _ticketRepository.GetAll();

            if (input.TicketStatus == TicketListStatusFilterEnum.PotentialDuplicateTickets)
            {
                query = (from ticket in query
                         join otherTicket in _ticketRepository.GetAll()
                         on new { ticket.TicketNumber, ticket.LoadAtId, ticket.DeliverToId }
                         equals new { otherTicket.TicketNumber, otherTicket.LoadAtId, otherTicket.DeliverToId }
                         where otherTicket != null && otherTicket.Id != ticket.Id
                         select ticket)
                        .Distinct();
            }

            return query
                .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId.Value)
                .WhereIf(input.InvoiceId.HasValue, x => x.InvoiceLine.InvoiceId == input.InvoiceId.Value)
                .WhereIf(input.CarrierId.HasValue, x => x.CarrierId == input.CarrierId)
                .WhereIf(input.ServiceId.HasValue, x => x.ServiceId == input.ServiceId)
                .WhereIf(input.DriverId.HasValue, x => x.DriverId == input.DriverId)
                .WhereIf(ticketDateRangeBegin.HasValue, x => x.TicketDateTime >= ticketDateRangeBegin)
                .WhereIf(ticketDateRangeEnd.HasValue, x => x.TicketDateTime < ticketDateRangeEnd)
                .WhereIf(orderDateRangeBegin.HasValue, x => x.OrderLine.Order.DeliveryDate >= orderDateRangeBegin)
                .WhereIf(orderDateRangeEnd.HasValue, x => x.OrderLine.Order.DeliveryDate < orderDateRangeEnd)
                .WhereIf(!string.IsNullOrEmpty(input.TicketNumber), x => x.TicketNumber.Contains(input.TicketNumber))
                .WhereIf(input.TruckId.HasValue, x => x.TruckId == input.TruckId)
                .WhereIf(!string.IsNullOrEmpty(input.JobNumber), x => x.OrderLine.JobNumber == input.JobNumber)
                .WhereIf(!input.Shifts.IsNullOrEmpty() && !input.Shifts.Contains(Shift.NoShift), t => t.Shift.HasValue && input.Shifts.Contains(t.Shift.Value) ||
                                                            t.OrderLine.Order.Shift.HasValue && input.Shifts.Contains(t.OrderLine.Order.Shift.Value))
                .WhereIf(!input.Shifts.IsNullOrEmpty() && input.Shifts.Contains(Shift.NoShift), t => !t.Shift.HasValue && !t.OrderLineId.HasValue || input.Shifts.Contains(t.Shift.Value) ||
                                                            t.OrderLineId.HasValue && !t.OrderLine.Order.Shift.HasValue || input.Shifts.Contains(t.OrderLine.Order.Shift.Value))
                .WhereIf(input.BillingStatus.HasValue, x => x.IsBilled == input.BillingStatus)
                .WhereIf(input.IsVerified.HasValue, x => x.IsVerified == input.IsVerified)
                .WhereIf(input.CustomerId.HasValue, x => x.Customer.Id == input.CustomerId)
                .WhereIf(input.TicketStatus == TicketListStatusFilterEnum.MissingTicketsOnly, x => string.IsNullOrEmpty(x.TicketNumber) || x.Quantity == 0)
                .WhereIf(input.TicketStatus == TicketListStatusFilterEnum.EnteredTicketsOnly, x => !string.IsNullOrEmpty(x.TicketNumber) && x.Quantity != 0)
                .WhereIf(input.TicketIds?.Any() == true, x => input.TicketIds.Contains(x.Id))
                .WhereIf(input.OrderId.HasValue, x => x.OrderLine.OrderId == input.OrderId)
                .WhereIf(input.IsImported.HasValue, x => x.IsImported == input.IsImported)
                .WhereIf(input.LoadAtId.HasValue, x => x.LoadAtId == input.LoadAtId)
                .WhereIf(input.DeliverToId.HasValue, x => x.DeliverToId == input.DeliverToId)
                .Select(t => new TicketListViewDto
                {
                    Id = t.Id,
                    IsVerified = t.IsVerified,
                    Date = t.TicketDateTime,
                    OrderDate = t.OrderLine.Order.DeliveryDate,
                    ShiftRaw = t.OrderLineId.HasValue ? t.OrderLine.Order.Shift : t.Shift,
                    Office = t.Office.Name,
                    CustomerName = t.Customer != null ? t.Customer.Name : "",
                    QuoteName = t.OrderLine.Order.Quote.Name,
                    JobNumber = t.OrderLine.JobNumber,
                    Product = t.Service.Service1,
                    TicketNumber = t.TicketNumber,
                    Quantity = t.Quantity,
                    Uom = t.UnitOfMeasure != null ? t.UnitOfMeasure.Name : "",
                    Carrier = t.Carrier != null ? t.Carrier.Name : "",
                    Truck = t.Truck.TruckCode ?? t.TruckCode,
                    Trailer = t.Trailer.TruckCode,
                    DriverName = t.Driver == null ? null : t.Driver.LastName + ", " + t.Driver.FirstName,
                    IsBilled = t.IsBilled,
                    TicketPhotoId = t.TicketPhotoId,
                    InvoiceLineId = (int?)t.InvoiceLine.Id,
                    HasPayStatements = t.PayStatementTickets.Any(),
                    HasLeaseHaulerStatements = t.LeaseHaulerStatementTicket != null,
                    ReceiptLineId = t.ReceiptLineId,
                    LoadAtNamePlain = t.LoadAt.Name + ", " + t.LoadAt.StreetAddress + ", " + t.LoadAt.City + ", " + t.LoadAt.State, //for sorting
                    LoadAt = t.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = t.LoadAt.Name,
                        StreetAddress = t.LoadAt.StreetAddress,
                        City = t.LoadAt.City,
                        State = t.LoadAt.State
                    },
                    DeliverToNamePlain = t.DeliverTo.Name + ", " + t.DeliverTo.StreetAddress + ", " + t.DeliverTo.City + ", " + t.DeliverTo.State, //for sorting
                    DeliverTo = t.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = t.DeliverTo.Name,
                        StreetAddress = t.DeliverTo.StreetAddress,
                        City = t.DeliverTo.City,
                        State = t.DeliverTo.State
                    },
                    Designation = t.OrderLine.Designation,
                    MaterialUomId = t.OrderLine.MaterialUomId,
                    FreightUomId = t.OrderLine.FreightUomId,
                    TicketUomId = t.UnitOfMeasureId,
                    MaterialRate = t.OrderLine.MaterialPricePerUnit,
                    FreightRate = t.OrderLine.FreightPricePerUnit,
                    FreightRateToPayDrivers = t.OrderLine.FreightRateToPayDrivers,
                    FuelSurcharge = t.FuelSurcharge,
                    IsTaxable = t.Service.IsTaxable,
                    SalesTaxRate = t.OrderLine.Order.SalesTaxRate,
                    Revenue = t.Quantity * ((t.OrderLine.MaterialPricePerUnit ?? 0) + (t.OrderLine.FreightPricePerUnit ?? 0)),
                    IsFreightPriceOverridden = t.OrderLine.IsFreightPriceOverridden,
                    IsMaterialPriceOverridden = t.OrderLine.IsMaterialPriceOverridden,
                    OrderLineFreightPrice = t.OrderLine.FreightPrice,
                    OrderLineMaterialPrice = t.OrderLine.MaterialPrice,
                    IsImported = t.IsImported,
                    ProductionPay = t.OrderLine.ProductionPay,
                    PayStatementId = t.PayStatementTickets.Select(x => x.PayStatementDetail.PayStatementId).First()
                });
            /*.WhereIf(!input.LoadAt.IsNullOrEmpty(), x => x.LoadAtNamePlain == input.LoadAt)
            .WhereIf(!input.DeliverTo.IsNullOrEmpty(), x => x.DeliverToNamePlain == input.DeliverTo);*/
        }

        [HttpPost]
        public async Task<TicketsByDriverResult> GetTicketsByDriver(GetTicketsByDriverInput input)
        {
            var result = new TicketsByDriverResult
            {
            };

            var orderLineQuery = _orderLineRepository.GetAll()
                .Where(x => x.Order.DeliveryDate == input.Date)
                .Select(x => new TicketsByDriverResult.OrderLineDto
                {
                    Id = x.Id,
                    Shift = x.Order.Shift,
                    OrderId = x.OrderId,
                    JobNumber = x.JobNumber,
                    CustomerId = x.Order.CustomerId,
                    CustomerName = x.Order.Customer.Name,
                    OrderDate = x.Order.DeliveryDate,
                    LoadAtId = x.LoadAtId,
                    DeliverToId = x.DeliverToId,
                    LoadAt = x.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = x.LoadAt.Name,
                        StreetAddress = x.LoadAt.StreetAddress,
                        City = x.LoadAt.City,
                        State = x.LoadAt.State
                    },
                    DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = x.DeliverTo.Name,
                        StreetAddress = x.DeliverTo.StreetAddress,
                        City = x.DeliverTo.City,
                        State = x.DeliverTo.State
                    },
                    ServiceId = x.ServiceId,
                    ServiceName = x.Service.Service1,
                    Designation = x.Designation,
                    MaterialUomId = x.MaterialUomId,
                    MaterialUomName = x.MaterialUom.Name,
                    FreightUomId = x.FreightUomId,
                    FreightUomName = x.FreightUom.Name,
                    FreightRate = x.FreightPricePerUnit,
                    FreightRateToPayDrivers = x.FreightRateToPayDrivers,
                    MaterialRate = x.MaterialPricePerUnit,
                    FuelSurchargeRate = x.FuelSurchargeRate,
                    MaterialTotal = x.MaterialPrice,
                    FreightTotal = x.FreightPrice,
                    IsMaterialTotalOverridden = x.IsMaterialPriceOverridden,
                    IsFreightTotalOverridden = x.IsFreightPriceOverridden,
                    Note = x.Note,
                    OrderLineTrucks = x.OrderLineTrucks.Select(t => new TicketsByDriverResult.OrderLineTruckDto
                    {
                        Id = t.Id,
                        TruckId = t.TruckId,
                        TrailerId = t.TrailerId,
                        DriverId = t.DriverId,
                        DriverNote = t.DriverNote,
                    }).ToList(),
                    IsComplete = x.IsComplete,
                    IsCancelled = x.IsCancelled
                });

            if (await orderLineQuery.AnyAsync(x => !x.IsComplete))
            {
                var today = await GetToday();
                if (input.Date < today)
                {
                    //for past orders: close open orders if no open dispatches, otherwise show a warning
                    result.HasOpenOrders = await ClosePastOrdersIfNoDispatches(input);
                }
                else
                {
                    //for today's orders: just show a warning
                    result.HasOpenOrders = true;
                }
            }

            result.OrderLines = await orderLineQuery.Where(x => x.IsComplete).ToListAsync();
            result.OrderLines.ForEach(ol =>
            {
                ol.UomId = ol.Designation.HasMaterial() ? ol.MaterialUomId : ol.FreightUomId;
                ol.UomName = ol.Designation.HasMaterial() ? ol.MaterialUomName : ol.FreightUomName;
            });

            result.Trucks = await _truckRepository.GetAll()
                .Select(x => new TicketsByDriverResult.TruckDto
                {
                    Id = x.Id,
                    TruckCode = x.TruckCode,
                    IsActive = x.IsActive,
                    CanPullTrailer = x.CanPullTrailer,
                    VehicleCategory = new TicketsByDriverResult.VehicleCategoryDto
                    {
                        AssetType = x.VehicleCategory.AssetType,
                    },
                    LeaseHaulerId = (int?)x.LeaseHaulerTruck.LeaseHaulerId,
                    DefaultDriverId = x.DefaultDriverId,
                    CurrentTrailerId = x.CurrentTrailerId,
                }).ToListAsync();

            result.DriverAssignments = await _driverAssignmentRepository.GetAll()
                .Where(x => x.Date == input.Date)
                .Select(x => new TicketsByDriverResult.DriverAssignmentDto
                {
                    Id = x.Id,
                    Shift = x.Shift,
                    TruckId = x.TruckId,
                    DriverId = x.DriverId
                }).ToListAsync();

            result.Tickets = await _ticketRepository.GetAll()
                .Where(x => x.OrderLineId.HasValue && x.OrderLine.Order.DeliveryDate == input.Date)
                .Select(t => new TicketsByDriverResult.TicketDto
                {
                    Id = t.Id,
                    OrderLineId = t.OrderLineId,
                    CarrierId = t.CarrierId,
                    TicketNumber = t.TicketNumber,
                    TicketDateTime = t.TicketDateTime,
                    Quantity = t.Quantity,
                    UomId = t.UnitOfMeasureId,
                    UomName = t.UnitOfMeasure.Name,
                    TruckId = t.TruckId,
                    TruckCode = t.TruckCode,
                    TrailerId = t.TrailerId,
                    TrailerTruckCode = t.Trailer.TruckCode,
                    DriverId = t.DriverId,
                    TicketPhotoId = t.TicketPhotoId,
                    ReceiptLineId = t.ReceiptLineId,
                    IsVerified = t.IsVerified,
                    IsReadOnly = t.InvoiceLine != null //already invoiced
                                || t.PayStatementTickets.Any() //already added to pay statements
                                || t.ReceiptLineId != null //already added to receipts
                                || t.LeaseHaulerStatementTicket != null //added to lease hauler statements
                })
                .OrderBy(x => x.Id)
                .ToListAsync();

            var timeZone = await GetTimezone();
            foreach (var ticket in result.Tickets)
            {
                ticket.TicketDateTime = ticket.TicketDateTime?.ConvertTimeZoneTo(timeZone);
            }

            //var driverIds = result.Tickets.Where(x => x.DriverId.HasValue).Select(x => x.DriverId.Value)
            //    .Union(
            //        result.OrderLines.SelectMany(o => o.OrderLineTrucks.Select(olt => olt.DriverId)).Where(x => x.HasValue).Select(x => x.Value)
            //    ).Distinct()
            //    .ToList();

            //var leaseHaulerIds = await _driverRepository.GetAll()
            //    .Where(x => x.IsExternal && driverIds.Contains(x.Id))
            //    .Select(x => (int?)x.LeaseHaulerDriver.LeaseHaulerId)
            //    .Where(x => x.HasValue)
            //    .Select(x => x.Value)
            //    .Distinct()
            //    .ToListAsync();

            result.Drivers = await _driverRepository.GetAll()
                .Select(x => new TicketsByDriverResult.DriverDto
                {
                    Id = x.Id,
                    Name = x.LastName + ", " + x.FirstName,
                    IsActive = !x.IsInactive,
                    IsExternal = x.IsExternal,
                    LeaseHaulerId = (int?)x.LeaseHaulerDriver.LeaseHaulerId
                })
                //.Where(x => !x.IsExternal || driverIds.Contains(x.Id) || x.LeaseHaulerId != null && leaseHaulerIds.Contains(x.LeaseHaulerId.Value))
                .OrderBy(d => d.Name)
                .ToListAsync();

            result.LeaseHaulers = await _leaseHaulerRepository.GetAll()
                .Select(x => new TicketsByDriverResult.LeaseHaulerDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .OrderBy(d => d.Name)
                .ToListAsync();

            result.DailyFuelCost = await _dailyFuelCostRepository.GetAll()
                .Where(x => x.Date < input.Date)
                .Select(x => new TicketsByDriverResult.DailyFuelCostDto
                {
                    Date = x.Date,
                    Cost = x.Cost,
                })
                .OrderByDescending(x => x.Date)
                .FirstOrDefaultAsync();

            return result;
        }

        /// <returns>true if there are still open dispatches; false if there are no open dispathes and the orders have been closed</returns>
        private async Task<bool> ClosePastOrdersIfNoDispatches(GetTicketsByDriverInput input)
        {
            var hasDispatches = await _dispatchRepository.GetAll()
                .AnyAsync(x => x.OrderLine.Order.DeliveryDate == input.Date
                    && Dispatch.OpenStatuses.Contains(x.Status));

            if (hasDispatches)
            {
                return true;
            }

            var orderLines = await _orderLineRepository.GetAll()
                .Include(ol => ol.OrderLineTrucks)
                .Where(x => x.Order.DeliveryDate == input.Date && !x.IsComplete)
                .ToListAsync();

            foreach (var orderLine in orderLines)
            {
                orderLine.IsComplete = true;

                foreach (var orderLineTruck in orderLine.OrderLineTrucks)
                {
                    orderLineTruck.IsDone = true;
                    orderLineTruck.Utilization = 0;
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return false;
        }

        public async Task<TicketsByDriverResult> EditTicketsByDriver(TicketsByDriverResult model)
        {
            var timezone = await GetTimezone();
            var orderLines = new List<OrderLine>();
            if (model.Tickets?.Any() == true)
            {
                var ticketIds = model.Tickets.Select(x => x.Id).Where(x => x != 0).ToList();
                var tickets = await _ticketRepository.GetAll().Where(x => ticketIds.Contains(x.Id)).ToListAsync();

                var orderLineIds = model.Tickets.Select(x => x.OrderLineId).Where(x => !orderLines.Any(o => o.Id == x)).Distinct().ToList();
                if (orderLineIds.Any())
                {
                    orderLines.AddRange(await _orderLineRepository.GetAll()
                        .Include(x => x.Order)
                        .Include(x => x.OrderLineTrucks)
                        .Where(x => orderLineIds.Contains(x.Id))
                        .ToListAsync());
                }

                var truckIds = model.Tickets.Select(x => x.TruckId).Where(x => x > 0).Distinct().ToList();
                var trucks = await _truckRepository.GetAll()
                    .Where(x => truckIds.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.TruckCode,
                        LeaseHaulerId = (int?)x.LeaseHaulerTruck.LeaseHaulerId
                    })
                    .ToListAsync();

                foreach (var ticketModel in model.Tickets)
                {
                    var ticket = ticketModel.Id != 0 ? tickets.FirstOrDefault(x => x.Id == ticketModel.Id) : new Ticket();
                    if (ticket == null)
                    {
                        throw new ApplicationException($"Ticket with id {ticketModel.Id} wasn't found");
                    }

                    if (ticketModel.OrderLineId == 0)
                    {
                        throw new ApplicationException("Ticket must have OrderLineId set");
                    }
                    var orderLine = orderLines.FirstOrDefault(x => x.Id == ticketModel.OrderLineId);
                    if (orderLine == null)
                    {
                        throw new ApplicationException($"OrderLine with id {ticketModel.OrderLineId} wasn't found");
                    }

                    if (ticket.Id == 0)
                    {
                        await EnsureCanAddTickets(ticketModel.OrderLineId.Value, OfficeId);
                    }
                    if (!await IsGrantedAsync(AppPermissions.EditInvoicedOrdersAndTickets))
                    {
                        await _ticketRepository.EnsureCanEditTicket(ticketModel.Id);
                    }

                    //var driverOrTruckHasChanged = false;
                    if (ticket.DriverId != ticketModel.DriverId)
                    {
                        //driverOrTruckHasChanged = true;
                        ticket.DriverId = ticketModel.DriverId;
                        await ThrowIfDriverHasTimeOffRequests(ticket.DriverId, orderLine.Order.DeliveryDate);
                    }
                    ticket.TicketNumber = ticketModel.TicketNumber;
                    var quantityWasChanged = ticket.Quantity != ticketModel.Quantity;
                    ticket.Quantity = ticketModel.Quantity;
                    ticket.IsVerified = ticketModel.IsVerified;
                    if (ticket.TruckId != ticketModel.TruckId)
                    {
                        //driverOrTruckHasChanged = true;
                        ticket.TruckId = ticketModel.TruckId;
                        if (ticket.TruckId.HasValue)
                        {
                            var truck = trucks.FirstOrDefault(x => x.Id == ticket.TruckId);
                            ticket.TruckCode = truck?.TruckCode;
                            ticketModel.TruckCode = ticket.TruckCode;
                            ticket.CarrierId = truck?.LeaseHaulerId;
                        }
                    }
                    ticket.TrailerId = ticketModel.TrailerId;
                    if (ticketModel.TicketDateTime.HasValue)
                    {
                        ticket.TicketDateTime = ticketModel.TicketDateTime.Value.ConvertTimeZoneFrom(timezone);
                    }
                    else
                    {
                        var newDateWithTime = orderLine.Order.DeliveryDate;
                        ticket.TicketDateTime = newDateWithTime?.ConvertTimeZoneFrom(timezone);
                        ticketModel.TicketDateTime = newDateWithTime;
                    }

                    //if (driverOrTruckHasChanged)
                    //{
                    //    if (ticket.TruckId != null)
                    //    {
                    //        if (!orderLine.OrderLineTrucks.Any(olt => olt.TruckId == ticket.TruckId))
                    //        {
                    //            var orderLineTruck = new OrderLineTruck
                    //            {
                    //                OrderLineId = ticket.OrderLineId.Value,
                    //                TruckId = ticket.TruckId.Value,
                    //                DriverId = 
                    //                IsDone = true,
                    //                Utilization = 0,
                    //            };
                    //            _orderLineTruckRepository.Insert(orderLineTruck);
                    //            if (!orderLine.OrderLineTrucks.Contains(orderLineTruck))
                    //            {
                    //                orderLine.OrderLineTrucks.Add(orderLineTruck);
                    //            }

                    //            //if (orderLine.Order.DateTime >= await GetToday())
                    //            //{
                    //            //    var existingDriverAssignment = await _driverAssignmentRepository.GetAll()
                    //            //        .Where(da => da.TruckId == ticket.TruckId && da.Date == orderLine.Order.DateTime && da.Shift == orderLine.Order.Shift)
                    //            //        .FirstOrDefaultAsync();
                    //            //    if (existingDriverAssignment == null)
                    //            //    {
                    //            //        var truck = await _truckRepository.GetAll()
                    //            //            .Where(x => x.Id == ticket.TruckId)
                    //            //            .Select(x => new { x.VehicleCategory.IsPowered, x.IsEmbedded })
                    //            //            .FirstOrDefaultAsync();
                    //            //        if (truck != null && truck.IsPowered &&
                    //            //            (!truck.IsEmbedded || await FeatureChecker.AllowLeaseHaulersFeature()))
                    //            //        {
                    //            //            var newDriverAssignment = new DriverAssignment
                    //            //            {
                    //            //                Date = orderLine.Order.DateTime.Value,
                    //            //                Shift = orderLine.Order.Shift,
                    //            //                DriverId = ticket.DriverId,
                    //            //                OfficeId = orderLine.Order.LocationId,
                    //            //                TruckId = ticket.TruckId.Value,
                    //            //            };
                    //            //            await _driverAssignmentRepository.InsertAsync(newDriverAssignment);
                    //            //        }
                    //            //    }
                    //            //}
                    //        }
                    //    }
                    //}

                    if (ticketModel.Id == 0)
                    {
                        ticket.OrderLineId = orderLine.Id;
                        ticket.LoadAtId = orderLine.LoadAtId;
                        ticket.DeliverToId = orderLine.DeliverToId;
                        ticket.CustomerId = orderLine.Order.CustomerId;
                        ticket.OfficeId = orderLine.Order.LocationId;
                        ticket.ServiceId = orderLine.ServiceId;
                        ticket.UnitOfMeasureId = orderLine.Designation.HasMaterial() ? orderLine.MaterialUomId : orderLine.FreightUomId;

                        ticketModel.Id = await _ticketRepository.InsertAndGetIdAsync(ticket);
                    }

                    if (quantityWasChanged)
                    {
                        await CurrentUnitOfWork.SaveChangesAsync();
                        await _fuelSurchargeCalculator.RecalculateTicket(ticket.Id);
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }


            var changedTickets = new List<Ticket>();
            if (model.OrderLines?.Any() == true)
            {
                foreach (var orderLineModel in model.OrderLines)
                {
                    var orderLine = await _orderLineRepository.GetAll()
                        .Include(x => x.Tickets)
                        .FirstOrDefaultAsync(x => x.Id == orderLineModel.Id);

                    if (orderLine == null)
                    {
                        throw new ApplicationException($"OrderLine with id {orderLineModel.Id} wasn't found");
                    }

                    var orderLineTickets = orderLine.Tickets.ToList();
                    //if (orderLine.Order.CustomerId != orderLineModel.CustomerId)
                    // we disallowed to change CustomerId from tickets by driver view per #10977
                    //}
                    if (orderLine.LoadAtId != orderLineModel.LoadAtId)
                    {
                        orderLine.LoadAtId = orderLineModel.LoadAtId;
                        orderLineTickets.ForEach(t => t.LoadAtId = orderLineModel.LoadAtId);
                    }
                    if (orderLine.DeliverToId != orderLineModel.DeliverToId)
                    {
                        orderLine.DeliverToId = orderLineModel.DeliverToId;
                        orderLineTickets.ForEach(t => t.DeliverToId = orderLineModel.DeliverToId);
                    }
                    if (orderLine.ServiceId != orderLineModel.ServiceId)
                    {
                        orderLine.ServiceId = orderLineModel.ServiceId;
                        orderLineTickets.ForEach(t => t.ServiceId = orderLineModel.ServiceId);
                    }

                    orderLine.JobNumber = orderLineModel.JobNumber;

                    var rateWasChanged = false;
                    if (orderLine.Designation != orderLineModel.Designation)
                    {
                        var oldDesignation = orderLine.Designation;
                        orderLine.Designation = orderLineModel.Designation;
                        if (orderLine.Designation.MaterialOnly())
                        {
                            orderLine.FreightUomId = null;
                            if (orderLine.FreightPrice != 0 || orderLine.FreightPricePerUnit != 0 || orderLine.FreightQuantity != 0)
                            {
                                rateWasChanged = true;
                            }
                            orderLine.FreightPrice = 0;
                            //orderLine.FreightPricePerUnit = 0; //Rate change will be handled below
                            orderLine.FreightQuantity = 0;
                        }
                        else if (orderLine.Designation.FreightOnly())
                        {
                            orderLine.MaterialUomId = null;
                            if (orderLine.MaterialPrice != 0 || orderLine.MaterialPricePerUnit != 0 || orderLine.MaterialQuantity != 0)
                            {
                                rateWasChanged = true;
                            }
                            orderLine.MaterialPrice = 0;
                            //orderLine.MaterialPricePerUnit = 0; //Rate change will be handled below
                            orderLine.MaterialQuantity = 0;
                        }
                        else
                        {
                            if (oldDesignation.MaterialOnly())
                            {
                                rateWasChanged = true;
                                orderLine.FreightQuantity = orderLine.MaterialQuantity;
                                //orderLine.FreightUomId = orderLine.MaterialUomId; //UOM change will be handled below
                            }
                            else if (oldDesignation.FreightOnly())
                            {
                                rateWasChanged = true;
                                orderLine.MaterialQuantity = orderLine.FreightQuantity;
                                //orderLine.MaterialUomId = orderLine.FreightUomId; //UOM change will be handled below
                            }
                        }
                    }

                    if (orderLine.Designation.FreightAndMaterial())
                    {
                        if (orderLine.MaterialUomId != orderLineModel.UomId || orderLine.FreightUomId != orderLineModel.UomId)
                        {
                            await UncheckProductionPayOnFreightUomChangeIfNeeded(orderLine, orderLineModel.UomId);
                            orderLine.MaterialUomId = orderLineModel.UomId;
                            orderLine.FreightUomId = orderLineModel.UomId;
                            orderLineTickets.ForEach(t => t.UnitOfMeasureId = orderLineModel.UomId);
                        }
                    }
                    else if (orderLine.Designation.HasMaterial())
                    {
                        if (orderLine.MaterialUomId != orderLineModel.UomId)
                        {
                            orderLine.MaterialUomId = orderLineModel.UomId;
                            orderLineTickets.ForEach(t => t.UnitOfMeasureId = orderLineModel.UomId);
                        }
                    }
                    else
                    {
                        if (orderLine.FreightUomId != orderLineModel.UomId)
                        {
                            await UncheckProductionPayOnFreightUomChangeIfNeeded(orderLine, orderLineModel.UomId);
                            orderLine.FreightUomId = orderLineModel.UomId;
                            orderLineTickets.ForEach(t => t.UnitOfMeasureId = orderLineModel.UomId);
                        }
                    }

                    ServicePricingDto pricing = null;
                    if (orderLine.FreightPricePerUnit != orderLineModel.FreightRate || orderLine.MaterialPricePerUnit != orderLineModel.MaterialRate)
                    {
                        rateWasChanged = true;
                        pricing = await _serviceAppService.GetServicePricing(new GetServicePricingInput
                        {
                            ServiceId = orderLine.ServiceId,
                            MaterialUomId = orderLine.MaterialUomId,
                            FreightUomId = orderLine.FreightUomId,
                            QuoteServiceId = orderLine.QuoteServiceId
                        });
                        if (!(orderLine.MaterialPricePerUnit > 0) && orderLineModel.MaterialRate > 0
                            && orderLine.Designation.FreightOnly())
                        {
                            orderLine.Designation = orderLineModel.Designation = DesignationEnum.FreightAndMaterial;
                            orderLine.MaterialUomId = orderLineModel.MaterialUomId = orderLine.FreightUomId;
                            orderLine.MaterialQuantity = orderLine.FreightQuantity;
                        }
                    }
                    if (orderLine.FreightPricePerUnit != orderLineModel.FreightRate)
                    {
                        orderLine.FreightPricePerUnit = orderLineModel.FreightRate;
                        if (pricing?.QuoteBasedPricing?.FreightRate != null)
                        {
                            orderLine.IsFreightPricePerUnitOverridden = pricing.QuoteBasedPricing.FreightRate != orderLine.FreightPricePerUnit;
                        }
                        else if (pricing?.FreightRate != null && pricing.HasPricing)
                        {
                            orderLine.IsFreightPricePerUnitOverridden = pricing.FreightRate != orderLine.FreightPricePerUnit;
                        }
                        if (!orderLine.IsFreightPriceOverridden)
                        {
                            orderLine.FreightPrice = Math.Round((orderLine.FreightPricePerUnit ?? 0) * (orderLine.FreightQuantity ?? 0), 2);
                        }
                    }
                    if (orderLine.MaterialPricePerUnit != orderLineModel.MaterialRate)
                    {
                        orderLine.MaterialPricePerUnit = orderLineModel.MaterialRate;
                        if (pricing?.QuoteBasedPricing?.PricePerUnit != null)
                        {
                            orderLine.IsMaterialPricePerUnitOverridden = pricing.QuoteBasedPricing.PricePerUnit != orderLine.MaterialPricePerUnit;
                        }
                        else if (pricing?.PricePerUnit != null && pricing.HasPricing)
                        {
                            orderLine.IsMaterialPricePerUnitOverridden = pricing.PricePerUnit != orderLine.MaterialPricePerUnit;
                        }
                        if (!orderLine.IsMaterialPriceOverridden)
                        {
                            orderLine.MaterialPrice = Math.Round((orderLine.MaterialPricePerUnit ?? 0) * (orderLine.MaterialQuantity ?? 0), 2);
                        }
                    }
                    orderLine.FreightRateToPayDrivers = orderLineModel.FreightRateToPayDrivers;

                    if (rateWasChanged)
                    {
                        await CurrentUnitOfWork.SaveChangesAsync();
                        await _orderTaxCalculator.CalculateTotalsAsync(orderLine.OrderId);
                        await _fuelSurchargeCalculator.RecalculateOrderLinesWithTickets(orderLine.Id);
                        orderLineModel.FuelSurchargeRate = orderLine.FuelSurchargeRate;
                    }

                    changedTickets.AddRange(orderLineTickets);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            if (changedTickets.Any())
            {
                var ticketIds = changedTickets.Select(x => x.Id).Distinct().ToList();
                var tickets = await _ticketRepository.GetAll()
                    .Where(x => ticketIds.Contains(x.Id))
                    .Select(t => new TicketsByDriverResult.TicketDto
                    {
                        Id = t.Id,
                        OrderLineId = t.OrderLineId,
                        TicketNumber = t.TicketNumber,
                        TicketDateTime = t.TicketDateTime,
                        Quantity = t.Quantity,
                        UomId = t.UnitOfMeasureId,
                        UomName = t.UnitOfMeasure.Name,
                        TruckId = t.TruckId,
                        TruckCode = t.TruckCode,
                        DriverId = t.DriverId,
                        TicketPhotoId = t.TicketPhotoId,
                        ReceiptLineId = t.ReceiptLineId,
                        IsVerified = t.IsVerified,
                        IsReadOnly = t.InvoiceLine != null //already invoiced
                                    || t.PayStatementTickets.Any() //already added to pay statements
                                    || t.ReceiptLineId != null //already added to receipts
                                    || t.LeaseHaulerStatementTicket != null //added to lease hauler statements
                    })
                    .OrderBy(x => x.Id)
                    .ToListAsync();

                foreach (var ticket in tickets)
                {
                    ticket.TicketDateTime = ticket.TicketDateTime?.ConvertTimeZoneTo(timezone);
                }

                if (model.Tickets?.Any() != true)
                {
                    model.Tickets = tickets;
                }
                else
                {
                    foreach (var ticket in tickets)
                    {
                        model.Tickets.RemoveAll(x => x.Id == ticket.Id);
                        model.Tickets.Add(ticket);
                    }
                }
            }

            return model;
        }

        private async Task UncheckProductionPayOnFreightUomChangeIfNeeded(OrderLine orderLine, int? newUomId)
        {
            if (orderLine.FreightUomId == newUomId
                || !orderLine.ProductionPay
                || !newUomId.HasValue
                || !await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.PreventProductionPayOnHourlyJobs))
            {
                return;
            }

            var uom = await _uomRepository.GetAll()
                .Where(x => x.Id == newUomId)
                .Select(x => new
                {
                    x.Name
                }).FirstAsync();

            if (uom.Name.ToLower().TrimEnd('s') == "hour")
            {
                orderLine.ProductionPay = false;
            }
        }

        public async Task SetIsVerifiedForTickets(List<TicketIsVerifiedDto> models)
        {
            if (models?.Any() == true)
            {
                var ticketIds = models.Select(x => x.Id).ToList();
                var tickets = await _ticketRepository.GetAll().Where(x => ticketIds.Contains(x.Id)).ToListAsync();
                foreach (var model in models)
                {
                    var ticket = tickets.FirstOrDefault(x => x.Id == model.Id);
                    if (ticket != null)
                    {
                        ticket.IsVerified = model.IsVerified;
                    }
                }
            }
        }

        public async Task<TicketsByDriverResult.DailyFuelCostDto> EditCurrentFuelCost(EditCurrentFuelCostInput input)
        {
            var dailyFuelCost = await _dailyFuelCostRepository.GetAll()
                .Where(x => x.Date == input.Date.AddDays(-1))
                .FirstOrDefaultAsync();

            dailyFuelCost ??= new DailyFuelCost
            {
                Date = input.Date.AddDays(-1)
            };

            var costChanged = dailyFuelCost.Id == 0 || dailyFuelCost.Cost != input.Cost;
            dailyFuelCost.Cost = input.Cost;

            if (dailyFuelCost.Id == 0)
            {
                await _dailyFuelCostRepository.InsertAsync(dailyFuelCost);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            if (costChanged)
            {
                await _fuelSurchargeCalculator.RecalculateOrderLinesWithTickets(input.Date);
            }

            return new TicketsByDriverResult.DailyFuelCostDto
            {
                Date = dailyFuelCost.Date,
                Cost = dailyFuelCost.Cost
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_View)]
        public async Task<Document> GetTicketPrintOut(GetTicketPrintOutInput input)
        {
            var data = await GetTicketPrintOutData(input);

            Document report;

            report = await _ticketPrintOutGenerator.GenerateReport(data);
            return report;
        }

        private async Task<List<TicketPrintOutDto>> GetTicketPrintOutData(GetTicketPrintOutInput input)
        {
            var item = await _ticketRepository.GetAll()
                .Where(x => input.TicketId == x.Id)
                .Select(x => new TicketPrintOutDto
                {
                    TicketNumber = x.TicketNumber,
                    TicketDateTime = x.TicketDateTime,
                    CustomerName = x.Customer.Name,
                    ServiceName = x.Service.Service1,
                    MaterialQuantity = x.OrderLine.MaterialQuantity,
                    MaterialUomName = x.OrderLine.MaterialUom.Name,
                    Note = x.OrderLine.Note

                }).FirstOrDefaultAsync();

            item.LegalName = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingLegalName);
            item.LegalAddress = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingAddress);
            item.BillingPhoneNumber = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingPhoneNumber);
            item.LogoPath = await _binaryObjectManager.GetLogoAsBase64StringAsync(await GetCurrentTenantAsync());
            item.TicketDateTime = item.TicketDateTime?.ConvertTimeZoneTo(await GetTimezone());
            item.DebugLayout = input.DebugLayout;

            return new List<TicketPrintOutDto> { item };
        }

        private async Task ThrowIfDriverHasTimeOffRequests(int? driverId, DateTime? date)
        {
            if (driverId == null || date == null)
            {
                return;
            }
            if (await _timeOffRepository.GetAll()
                    .AnyAsync(x => x.DriverId == driverId && date <= x.EndDate && date >= x.StartDate))
            {
                throw new UserFriendlyException(L("DriverCantBeAssignedOnDayOff"));
            }
        }

        private async Task EnsureCanAddTickets(int orderLineId, int officeId)
        {
            var orderLine = await _orderLineRepository.GetAll()
                .Where(x => x.Id == orderLineId)
                .Select(x => new
                {
                    x.IsMaterialPriceOverridden,
                    x.IsFreightPriceOverridden,
                    HasTickets = x.Tickets.Any(),
                    HasTicketsForOtherOffices = x.Tickets.Any(a => a.OfficeId != officeId)
                }).FirstAsync();

            if (orderLine.IsMaterialPriceOverridden || orderLine.IsFreightPriceOverridden)
            {
                if (orderLine.HasTicketsForOtherOffices)
                {
                    throw new UserFriendlyException("You can't add tickets to a line item with overridden totals for which another office already added tickets");
                }

                if (orderLine.HasTickets)
                {
                    throw new UserFriendlyException(L("OrderLineWithOverriddenTotalCanOnlyHaveSingleTicketError"));
                }
            }
        }
    }
}
