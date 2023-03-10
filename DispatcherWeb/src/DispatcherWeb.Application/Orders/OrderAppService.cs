using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Timing;
using Abp.UI;
using Abp.Web.Models;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Emailing;
using DispatcherWeb.Encryption;
using DispatcherWeb.Exceptions;
using DispatcherWeb.Features;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Templates;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Orders.Reports;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Payments;
using DispatcherWeb.Payments.Dto;
using DispatcherWeb.Quotes;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.Services;
using DispatcherWeb.Storage;
using DispatcherWeb.SyncRequests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using MigraDoc.DocumentObjectModel;

namespace DispatcherWeb.Orders
{
    [AbpAuthorize]
    public class OrderAppService : DispatcherWebAppServiceBase, IOrderAppService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<SharedOrder> _sharedOrderRepository;
        private readonly IRepository<SharedOrderLine> _sharedOrderLineRepository;
        private readonly IRepository<BilledOrder> _billedOrderRepository;
        private readonly IRepository<QuoteService> _quoteServiceRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<OrderEmail> _orderEmailRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<OrderPayment> _orderPaymentRepository;
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<FuelSurchargeCalculation> _fuelSurchargeCalculationRepository;
        private readonly IOrderLineUpdaterFactory _orderLineUpdaterFactory;
        private readonly IDriverApplicationPushSender _driverApplicationPushSender;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IReceiptsExcelExporter _receiptsExcelExporter;
        private readonly ITrackableEmailSender _trackableEmailSender;
        private readonly IAppNotifier _appNotifier;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly IEncryptionService _encryptionService;
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IOrderLineScheduledTrucksUpdater _orderLineScheduledTrucksUpdater;
        private readonly IPaymentAppService _paymentAppService;
        private readonly IOfficeSettingsManager _officeSettingsManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IFuelSurchargeCalculator _fuelSurchargeCalculator;

        public OrderAppService(
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<SharedOrder> sharedOrderRepository,
            IRepository<SharedOrderLine> sharedOrderLineRepository,
            IRepository<BilledOrder> billedOrderRepository,
            IRepository<QuoteService> quoteServiceRepository,
            IRepository<Office> officeRepository,
            IRepository<User, long> userRepository,
            IRepository<OrderEmail> orderEmailRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Payment> paymentRepository,
            IRepository<OrderPayment> orderPaymentRepository,
            IRepository<Receipt> receiptRepository,
            IRepository<Service> serviceRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<FuelSurchargeCalculation> fuelSurchargeCalculationRepository,
            IOrderLineUpdaterFactory orderLineUpdaterFactory,
            IDriverApplicationPushSender driverApplicationPushSender,
            ISyncRequestSender syncRequestSender,
            IReceiptsExcelExporter receiptsExcelExporter,
            ITrackableEmailSender trackableEmailSender,
            IAppNotifier appNotifier,
            IWebHostEnvironment hostingEnvironment,
            ISingleOfficeAppService singleOfficeService,
            IEncryptionService encryptionService,
            OrderTaxCalculator orderTaxCalculator,
            IOrderLineScheduledTrucksUpdater orderLineScheduledTrucksUpdater,
            IPaymentAppService paymentAppService,
            IOfficeSettingsManager officeSettingsManager,
            IBinaryObjectManager binaryObjectManager,
            IFuelSurchargeCalculator fuelSurchargeCalculator
            )
        {
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _sharedOrderRepository = sharedOrderRepository;
            _sharedOrderLineRepository = sharedOrderLineRepository;
            _billedOrderRepository = billedOrderRepository;
            _quoteServiceRepository = quoteServiceRepository;
            _officeRepository = officeRepository;
            _userRepository = userRepository;
            _orderEmailRepository = orderEmailRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _fuelSurchargeCalculationRepository = fuelSurchargeCalculationRepository;
            _paymentRepository = paymentRepository;
            _orderPaymentRepository = orderPaymentRepository;
            _receiptRepository = receiptRepository;
            _serviceRepository = serviceRepository;
            _dispatchRepository = dispatchRepository;
            _orderLineUpdaterFactory = orderLineUpdaterFactory;
            _driverApplicationPushSender = driverApplicationPushSender;
            _syncRequestSender = syncRequestSender;
            _receiptsExcelExporter = receiptsExcelExporter;
            _trackableEmailSender = trackableEmailSender;
            _appNotifier = appNotifier;
            _hostingEnvironment = hostingEnvironment;
            _singleOfficeService = singleOfficeService;
            _encryptionService = encryptionService;
            _orderTaxCalculator = orderTaxCalculator;
            _orderLineScheduledTrucksUpdater = orderLineScheduledTrucksUpdater;
            _paymentAppService = paymentAppService;
            _officeSettingsManager = officeSettingsManager;
            _binaryObjectManager = binaryObjectManager;
            _fuelSurchargeCalculator = fuelSurchargeCalculator;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<PagedResultDto<OrderDto>> GetOrders(GetOrdersInput input)
        {
            var query = _orderRepository.GetAll()
                .WhereIf(input.StartDate.HasValue,
                         x => x.DeliveryDate >= input.StartDate)
                .WhereIf(input.EndDate.HasValue,
                         x => x.DeliveryDate <= input.EndDate)
                .WhereIf(input.OfficeId.HasValue,
                         x => x.LocationId == input.OfficeId /*|| x.SharedOrders.Any(s => s.OfficeId == input.OfficeId)*/)
                .WhereIf(input.CustomerId.HasValue,
                         x => x.CustomerId == input.CustomerId)
                .WhereIf(input.ServiceId.HasValue,
                         x => x.OrderLines.Any(ol => ol.ServiceId == input.ServiceId))
                .WhereIf(!string.IsNullOrEmpty(input.JobNumber),
                        x => x.OrderLines.Any(ol => ol.JobNumber == input.JobNumber))
                .WhereIf(!string.IsNullOrEmpty(input.Misc),
                         x => x.Quote.Name.Contains(input.Misc)
                         || x.ChargeTo.Contains(input.Misc))
                .WhereIf(input.LoadAtId.HasValue,
                         x => x.OrderLines.Any(ol => ol.LoadAtId == input.LoadAtId))
                .WhereIf(input.DeliverToId.HasValue,
                         x => x.OrderLines.Any(ol => ol.DeliverToId == input.DeliverToId))
                 .Where(x => x.IsPending == input.ShowPendingOrders);

            var totalCount = await query.CountAsync();
            var items = await query
                .Select(x => new OrderDto
                {
                    Id = x.Id,
                    DeliveryDate = x.DeliveryDate,
                    OfficeId = x.LocationId,
                    OfficeName = x.Office.Name,
                    CustomerName = x.Customer.Name,
                    QuoteName = x.Quote.Name,
                    PONumber = x.PONumber,
                    ContactName = x.CustomerContact.Name,
                    ChargeTo = x.ChargeTo,
                    CODTotal = x.CODTotal,
                    NumberOfTrucks = x.OrderLines.Sum(ol => ol.NumberOfTrucks),
                    IsShared = false /*x.SharedOrders.Any(o => o.OfficeId != x.LocationId)*/,
                    EmailDeliveryStatuses = x.OrderEmails.Select(y => y.Email.CalculatedDeliveryStatus).ToList(),
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<OrderDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<ListResultDto<SelectListDto>> GetOrderIdsSelectList(GetSelectListInput input)
        {
            var ordersQuery = _orderRepository.GetAll()
                                    .Select(x => new SelectListDto
                                    {
                                        Id = x.Id.ToString(),
                                        Name = x.Id.ToString()
                                    });

            return await ordersQuery.GetSelectListResult(input);
        }

        [DontWrapResult]
        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<OrderEditDto> GetOrderForEdit(NullableIdDto input)
        {
            OrderEditDto orderEditDto;

            if (input.Id.HasValue)
            {
                orderEditDto = await _orderRepository.GetAll()
                    .Select(order => new OrderEditDto
                    {
                        Id = order.Id,
                        CreationTime = order.CreationTime,
                        CreatorName = order.CreatorUser.Name + " " + order.CreatorUser.Surname,
                        LastModificationTime = order.LastModificationTime,
                        LastModifierName = order.LastModifierUser.Name + " " + order.LastModifierUser.Surname,
                        MaterialCompanyOrderId = order.MaterialCompanyOrderId,
                        CODTotal = order.CODTotal,
                        ContactId = order.ContactId,
                        ContactName = order.CustomerContact.Name,
                        ContactPhone = order.CustomerContact.PhoneNumber,
                        ChargeTo = order.ChargeTo,
                        CustomerId = order.CustomerId,
                        CustomerName = order.Customer.Name,
                        CustomerAccountNumber = order.Customer.AccountNumber,
                        CustomerIsCod = order.Customer.IsCod,
                        DeliveryDate = order.DeliveryDate,
                        Shift = order.Shift,
                        IsPending = order.IsPending,
                        Directions = order.Directions,
                        FreightTotal = order.FreightTotal,
                        IsClosed = order.IsClosed,
                        IsShared = order.SharedOrders.Any(o => o.OfficeId != order.LocationId),
                        //IsFreightTotalOverridden = order.IsFreightTotalOverridden,
                        //IsMaterialTotalOverridden = order.IsMaterialTotalOverridden,
                        LocationId = order.LocationId,
                        MaterialTotal = order.MaterialTotal,
                        OfficeName = order.Office.Name,
                        PONumber = order.PONumber,
                        SpectrumNumber = order.SpectrumNumber,
                        ProjectId = order.ProjectId,
                        QuoteId = order.QuoteId,
                        QuoteName = order.Quote.Name,
                        SalesTax = order.SalesTax,
                        SalesTaxRate = order.SalesTaxRate,
                        SharedDateTime = order.SharedDateTime,
                        Priority = order.Priority,
                        BaseFuelCost = order.BaseFuelCost,
                        FuelSurchargeCalculationId = order.FuelSurchargeCalculationId,
                        FuelSurchargeCalculationName = order.FuelSurchargeCalculation.Name,
                        CanChangeBaseFuelCost = order.FuelSurchargeCalculation.CanChangeBaseFuelCost,
                        HasSharedOrderLines = order.OrderLines.Any(ol => ol.SharedOrderLines.Any(s => s.OfficeId == OfficeId)),
                        Receipts = order.Receipts
                            .Where(x => x.OfficeId == OfficeId)
                            .Select(r => new ReceiptDto
                            {
                                Id = r.Id,
                                ReceiptDate = r.ReceiptDate,
                                Total = r.Total
                            }).ToList()
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.Id.Value);

                if (orderEditDto == null)
                {
                    throw await GetOrderNotFoundException(new EntityDto(input.Id.Value));
                }

                var payment = await _orderRepository.GetAll()
                    .Where(x => x.Id == input.Id)
                    .SelectMany(x => x.OrderPayments)
                    .Where(x => x.OfficeId == OfficeId)
                    .Select(x => x.Payment)
                    .Where(x => !x.IsCancelledOrRefunded)
                    .Select(x => new
                    {
                        x.AuthorizationDateTime,
                        x.AuthorizationCaptureDateTime
                    }).FirstOrDefaultAsync();

                orderEditDto.AuthorizationDateTime = payment?.AuthorizationDateTime;
                orderEditDto.AuthorizationCaptureDateTime = payment?.AuthorizationCaptureDateTime;

                var timeZone = await GetTimezone();
                orderEditDto.CreationTime = orderEditDto.CreationTime?.ConvertTimeZoneTo(timeZone);
                orderEditDto.LastModificationTime = orderEditDto.LastModificationTime?.ConvertTimeZoneTo(timeZone);
                orderEditDto.SharedDateTime = orderEditDto.SharedDateTime?.ConvertTimeZoneTo(timeZone);
            }
            else
            {
                orderEditDto = new OrderEditDto
                {
                    LocationId = OfficeId,
                    OfficeName = Session.OfficeName,
                    Priority = OrderPriority.Medium
                };
                if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.Invoice.AutopopulateDefaultTaxRate)
                    && (TaxCalculationType)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType) != TaxCalculationType.NoCalculation)
                {
                    orderEditDto.SalesTaxRate = await SettingManager.GetSettingValueAsync<decimal>(AppSettings.Invoice.DefaultTaxRate);
                }
            }
            orderEditDto.CanEditAnyOrderDirections = await CanEditAnyOrderDirectionsAsync();
            await _singleOfficeService.FillSingleOffice(orderEditDto);

            var defaultFuelSurchargeCalculationId = await SettingManager.GetDefaultFuelSurchargeCalculationId();
            if (defaultFuelSurchargeCalculationId > 0)
            {
                var defaultFuelSurchargeCalculation = await _fuelSurchargeCalculationRepository.GetAll()
                    .Where(x => x.Id == defaultFuelSurchargeCalculationId)
                    .Select(x => new
                    {
                        x.Name,
                        x.CanChangeBaseFuelCost,
                        x.BaseFuelCost
                    })
                    .FirstOrDefaultAsync();

                orderEditDto.DefaultFuelSurchargeCalculationName = defaultFuelSurchargeCalculation.Name;
                orderEditDto.DefaultBaseFuelCost = defaultFuelSurchargeCalculation.BaseFuelCost;
                orderEditDto.DefaultCanChangeBaseFuelCost = defaultFuelSurchargeCalculation.CanChangeBaseFuelCost;

                if (!input.Id.HasValue)
                {
                    orderEditDto.FuelSurchargeCalculationId = defaultFuelSurchargeCalculationId;
                    orderEditDto.FuelSurchargeCalculationName = defaultFuelSurchargeCalculation.Name;
                    orderEditDto.CanChangeBaseFuelCost = defaultFuelSurchargeCalculation.CanChangeBaseFuelCost;
                    orderEditDto.BaseFuelCost = defaultFuelSurchargeCalculation.BaseFuelCost;
                }
            }
            else
            {
                orderEditDto.DefaultFuelSurchargeCalculationName = AppConsts.FuelSurchargeCalculationBlankName;
            }

            return orderEditDto;
        }

        [UnitOfWork(IsDisabled = true)]
        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<EditOrderResult> EditJob(JobEditDto model)
        {
            OrderEditDto editOrderModal;
            using (var unitOfWork = UnitOfWorkManager.Begin(new UnitOfWorkOptions { IsTransactional = true }))
            {
                editOrderModal = await GetOrderForEdit(new NullableIdDto(model.OrderId));
                await unitOfWork.CompleteAsync();
            }
            editOrderModal.DeliveryDate = model.DeliveryDate;
            editOrderModal.CustomerId = model.CustomerId;
            editOrderModal.QuoteId = model.QuoteId;
            editOrderModal.ChargeTo = model.ChargeTo;
            editOrderModal.Priority = model.Priority;
            editOrderModal.Shift = model.Shift;
            editOrderModal.OfficeId = model.OfficeId;
            editOrderModal.ProjectId = model.ProjectId;
            editOrderModal.ContactId = model.ContactId;
            editOrderModal.MaterialCompanyOrderId = model.MaterialCompanyOrderId;
            editOrderModal.PONumber = model.PONumber;
            editOrderModal.SpectrumNumber = model.SpectrumNumber;
            editOrderModal.Directions = model.Directions;

            var editOrderResult = await EditOrder(editOrderModal);
            if (!editOrderResult.Completed)
            {
                return editOrderResult;
            }

            using (var unitOfWork = UnitOfWorkManager.Begin(new UnitOfWorkOptions { IsTransactional = true }))
            {
                var editOrderLineModal = await GetOrderLineForEdit(new GetOrderLineForEditInput(model.OrderLineId, editOrderResult.Id));
                editOrderLineModal.JobNumber = model.JobNumber;
                editOrderLineModal.Designation = model.Designation;
                editOrderLineModal.LoadAtId = model.LoadAtId;
                editOrderLineModal.DeliverToId = model.DeliverToId;
                editOrderLineModal.ServiceId = model.ServiceId;
                editOrderLineModal.FreightUomId = model.FreightUomId;
                editOrderLineModal.MaterialUomId = model.MaterialUomId;
                editOrderLineModal.FreightPricePerUnit = model.FreightPricePerUnit;
                editOrderLineModal.MaterialPricePerUnit = model.MaterialPricePerUnit;
                editOrderLineModal.FreightQuantity = model.FreightQuantity;
                editOrderLineModal.MaterialQuantity = model.MaterialQuantity;
                editOrderLineModal.FreightPrice = model.FreightPrice;
                editOrderLineModal.MaterialPrice = model.MaterialPrice;
                editOrderLineModal.LeaseHaulerRate = model.LeaseHaulerRate;
                editOrderLineModal.NumberOfTrucks = model.NumberOfTrucks;
                editOrderLineModal.IsMultipleLoads = model.IsMultipleLoads;
                editOrderLineModal.TimeOnJob = model.TimeOnJob;
                editOrderLineModal.ProductionPay = model.ProductionPay;
                editOrderLineModal.Note = model.Note;
                editOrderLineModal.QuoteServiceId = model.QuoteServiceId;

                var orderLine = await EditOrderLine(editOrderLineModal);

                await unitOfWork.CompleteAsync();

                return new EditJobResult
                {
                    Id = editOrderResult.Id,
                    Completed = editOrderResult.Completed,
                    HasZeroQuantityItems = editOrderResult.HasZeroQuantityItems,
                    NotAvailableTrucks = editOrderResult.NotAvailableTrucks,
                    OrderLineId = orderLine.OrderLineId
                };
            }
        }

        [UnitOfWork(IsDisabled = true)]
        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<EditOrderResult> EditOrder(OrderEditDto model)
        {
            using (var unitOfWork = UnitOfWorkManager.Begin(new UnitOfWorkOptions { IsTransactional = true }))
            {
                var result = await EditOrderInternal(model);

                if (!result.Completed)
                {
                    return result;
                }

                if (model.Id > 0)
                {
                    var failedEditResult = await SyncLinkedOrders(model.Id.Value);
                    if (failedEditResult != null && !failedEditResult.Completed)
                    {
                        return failedEditResult;
                    }
                }

                await unitOfWork.CompleteAsync();
                return result;
            }
        }

        private async Task<EditOrderResult> EditOrderInternal(OrderEditDto model)
        {
            var order = model.Id.HasValue ? await _orderRepository.GetAsync(model.Id.Value) : new Order();

            if (!model.IsPending && model.DeliveryDate == null)
            {
                throw new UserFriendlyException("Order Delivery Date is a required field");
            }

            await ThrowUserFriendlyExceptionIfStatusIsChangedToPendingAndThereArePrerequisites(order, model);
            // #7551 prevent transferring to another office when there are trucks scheduled against it
            //await ClearTrucksIfOfficeIsChanged(order, model);
            await ThrowUserFriendlyExceptionIfOfficeIsChangedAndThereArePrerequisites(order, model);

            var dispatchRelatedFieldWasChanged = false;

            if (model.Id.HasValue)
            {
                if (order.DeliveryDate != model.DeliveryDate
                    || order.Shift != model.Shift
                    || order.LocationId != model.LocationId
                    || order.QuoteId != model.QuoteId)
                {
                    await ThrowIfOrderLinesHaveTickets(model.Id.Value);
                }

                if (order.LocationId != model.LocationId)
                {
                    await ThrowIfOrderHasReceiptsForOwnerOfficeId(model.Id.Value);
                }

                if (order.CustomerId != model.CustomerId)
                {
                    if (await HasLoadedDispatchesForOrder(model.Id.Value) || await HasManualTicketsForOrder(model.Id.Value))
                    {
                        throw new UserFriendlyException(L("Order_Edit_Error_HasDispatches"));
                    }
                    dispatchRelatedFieldWasChanged = true;
                }
            }

            var deliveryDateWasChanged = model.DeliveryDate != order.DeliveryDate;

            if (!model.DeliveryDate.HasValue || !model.Id.HasValue)
            {
                order.DeliveryDate = model.DeliveryDate;
                order.Shift = model.Shift;
            }
            else if (order.DeliveryDate != model.DeliveryDate || order.Shift != model.Shift)
            {
                var setOrderDateResult = await SetOrderDateInternal(new SetOrderDateInput
                {
                    Date = model.DeliveryDate.Value,
                    Shift = model.Shift,
                    KeepTrucks = true,
                    OrderId = model.Id.Value,
                    OrderLineId = null,
                    RemoveNotAvailableTrucks = model.RemoveNotAvailableTrucks,
                });
                if (!setOrderDateResult.Completed)
                {
                    return new EditOrderResult
                    {
                        Completed = false,
                        NotAvailableTrucks = setOrderDateResult.NotAvailableTrucks,
                    };
                }
            }

            var needToRecalculateFuelSurcharge = order.FuelSurchargeCalculationId != model.FuelSurchargeCalculationId || order.BaseFuelCost != model.BaseFuelCost;

            order.CODTotal = model.CODTotal;
            order.ContactId = model.ContactId;
            order.ChargeTo = model.ChargeTo;
            order.CustomerId = model.CustomerId;
            order.IsPending = model.IsPending;
            order.Directions = model.Directions;
            order.FreightTotal = model.FreightTotal;
            order.IsClosed = model.IsClosed;
            //order.IsFreightTotalOverridden = model.IsFreightTotalOverridden;
            //order.IsMaterialTotalOverridden = model.IsMaterialTotalOverridden;
            order.LocationId = model.LocationId;
            order.MaterialTotal = model.MaterialTotal;
            order.PONumber = model.PONumber;
            order.SpectrumNumber = model.SpectrumNumber;
            order.ProjectId = model.ProjectId;
            order.QuoteId = model.QuoteId;
            order.SalesTax = model.SalesTax;
            order.SalesTaxRate = model.SalesTaxRate;
            order.FuelSurchargeCalculationId = model.FuelSurchargeCalculationId;
            order.BaseFuelCost = model.BaseFuelCost;
            order.Priority = model.Priority;

            if (!model.Id.HasValue && model.OrderLines != null)
            {
                model.Id = await _orderRepository.InsertAndGetIdAsync(order);

                foreach (var orderLineModel in model.OrderLines)
                {
                    orderLineModel.OrderId = order.Id;
                    orderLineModel.UpdateStaggeredTime = true;
                    if (model.DeliveryDate.HasValue)
                    {
                        if (orderLineModel.FirstStaggeredTimeOnJob.HasValue)
                        {
                            orderLineModel.FirstStaggeredTimeOnJob = model.DeliveryDate.Value.Date.Add(orderLineModel.FirstStaggeredTimeOnJob.Value.TimeOfDay);
                        }
                        if (orderLineModel.TimeOnJob.HasValue)
                        {
                            orderLineModel.TimeOnJob = model.DeliveryDate.Value.Date.Add(orderLineModel.TimeOnJob.Value.TimeOfDay);
                        }
                    }
                    await EditOrderLineInternal(orderLineModel);
                }

                var serviceIds = model.OrderLines.Select(x => x.ServiceId).Distinct().ToList();
                var services = await _serviceRepository.GetAll()
                    .Where(x => serviceIds.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.IsTaxable
                    }).ToListAsync();

                await _orderTaxCalculator.CalculateTotalsAsync(order, model.OrderLines.Select(x => new OrderLineTaxDetailsDto
                {
                    IsTaxable = services.FirstOrDefault(s => s.Id == x.ServiceId)?.IsTaxable ?? true,
                    FreightPrice = x.FreightPrice,
                    MaterialPrice = x.MaterialPrice
                }));
            }
            else if (model.Id > 0)
            {
                var orderLines = await _orderLineRepository.GetAll()
                    .Where(x => x.OrderId == model.Id)
                    .Select(x => new OrderLineTaxDetailsDto
                    {
                        FreightPrice = x.FreightPrice,
                        MaterialPrice = x.MaterialPrice,
                        IsTaxable = x.Service.IsTaxable
                    }).ToListAsync();

                await _orderTaxCalculator.CalculateTotalsAsync(order, orderLines);
            }

            if (model.Id > 0 && deliveryDateWasChanged)
            {
                await UpdateOrderLineDatesIfNeeded(model.DeliveryDate, model.Id);
            }

            var result = new EditOrderResult
            {
                Completed = true,
                Id = model.Id ?? await _orderRepository.InsertAndGetIdAsync(order),
            };
            model.Id = result.Id;
            await CurrentUnitOfWork.SaveChangesAsync();

            if (needToRecalculateFuelSurcharge)
            {
                await _fuelSurchargeCalculator.RecalculateOrderLinesWithTicketsForOrder(result.Id);
            }

            result.HasZeroQuantityItems = await _orderLineRepository.GetAll()
                .AnyAsync(x => x.OrderId == model.Id
                    && (x.MaterialQuantity == null || x.MaterialQuantity == 0)
                    && (x.FreightQuantity == null || x.FreightQuantity == 0)
                    && (x.NumberOfTrucks == null || x.NumberOfTrucks == 0));

            if (dispatchRelatedFieldWasChanged)
            {
                await UpdateAssociatedDispatchesOnOrderChange(model.Id.Value);
            }

            return result;
        }

        /// <summary>
        /// Syncs destination orders linked to the provided source order
        /// </summary>
        /// <returns>Returns null in case of success, or EditOrderResult of a failed order in case of a failure</returns>
        private async Task<EditOrderResult> SyncLinkedOrders(int sourceOrderId)
        {
            await CurrentUnitOfWork.SaveChangesAsync();
            var sourceOrder = await _orderRepository.GetAll()
                .Where(x => x.Id == sourceOrderId)
                .Select(x => new
                {
                    x.HasLinkedHaulingCompanyOrders,
                    x.DeliveryDate,
                    x.Directions,
                    x.IsClosed,
                    x.IsPending,
                    x.PONumber,
                    x.Priority,
                    x.SpectrumNumber,
                })
                .FirstAsync();

            if (sourceOrder.HasLinkedHaulingCompanyOrders)
            {
                var destinationOrderIds = new List<MustHaveTenantDto>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant))
                {
                    destinationOrderIds = await _orderRepository.GetAll()
                        .Where(x => x.MaterialCompanyOrderId == sourceOrderId)
                        .Select(x => new MustHaveTenantDto
                        {
                            Id = x.Id,
                            TenantId = x.TenantId,
                        })
                        .ToListAsync();
                }

                foreach (var destinationOrderId in destinationOrderIds)
                {
                    using (CurrentUnitOfWork.SetTenantId(destinationOrderId.TenantId))
                    {
                        var destinationOrderEditDto = await GetOrderForEdit(new NullableIdDto(destinationOrderId.Id));
                        destinationOrderEditDto.DeliveryDate = sourceOrder.DeliveryDate;
                        destinationOrderEditDto.Directions = sourceOrder.Directions;
                        destinationOrderEditDto.IsClosed = sourceOrder.IsClosed;
                        destinationOrderEditDto.IsPending = sourceOrder.IsPending;
                        destinationOrderEditDto.PONumber = sourceOrder.PONumber;
                        destinationOrderEditDto.Priority = sourceOrder.Priority;
                        destinationOrderEditDto.SpectrumNumber = sourceOrder.SpectrumNumber;
                        var destinationEditResult = await EditOrderInternal(destinationOrderEditDto);
                        if (!destinationEditResult.Completed)
                        {
                            return destinationEditResult;
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }

            return null;
        }

        private async Task UpdateAssociatedDispatchesOnOrderChange(int orderId)
        {
            var dispatches = await _dispatchRepository.GetAll()
                                .Where(x => x.OrderLine.OrderId == orderId)
                                .ToListAsync();

            if (dispatches.Any())
            {
                dispatches.ForEach(d => d.LastModificationTime = Clock.Now);

                await CurrentUnitOfWork.SaveChangesAsync();

                foreach (var dispatchGroup in dispatches.GroupBy(x => x.DriverId))
                {
                    await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchGroup.Key)
                    {
                        LogMessage = $"Updated Order has affected dispatch(es) {string.Join(", ", dispatchGroup.Select(x => x.Id))}"
                    });
                }
                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch, dispatches.Select(x => x.ToChangedEntity()))
                    .AddLogMessage("Updated Order has affected dispatch(es)"));
            }
        }

        private async Task ThrowUserFriendlyExceptionIfStatusIsChangedToPendingAndThereArePrerequisites(Order order, OrderEditDto model)
        {
            if (model.Id.HasValue && model.IsPending && order.IsPending != model.IsPending)
            {
                if (await _orderLineRepository.GetAll()
                    .AnyAsync(ol => ol.OrderId == order.Id && (
                                    ol.IsComplete ||
                                    ol.Tickets.Any() ||
                                    ol.OrderLineTrucks.Any()
                                    )
                    ))
                {
                    throw new UserFriendlyException("Cannot change status to Pending.");
                }
            }
        }

        private async Task ThrowUserFriendlyExceptionIfOfficeIsChangedAndThereArePrerequisites(Order order, OrderEditDto model)
        {
            if (model.Id.HasValue && model.OfficeId != order.LocationId)
            {
                await CheckOrderPrerequisites(model.Id.Value);
            }
        }

        private async Task ThrowIfOrderLinesHaveTickets(int orderId)
        {
            var orderLineDetails = await _orderLineRepository.GetAll()
                .Where(x => x.OrderId == orderId)
                .Select(x => new
                {
                    HasTickets = x.Tickets.Any()
                })
                .ToListAsync();

            if (orderLineDetails.Any(x => x.HasTickets))
            {
                throw new UserFriendlyException(L("Order_Edit_Error_HasTickets"));
            }
        }

        private async Task ThrowIfOrderLineHasTicketsOrActualAmounts(int orderLineId)
        {
            var orderLineDetails = await _orderLineRepository.GetAll()
                .Where(x => x.Id == orderLineId)
                .Select(x => new
                {
                    HasTickets = x.Tickets.Any(),
                    HasOpenDispatches = x.Dispatches.Any(d => !Dispatch.ClosedDispatchStatuses.Contains(d.Status)),
                })
                .FirstAsync();

            if (orderLineDetails.HasTickets || orderLineDetails.HasOpenDispatches)
            {
                throw new UserFriendlyException(L("OrderLine_Edit_Error_HasTickets"));
            }
        }

        private async Task ThrowIfOrderHasReceiptsForOwnerOfficeId(int orderId)
        {
            if (await _orderRepository.GetAll()
                .Where(x => x.Id == orderId
                    && x.Receipts.Any(r => r.OfficeId == x.LocationId))
                .AnyAsync())
            {
                throw new UserFriendlyException(L("Order_ChangeOffice_Error_HasReceipts"));
            }
        }

        private async Task ThrowIfOrderLineHasReceiptsForOwnerOfficeId(int orderLineId)
        {
            if (await _orderLineRepository.GetAll()
                .Where(x => x.Id == orderLineId
                    && x.ReceiptLines.Any(r => r.Receipt.OfficeId == x.Order.LocationId))
                .AnyAsync())
            {
                throw new UserFriendlyException(L("Order_ChangeOffice_Error_HasReceipts"));
            }
        }

        public async Task<OrderLastModifiedDatesDto> GetOrderLastModifiedDates(int orderId)
        {
            var result = await _orderRepository.GetAll()
                .Where(x => x.Id == orderId)
                .Select(x => new OrderLastModifiedDatesDto
                {
                    LastModificationTime = x.LastModificationTime,
                    LastModifierName = x.LastModifierUser.Name + " " + x.LastModifierUser.Surname,
                    CreationTime = x.CreationTime,
                    CreatorName = x.CreatorUser.Name + " " + x.CreatorUser.Surname
                }).FirstAsync();

            var timeZone = await GetTimezone();
            result.CreationTime = result.CreationTime.ConvertTimeZoneTo(timeZone);
            result.LastModificationTime = result.LastModificationTime?.ConvertTimeZoneTo(timeZone);

            return result;
        }

        [UnitOfWork(IsDisabled = true)]
        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<SetOrderDateResult> SetOrderDate(SetOrderDateInput input)
        {
            using (var unitOfWork = UnitOfWorkManager.Begin(new UnitOfWorkOptions { IsTransactional = true }))
            {
                var result = await SetOrderDateInternal(input);
                if (result.Completed)
                {
                    if (input.OrderLineId.HasValue)
                    {
                        var newOrder = await _orderLineRepository.GetAll()
                            .Where(x => x.Id == input.OrderLineId)
                            .Select(x => new
                            {
                                x.OrderId
                            }).FirstAsync();
                        await UpdateOrderLineDatesIfNeeded(input.Date, newOrder.OrderId);
                    }
                    else
                    {
                        await UpdateOrderLineDatesIfNeeded(input.Date, input.OrderId);
                    }

                    await unitOfWork.CompleteAsync();
                }
                return result;
            }
        }

        private async Task<SetOrderDateResult> SetOrderDateInternal(SetOrderDateInput input)
        {
            await CheckUseShiftSettingCorrespondsInput(input.Shift);
            SetOrderDateResult result = new SetOrderDateResult();
            var order = await _orderRepository.GetAsync(input.OrderId);
            if (order.DeliveryDate == input.Date && order.Shift == input.Shift)
            {
                result.Completed = true;
                return result;
            }
            if (input.OrderLineId == null)
            {
                var hasCompletedOrderLines = await _orderLineRepository.GetAll()
                    .Where(x => x.OrderId == input.OrderId)
                    .AnyAsync(x => x.IsComplete);

                if (hasCompletedOrderLines)
                {
                    throw new UserFriendlyException(L("Order_Edit_Error_HasCompletedOrderLines"));
                }

                await CheckOpenDispatchesForOrder(input.OrderId);
                await ThrowIfOrderLinesHaveTickets(input.OrderId);

                if (!input.KeepTrucks)
                {
                    await DeleteOrderLineTrucks(x => x.OrderLine.OrderId == input.OrderId);
                }
                else
                {
                    await DeleteOrderLineTrucks(x => x.OrderLine.OrderId == input.OrderId
                        && (!x.Truck.IsActive
                            || x.Truck.IsOutOfService
                            || x.Truck.LocationId == null
                            || x.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule), input.Date);
                    var notAvailableTrucks = await GetOrderTrucksNotAvailableForDateShift(input.OrderId, input.Date, input.Shift);
                    notAvailableTrucks.AddRange(await GetOrderTrucksWithoutDriverOnDateShift(input.OrderId, input.Date, input.Shift));
                    if (await ShouldReturnResultOrDeleteNotAvailableTrucks(notAvailableTrucks, input.RemoveNotAvailableTrucks, result))
                    {
                        return result;
                    }

                    if (order.DeliveryDate != null)
                    {
                        await UpdateDriverAssignmentsDateShift(order.Id, order.DeliveryDate.Value, order.Shift, input.Date.Date, input.Shift);
                    }
                }
                order.DeliveryDate = input.Date.Date;
                order.Shift = input.Shift;
            }
            else
            {
                var orderLine = await _orderLineRepository.GetAll()
                    .Where(x => x.Id == input.OrderLineId)
                    .Select(x => new
                    {
                        x.IsComplete
                    }).SingleAsync();

                if (orderLine.IsComplete)
                {
                    throw new UserFriendlyException(L("Order_Edit_Error_HasCompletedOrderLines"));
                }

                await CheckOpenDispatchesForOrderLine(input.OrderLineId.Value);
                await ThrowIfOrderLineHasTicketsOrActualAmounts(input.OrderLineId.Value);

                if (!input.KeepTrucks)
                {
                    await DeleteOrderLineTrucks(x => x.OrderLineId == input.OrderLineId);
                }
                else
                {
                    await DeleteOrderLineTrucks(x => x.OrderLineId == input.OrderLineId
                        && (!x.Truck.IsActive
                            || x.Truck.IsOutOfService
                            || x.Truck.LocationId == null
                            || x.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule), input.Date);
                    var notAvailableTrucks = await GetOrderLineTrucksNotAvailableForDateShift(input.OrderLineId.Value, input.Date, input.Shift);
                    notAvailableTrucks.AddRange(await GetOrderLineTrucksWithoutDriverOnDateShift(input.OrderLineId.Value, input.Date, input.Shift));
                    if (await ShouldReturnResultOrDeleteNotAvailableTrucks(notAvailableTrucks, input.RemoveNotAvailableTrucks, result))
                    {
                        return result;
                    }

                    Debug.Assert(order.DeliveryDate != null, "order.DeliveryDate != null");
                    await CopyDriverAssignmentsForOrderLineWithNewDateShift(input.OrderLineId.Value, order.DeliveryDate.Value, order.Shift, input.Date.Date, input.Shift);
                }
                Order newOrder = await CreateOrderCopyAndAssignOrderLineToIt(order, input.OrderLineId.Value);
                newOrder.DeliveryDate = input.Date.Date;
                newOrder.Shift = input.Shift;
            }
            result.Completed = true;
            return result;

            // Local functions
            async Task<List<NotAvailableOrderLineTruck>> GetOrderLineTrucksWithoutDriverOnDateShift(int orderLineId, DateTime date, Shift? shift) =>
                await GetTrucksWithoutDriverOnDateShift(null, orderLineId, date, shift);
            async Task<List<NotAvailableOrderLineTruck>> GetOrderTrucksWithoutDriverOnDateShift(int orderId, DateTime date, Shift? shift) =>
                await GetTrucksWithoutDriverOnDateShift(orderId, null, date, shift);
            async Task<List<NotAvailableOrderLineTruck>> GetTrucksWithoutDriverOnDateShift(int? orderId, int? orderLineId, DateTime date, Shift? shift)
            {
                return await _orderLineTruckRepository.GetAll()
                    .WhereIf(orderId.HasValue, olt => olt.OrderLine.OrderId == orderId)
                    .WhereIf(orderLineId.HasValue, olt => olt.OrderLineId == orderLineId.Value)
                    .Where(olt => olt.Truck.DriverAssignments.Any(da => da.Date == date && da.Shift == shift && da.DriverId == null))
                    .Select(olt => new NotAvailableOrderLineTruck(olt.TruckId, olt.OrderLineId, olt.Truck.TruckCode, olt.Utilization))
                    .ToListAsync();
            }

            async Task UpdateDriverAssignmentsDateShift(int orderId, DateTime date, Shift? shift, DateTime newDate, Shift? newShift)
            {
                await UpdateOrCopyDriverAssignments(orderId, null, date, shift, newDate, newShift);
            }

            async Task CopyDriverAssignmentsForOrderLineWithNewDateShift(int orderLineId, DateTime date, Shift? shift, DateTime newDate, Shift? newShift)
            {
                await UpdateOrCopyDriverAssignments(null, orderLineId, date, shift, newDate, newShift);
            }

            async Task UpdateOrCopyDriverAssignments(int? orderId, int? orderLineId, DateTime date, Shift? shift, DateTime newDate, Shift? newShift)
            {
                var timezone = await GetTimezone();
                var driverAssignmentsToCopy = await GetDriverAssignmentsForOrderOrOrderLineTrucksOnDateShift(orderId, orderLineId, date, shift);
                var existingDriverAssignmentsOnNewDateShift = await GetDriverAssignmentsForOrderOrOrderLineTrucksOnDateShift(orderId, orderLineId, newDate, newShift);
                foreach (var driverAssignment in driverAssignmentsToCopy)
                {
                    if (existingDriverAssignmentsOnNewDateShift.Any(da =>
                            da.TruckId == driverAssignment.TruckId &&
                            da.Date == newDate &&
                            da.Shift == newShift))
                    {
                        continue;
                    }

                    if (orderId.HasValue)
                    {
                        driverAssignment.Date = newDate;
                        driverAssignment.Shift = newShift;
                    }
                    else
                    {
                        var startTime = driverAssignment.StartTime?.ConvertTimeZoneTo(timezone);
                        DriverAssignment driverAssignmentToCopy = new DriverAssignment
                        {
                            Date = newDate,
                            Shift = newShift,
                            DriverId = driverAssignment.DriverId,
                            TruckId = driverAssignment.TruckId,
                            OfficeId = driverAssignment.OfficeId,
                            StartTime = (startTime == null ? (DateTime?)null : newDate.Date.Add(startTime.Value.TimeOfDay))?.ConvertTimeZoneFrom(timezone),
                        };
                        existingDriverAssignmentsOnNewDateShift.Add(driverAssignmentToCopy);
                        await _driverAssignmentRepository.InsertAsync(driverAssignmentToCopy);
                    }
                }

            }

            async Task<IList<DriverAssignment>> GetDriverAssignmentsForOrderOrOrderLineTrucksOnDateShift(int? orderId, int? orderLineId, DateTime date, Shift? shift)
            {
                Debug.Assert(orderId.HasValue || orderLineId.HasValue);
                Debug.Assert(!orderId.HasValue || !orderLineId.HasValue);
                return await _orderLineTruckRepository.GetAll()
                    .WhereIf(orderId.HasValue, olt => olt.OrderLine.OrderId == orderId)
                    .WhereIf(orderLineId.HasValue, olt => olt.OrderLineId == orderLineId)
                    .SelectMany(olt => olt.Truck.DriverAssignments)
                    .Where(da => da.Date == date && da.Shift == shift)
                    .Select(da => da)
                    .ToListAsync();
            }
        }

        private async Task DeleteOrderLineTrucks(Expression<Func<OrderLineTruck, bool>> filter, DateTime? deliveryDate = null)
        {
            var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                .Where(filter)
                .ToListAsync();

            orderLineTrucks.ForEach(_orderLineTruckRepository.Delete);

            var today = await GetToday();
            var orderLineIds = orderLineTrucks.Select(x => x.OrderLineId).Distinct().ToList();
            var orderLineDates = await _orderLineRepository.GetAll()
                .Select(x => new
                {
                    x.Id,
                    x.Order.DeliveryDate
                }).ToListAsync();

            await CurrentUnitOfWork.SaveChangesAsync();
            foreach (var orderLineId in orderLineIds)
            {
                if ((deliveryDate ?? orderLineDates.FirstOrDefault(x => x.Id == orderLineId)?.DeliveryDate) >= today)
                {
                    var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLineId);
                    orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                    await orderLineUpdater.SaveChangesAsync();
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task UpdateOrderLineDatesIfNeeded(DateTime? deliveryDate, int? orderId)
        {
            if (deliveryDate == null || !(orderId > 0))
            {
                return;
            }

            var orderLines = await _orderLineRepository.GetAll()
                .Where(x => x.OrderId == orderId)
                .ToListAsync();

            var date = deliveryDate.Value;
            var timezone = await GetTimezone();

            var notStaggeredOrderLineIds = new List<int>();

            foreach (var orderLine in orderLines)
            {
                var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLine.Id);
                await orderLineUpdater.UpdateFieldAsync(x => x.FirstStaggeredTimeOnJob, date.AddTimeOrNull(orderLine.FirstStaggeredTimeOnJob?.ConvertTimeZoneTo(timezone))?.ConvertTimeZoneFrom(timezone));
                await orderLineUpdater.UpdateFieldAsync(x => x.TimeOnJob, date.AddTimeOrNull(orderLine.TimeOnJob?.ConvertTimeZoneTo(timezone))?.ConvertTimeZoneFrom(timezone));
                await orderLineUpdater.SaveChangesAsync();

                //staggered OLT time will be recalculated when FirstStaggeredTimeOnJob changes above, so we only need to handle non-staggered ones
                if (orderLine.StaggeredTimeKind == StaggeredTimeKind.None)
                {
                    notStaggeredOrderLineIds.Add(orderLine.Id);
                }
            }

            if (notStaggeredOrderLineIds.Any())
            {
                var orderLineTrucks = await _orderLineTruckRepository.GetAll()
                    .Where(x => notStaggeredOrderLineIds.Contains(x.OrderLineId))
                    .ToListAsync();

                foreach (var orderLineTruck in orderLineTrucks)
                {
                    orderLineTruck.TimeOnJob = date.AddTimeOrNull(orderLineTruck.TimeOnJob?.ConvertTimeZoneTo(timezone))?.ConvertTimeZoneFrom(timezone);
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task CheckOpenDispatchesForOrderLine(int orderLineId)
        {
            if (await _orderLineRepository.GetAll()
                    .Where(x => x.Id == orderLineId)
                    .AnyAsync(x => x.Dispatches.Any(d => Dispatch.OpenStatuses.Contains(d.Status))))
            {
                throw new UserFriendlyException(L("Order_Edit_Error_HasDispatches"));
            }
        }

        private async Task CheckOpenDispatchesForOrder(int orderId)
        {
            if (await _orderLineRepository.GetAll()
                    .Where(x => x.OrderId == orderId)
                    .AnyAsync(x => x.Dispatches.Any(d => Dispatch.OpenStatuses.Contains(d.Status))))
            {
                throw new UserFriendlyException(L("Order_Edit_Error_HasDispatches"));
            }
        }

        private async Task<bool> HasLoadedDispatchesForOrder(int orderId)
        {
            return await _orderLineRepository.GetAll()
                    .AnyAsync(ol => ol.OrderId == orderId && ol.Dispatches.Any(d => d.Status == DispatchStatus.Loaded || d.Status == DispatchStatus.Completed));
        }

        private async Task<bool> HasManualTicketsForOrder(int orderId)
        {
            return await _orderLineRepository.GetAll()
                .AnyAsync(ol => ol.OrderId == orderId
                    && ol.Tickets.Any(t => t.Load == null));
        }

        private async Task<bool> ShouldReturnResultOrDeleteNotAvailableTrucks(
            List<NotAvailableOrderLineTruck> notAvailableTrucks,
            bool removeNotAvailableTrucks,
            SetOrderDateResult result
        )
        {
            if (notAvailableTrucks.Count > 0)
            {
                if (removeNotAvailableTrucks)
                {
                    var truckIdsPerOrderLineId = notAvailableTrucks.GroupBy(x => x.OrderLineId);
                    foreach (var truckIdGroup in truckIdsPerOrderLineId)
                    {
                        var orderLineId = truckIdGroup.Key;
                        var truckIds = truckIdGroup.Select(x => x.TruckId).Distinct().ToList();
                        await DeleteOrderLineTrucks(x => x.OrderLineId == orderLineId && truckIds.Contains(x.TruckId));
                    }
                }
                else
                {
                    result.NotAvailableTrucks = notAvailableTrucks.Select(x => x.TruckCode).ToList();
                    return true;
                }
            }
            return false;
        }
        private async Task<List<NotAvailableOrderLineTruck>> GetOrderTrucksNotAvailableForDateShift(int orderId, DateTime date, Shift? shift)
        {
            return await GetTrucksNotAvailableForDateShift(orderId, null, date, shift);
        }
        private async Task<List<NotAvailableOrderLineTruck>> GetOrderLineTrucksNotAvailableForDateShift(int orderLineId, DateTime date, Shift? shift)
        {
            return await GetTrucksNotAvailableForDateShift(null, orderLineId, date, shift);
        }
        private async Task<List<NotAvailableOrderLineTruck>> GetTrucksNotAvailableForDateShift(int? orderId, int? orderLineId, DateTime date, Shift? shift)
        {
            Debug.Assert(orderId.HasValue && !orderLineId.HasValue || !orderId.HasValue && orderLineId.HasValue);
            var orderTrucks = _orderLineTruckRepository.GetAll()
                .WhereIf(orderId.HasValue, olt => olt.OrderLine.OrderId == orderId.Value)
                .WhereIf(orderLineId.HasValue, olt => olt.OrderLineId == orderLineId.Value)
                .Select(olt => new { olt.TruckId, olt.OrderLineId, olt.Truck.TruckCode, olt.Utilization });
            var anotherDateOrdersTrucks = _orderLineTruckRepository.GetAll()
                .Where(olt => olt.OrderLine.Order.DeliveryDate == date && olt.OrderLine.Order.Shift == shift)
                .Select(olt => new { olt.TruckId, olt.Utilization });
            var notAvailableTrucks =
                from ot in orderTrucks
                join aot in anotherDateOrdersTrucks on ot.TruckId equals aot.TruckId
                where (ot.Utilization + aot.Utilization) > 1
                select new NotAvailableOrderLineTruck(ot.TruckId, ot.OrderLineId, ot.TruckCode, ot.Utilization + aot.Utilization);
            return await notAvailableTrucks.ToListAsync();
        }
        private class NotAvailableOrderLineTruck
        {
            public NotAvailableOrderLineTruck(int truckId, int orderLineId, string truckCode, decimal utilization)
            {
                TruckId = truckId;
                OrderLineId = orderLineId;
                TruckCode = truckCode;
                Utilization = utilization;
            }
            public int TruckId { get; }
            public int OrderLineId { get; }
            public string TruckCode { get; }
            public decimal Utilization { get; }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task SetOrderOfficeId(SetOrderOfficeIdInput input)
        {
            var order = await _orderRepository.GetAsync(input.OrderId);
            if (order.LocationId == input.OfficeId)
            {
                return;
            }
            if (input.OrderLineId == null)
            {
                await CheckOrderPrerequisites(input.OrderId);
                await ThrowIfOrderLinesHaveTickets(input.OrderId);
                await ThrowIfOrderHasReceiptsForOwnerOfficeId(input.OrderId);
                order.LocationId = input.OfficeId;

                await _orderLineRepository.DeleteAsync(x => x.OrderId == input.OrderId
                    && (x.MaterialQuantity == null || x.MaterialQuantity == 0)
                    && (x.FreightQuantity == null || x.FreightQuantity == 0)
                    && (x.NumberOfTrucks == null || x.NumberOfTrucks < 0.01));
                await CurrentUnitOfWork.SaveChangesAsync();
                await _orderTaxCalculator.CalculateTotalsAsync(order.Id);
            }
            else
            {
                if (await _orderLineRepository.GetAll()
                    .AnyAsync(ol => ol.Id == input.OrderLineId && (
                                    ol.SharedOrderLines.Any() ||
                                    ol.IsComplete ||
                                    ol.Tickets.Any() ||
                                    ol.OrderLineTrucks.Any()
                    ))
                )
                {
                    throw new UserFriendlyException("The Order Line cannot be transferred!");
                }
                await ThrowIfOrderLineHasTicketsOrActualAmounts(input.OrderLineId.Value);
                await ThrowIfOrderLineHasReceiptsForOwnerOfficeId(input.OrderLineId.Value);
                Order newOrder = await CreateOrderCopyAndAssignOrderLineToIt(order, input.OrderLineId.Value);
                newOrder.LocationId = input.OfficeId;
                await CurrentUnitOfWork.SaveChangesAsync();
                await _orderTaxCalculator.CalculateTotalsAsync(newOrder.Id);
                await _orderTaxCalculator.CalculateTotalsAsync(order.Id);
            }
        }
        private async Task CheckOrderPrerequisites(int orderId)
        {
            if (await _orderRepository.GetAll()
                .AnyAsync(o => o.Id == orderId && (
                                   o.OrderLines.Any(ol => ol.SharedOrderLines.Any()) ||
                                   o.OrderLines.Any(ol => ol.IsComplete) ||
                                   o.OrderLines.Any(ol => ol.Tickets.Any()) ||
                                   o.OrderLines.Any(ol => ol.OrderLineTrucks.Any())
                               ))
            )
            {
                throw new UserFriendlyException("The Order cannot be transferred!");
            }
        }

        private async Task<Order> CreateOrderCopyAndAssignOrderLineToIt(Order order, int orderLineId)
        {
            var newOrder = order.CreateCopy();
            await _orderRepository.InsertAsync(newOrder);
            var orderLine = await _orderLineRepository.GetAsync(orderLineId);
            orderLine.Order = newOrder;

            await DecrementOrderLineNumbers(order.Id, orderLine.LineNumber);
            orderLine.LineNumber = 1;

            await CurrentUnitOfWork.SaveChangesAsync();

            await _orderTaxCalculator.CalculateTotalsAsync(order.Id);
            await _orderTaxCalculator.CalculateTotalsAsync(newOrder.Id);

            return newOrder;
        }



        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<bool> DoesOrderHaveMultipleLines(int orderId) =>
            await _orderLineRepository.GetAll().CountAsync(ol => ol.OrderId == orderId) > 1;

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<int[]> CopyOrder(CopyOrderInput input)
        {
            if (input.DateBegin > input.DateEnd)
            {
                throw new ArgumentException($"{nameof(input.DateBegin)} must be less or equal than {nameof(input.DateEnd)}");
            }

            if ((input.DateEnd - input.DateBegin).Days + 1 > 7)
            {
                throw new ArgumentException("You cannot copy order for more than 7 days.");
            }

            await CheckUseShiftSettingCorrespondsInput(input.Shifts);
            Shift?[] shifts = input.Shifts.ToNullableArrayWithNullElementIfEmpty();

            var order = await _orderRepository.GetAll()
                .AsNoTracking()
                .Include(x => x.OrderLines)
                .Include(x => x.SharedOrders)
                .FirstAsync(x => x.Id == input.OrderId);

            bool allowCopyZeroQuantity = await FeatureChecker.IsEnabledAsync(AppFeatures.AllowCopyingZeroQuantityOrderLineItemsFeature);
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
            var timezone = await GetTimezone();

            List<int> createdOrderIds = new List<int>();
            DateTime currentDate = input.DateBegin;
            while (currentDate <= input.DateEnd)
            {
                foreach (var shift in shifts)
                {
                    var newOrder = order.CreateCopy();
                    newOrder.IsClosed = false;
                    newOrder.DeliveryDate = currentDate.Date;
                    newOrder.Shift = shift;

                    var newOrderLines = order.OrderLines.ToList();

                    bool copySingleOrderLine = newOrderLines
                        .WhereIf(input.OrderLineId.HasValue, ol => ol.Id == input.OrderLineId)
                        .WhereIf(!allowCopyZeroQuantity && !input.OrderLineId.HasValue, ol => ol.MaterialQuantity > 0 || ol.FreightQuantity > 0 || ol.NumberOfTrucks > 0)
                        .Count() == 1;

                    newOrderLines = order.OrderLines
                        .WhereIf(input.OrderLineId.HasValue, ol => ol.Id == input.OrderLineId)
                        .WhereIf(!allowCopyZeroQuantity && !input.OrderLineId.HasValue, ol => ol.MaterialQuantity > 0 || ol.FreightQuantity > 0 || ol.NumberOfTrucks > 0)
                        .Select(x => new OrderLine
                        {
                            LineNumber = copySingleOrderLine ? 1 : x.LineNumber,
                            QuoteServiceId = x.QuoteServiceId,
                            MaterialQuantity = x.MaterialQuantity,
                            FreightQuantity = x.FreightQuantity,
                            NumberOfTrucks = x.NumberOfTrucks,
                            ScheduledTrucks = input.CopyTrucks ? x.ScheduledTrucks : null,
                            MaterialPricePerUnit = x.MaterialPricePerUnit,
                            FreightPricePerUnit = x.FreightPricePerUnit,
                            IsMaterialPricePerUnitOverridden = x.IsMaterialPricePerUnitOverridden,
                            IsFreightPricePerUnitOverridden = x.IsFreightPricePerUnitOverridden,
                            ServiceId = x.ServiceId,
                            LoadAtId = x.LoadAtId,
                            DeliverToId = x.DeliverToId,
                            FreightUomId = x.FreightUomId,
                            MaterialUomId = x.MaterialUomId,
                            Designation = x.Designation,
                            MaterialPrice = x.MaterialPrice,
                            FreightPrice = x.FreightPrice,
                            IsMaterialPriceOverridden = x.IsMaterialPriceOverridden,
                            IsFreightPriceOverridden = x.IsFreightPriceOverridden,
                            LeaseHaulerRate = x.LeaseHaulerRate,
                            TimeOnJob = x.TimeOnJob == null ? null : (currentDate.Date.Add(x.TimeOnJob.Value.ConvertTimeZoneTo(timezone).TimeOfDay)).ConvertTimeZoneFrom(timezone),
                            FirstStaggeredTimeOnJob = x.FirstStaggeredTimeOnJob == null ? null : (currentDate.Date.Add(x.FirstStaggeredTimeOnJob.Value.ConvertTimeZoneTo(timezone).TimeOfDay)).ConvertTimeZoneFrom(timezone),
                            StaggeredTimeKind = x.StaggeredTimeKind,
                            StaggeredTimeInterval = x.StaggeredTimeInterval,
                            JobNumber = x.JobNumber,
                            Note = x.Note,
                            IsMultipleLoads = x.IsMultipleLoads,
                            ProductionPay = allowProductionPay && x.ProductionPay,
                            Order = newOrder,
                        }).ToList();

                    foreach (var newOrderLine in newOrderLines)
                    {
                        _orderLineRepository.Insert(newOrderLine);
                    }

                    var newId = await _orderRepository.InsertAndGetIdAsync(newOrder);
                    newOrder.Id = newId;
                    await _orderTaxCalculator.CalculateTotalsAsync(newOrder.Id);
                    await _fuelSurchargeCalculator.RecalculateOrderLinesWithTicketsForOrder(newOrder.Id);
                    createdOrderIds.Add(newId);
                }
                currentDate = currentDate.AddDays(1);
            }

            return createdOrderIds.ToArray();
        }

        public async Task RecalculateStaggeredTimeForOrders(RecalculateStaggeredTimeForOrdersInput input)
        {
            var orderLines = await _orderLineRepository.GetAll()
                .Where(x => input.OrderIds.Contains(x.OrderId))
                .Select(x => new
                {
                    x.Id,
                    x.Order.DeliveryDate
                })
                .ToListAsync();

            var today = await GetToday();
            foreach (var orderLine in orderLines)
            {
                if (orderLine.DeliveryDate >= today)
                {
                    var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLine.Id);
                    orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                    await orderLineUpdater.SaveChangesAsync();
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<OrderInternalNotesDto> GetOrderInternalNotes(EntityDto input)
        {
            var order = await _orderRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.Id,
                    x.HasInternalNotes,
                    x.EncryptedInternalNotes
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            var result = new OrderInternalNotesDto
            {
                OrderId = order.Id
            };

            if (order.HasInternalNotes)
            {
                result.InternalNotes = _encryptionService.Decrypt(order.EncryptedInternalNotes);
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task SetOrderInternalNotes(OrderInternalNotesDto input)
        {
            var order = await _orderRepository.GetAsync(input.OrderId);

            if (string.IsNullOrEmpty(input.InternalNotes))
            {
                order.EncryptedInternalNotes = null;
                order.HasInternalNotes = false;
            }
            else
            {
                order.EncryptedInternalNotes = _encryptionService.Encrypt(input.InternalNotes);
                order.HasInternalNotes = true;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<SharedOrderListDto> GetSharedOrders(EntityDto input)
        {
            var order = await _orderRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    OfficeId = x.LocationId
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            var shares = await _sharedOrderRepository.GetAll()
                .Where(x => x.OrderId == input.Id)
                .Select(x => x.OfficeId)
                .ToListAsync();

            var offices = await _officeRepository.GetAll()
                .Where(x => x.Id != order.OfficeId)
                .OrderBy(x => x.Name)
                .Select(x => new SharedOrderDto
                {
                    OfficeId = x.Id,
                    OfficeName = x.Name,
                    Checked = shares.Contains(x.Id)
                })
                .ToListAsync();

            return new SharedOrderListDto
            {
                OrderId = input.Id,
                SharedOrders = offices
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<SharedOrderLineListDto> GetSharedOrderLines(EntityDto input)
        {
            int officeId = await GetOrderLineOfficeId(input.Id);
            var sharedOrderLines = await _sharedOrderLineRepository.GetAll()
                .Where(x => x.OrderLineId == input.Id)
                .Select(x => x.OfficeId)
                .ToListAsync();

            var offices = await _officeRepository.GetAll()
                .Where(x => x.Id != officeId)
                .OrderBy(x => x.Name)
                .Select(x => new SharedOrderDto
                {
                    OfficeId = x.Id,
                    OfficeName = x.Name,
                    Checked = sharedOrderLines.Contains(x.Id)
                })
                .ToListAsync();

            return new SharedOrderLineListDto
            {
                OrderLineId = input.Id,
                SharedOrders = offices
            };
        }
        private async Task<int> GetOrderLineOfficeId(int orderLineId)
        {
            return await _orderRepository.GetAll()
                .Where(o => o.OrderLines.Any(ol => ol.Id == orderLineId))
                .Select(o => o.LocationId)
                .SingleAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        [RequiresFeature(AppFeatures.AllowSharedOrdersFeature)]
        public async Task SetSharedOrderLines(SetSharedOrderLineInput input)
        {
            await CheckOrderDateIsNotInPast(input.OrderLineId);
            var sharedOrderLines = await GetExistingSharedOrderLines(input.OrderLineId);
            await DeletePreviousSharedOrderLinesNotSharedNowAsync(sharedOrderLines, input.CheckedOfficeIds);

            int[] officeIdsToShare = input.CheckedOfficeIds.Where(id => sharedOrderLines.All(sol => sol.OfficeId != id)).ToArray();
            if (officeIdsToShare.Length == 0)
            {
                return;
            }
            CreateSharedOrderLineForEachOffice(input.OrderLineId, officeIdsToShare);
            await UpdateSharedDateTime(input.OrderLineId);
            await _orderTaxCalculator.CalculateTotalsForOrderLineAsync(input.OrderLineId);
        }
        private async Task CheckOrderDateIsNotInPast(int orderLineId)
        {
            var orderDate = await _orderRepository.GetAll()
                .Where(o => o.OrderLines.Any(ol => ol.Id == orderLineId))
                .Select(o => o.DeliveryDate)
                .SingleAsync();
            if (!orderDate.HasValue)
            {
                throw new ApplicationException("The Order doesn't have the Delivery Date!");
            }
            if (orderDate.Value < (await GetToday()).Date)
            {
                throw new ApplicationException("The Order is in the past!");
            }
        }
        private async Task<List<SharedOrderLine>> GetExistingSharedOrderLines(int orderLineId)
        {
            return await _sharedOrderLineRepository.GetAll()
                .Where(sol => sol.OrderLineId == orderLineId)
                .ToListAsync();
        }
        private async Task DeletePreviousSharedOrderLinesNotSharedNowAsync(List<SharedOrderLine> sharedOrderLines, int[] sharedOffices)
        {
            var sharedOrdersToDelete = sharedOrderLines
                .Where(sol => !sharedOffices.Contains(sol.OfficeId))
                .ToList();

            if (!sharedOrdersToDelete.Any())
            {
                return;
            }

            var orderLineId = sharedOrderLines.First().OrderLineId;
            var officeIds = sharedOrderLines.Select(x => x.OfficeId).ToList();
            var receipts = await _receiptRepository.GetAll()
                .Where(x => x.Order.OrderLines.Any(ol => ol.Id == orderLineId) && officeIds.Contains(x.OfficeId))
                .Select(x => new
                {
                    OfficeName = x.Office.Name,
                    OfficeId = x.OfficeId,

                }).ToListAsync();

            if (receipts.Any())
            {
                if (receipts.Count > 1)
                {
                    throw new UserFriendlyException("Unable to revoke order line shares", $"Offices {string.Join(", ", receipts.Select(x => x.OfficeName))} have receipts associated with it.");
                }
                else
                {
                    throw new UserFriendlyException("Unable to revoke order line share", $"{receipts.First().OfficeName} office has a receipt associated with it.");
                }
            }

            sharedOrdersToDelete.ForEach(async sol => await _sharedOrderLineRepository.DeleteAsync(sol));
        }
        private void CreateSharedOrderLineForEachOffice(int orderLineId, int[] officeIdsToShare)
        {
            foreach (int officeId in officeIdsToShare)
            {
                _sharedOrderLineRepository.Insert(new SharedOrderLine
                {
                    OrderLineId = orderLineId,
                    OfficeId = officeId,
                });
            }
        }
        private async Task UpdateSharedDateTime(int orderLineId)
        {
            var orderLine = await _orderLineRepository.GetAsync(orderLineId);
            if (orderLine.SharedDateTime == null)
            {
                orderLine.SharedDateTime = Clock.Now;
            }
        }

        private async Task NotifyAboutSharedOrder(List<int> newSharedOfficeIds, Order order)
        {
            var users = await UserManager.Users
                .Where(x => x.OfficeId.HasValue && newSharedOfficeIds.Contains(x.OfficeId.Value))
                .ToListAsync();

            var userIds = new List<Abp.UserIdentifier>();

            foreach (var user in users)
            {
                if (await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Dispatching))
                {
                    userIds.Add(user.ToUserIdentifier());
                }
            }

            await _appNotifier.OrderSharedAsync(userIds.ToArray(), order);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<int> GetOrderDuplicateCount(GetOrderDuplicateCountInput input)
        {
            return await _orderRepository.GetAll()
                .Where(x => x.Id != input.Id
                        && x.CustomerId == input.CustomerId
                        && x.QuoteId == input.QuoteId
                        && x.DeliveryDate == input.DeliveryDate)
                .CountAsync();
        }

        private async Task EnsureOrderCanBeDeletedAsync(EntityDto input)
        {
            var record = await _orderRepository.GetAll()
                .Where(o => o.Id == input.Id)
                .Select(o => new
                {
                    IsClosed = o.IsClosed,
                    HasCompletedOrderLines = o.OrderLines.Any(ol => ol.IsComplete),
                    HasRelatedData =
                        o.SharedOrders.Any() ||
                        o.BilledOrders.Any() ||
                        o.OrderEmails.Any() ||
                        o.OrderLines.Any(ol => ol.Tickets.Any()) ||
                        o.OrderLines.Any(ol => ol.ReceiptLines.Any()) ||
                        o.OrderLines.Any(ol => ol.Dispatches.Any()) ||
                        o.OrderLines.Any(ol => ol.OrderLineTrucks.Any()) ||
                        o.OrderLines.Any(ol => ol.SharedOrderLines.Any()) ||
                        o.HasLinkedHaulingCompanyOrders ||
                        o.OrderLines.Any(ol => ol.HaulingCompanyOrderLineId != null)
                })
                .SingleAsync();

            if (record.IsClosed)
            {
                throw new UserFriendlyException(L("Order_Delete_Error_OrderClosed"));
            }

            if (record.HasCompletedOrderLines)
            {
                throw new UserFriendlyException(L("Order_Delete_Error_HasCompletedOrderLines"));
            }

            if (record.HasRelatedData)
            {
                throw new UserFriendlyException(L("Order_Delete_Error_HasRelatedData"));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task DeleteOrder(EntityDto input)
        {
            await EnsureOrderCanBeDeletedAsync(input);

            var order = await _orderRepository.GetAll()
                .Include(x => x.OrderLines)
                .Where(x => x.Id == input.Id)
                .FirstAsync();

            var orderLines = order.OrderLines.ToList();

            foreach (var orderLine in orderLines)
            {
                await _orderLineRepository.DeleteAsync(orderLine);
            }
            await _orderRepository.DeleteAsync(order);
            await CurrentUnitOfWork.SaveChangesAsync();

            foreach (var orderLine in orderLines)
            {
                if (orderLine.MaterialCompanyOrderLineId.HasValue
                    && orderLine.MaterialCompanyTenantId.HasValue)
                {
                    using (CurrentUnitOfWork.SetTenantId(orderLine.MaterialCompanyTenantId))
                    {
                        var materialOrderLine = await _orderLineRepository.GetAsync(orderLine.MaterialCompanyOrderLineId.Value);
                        materialOrderLine.HaulingCompanyOrderLineId = null;
                        materialOrderLine.HaulingCompanyTenantId = null;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }
            if (order.MaterialCompanyOrderId.HasValue
                && order.MaterialCompanyTenantId.HasValue)
            {
                var otherHaulingOrdersExist = false;
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant))
                {
                    otherHaulingOrdersExist = await _orderRepository.GetAll()
                        .AnyAsync(x => x.MaterialCompanyOrderId == order.MaterialCompanyOrderId && x.Id != order.Id);
                }
                if (!otherHaulingOrdersExist)
                {
                    using (CurrentUnitOfWork.SetTenantId(order.MaterialCompanyTenantId))
                    {
                        var materialOrder = await _orderRepository.GetAsync(order.MaterialCompanyOrderId.Value);
                        materialOrder.HasLinkedHaulingCompanyOrders = false;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }
        }


        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<PagedResultDto<OrderLineEditDto>> GetOrderLines(GetOrderLinesInput input)
        {
            if (input.OrderId.HasValue)
            {
                var query = _orderLineRepository.GetAll();

                var totalCount = await query.CountAsync();

                var order = await _orderRepository.GetAll()
                    .Where(x => x.Id == input.OrderId)
                    .Select(x => new
                    {
                        x.QuoteId,
                        x.LocationId
                    }).SingleOrDefaultAsync();

                var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);

                var orderLines = await query
                    .Where(x => x.OrderId == input.OrderId)
                    .WhereIf(input.LoadAtId.HasValue || input.ForceDuplicateFilters,
                             x => x.LoadAtId == input.LoadAtId)
                    .WhereIf(input.ServiceId.HasValue,
                             x => x.ServiceId == input.ServiceId)
                    .WhereIf(input.MaterialUomId.HasValue,
                             x => x.MaterialUomId == input.MaterialUomId)
                    .WhereIf(input.FreightUomId.HasValue,
                             x => x.FreightUomId == input.FreightUomId)
                    .WhereIf(input.Designation.HasValue,
                             x => x.Designation == input.Designation)
                    .Select(x => new OrderLineEditDto
                    {
                        Id = x.Id,
                        LineNumber = x.LineNumber,
                        QuoteServiceId = x.QuoteServiceId,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightQuantity = x.FreightQuantity,
                        //Tickets = x.Tickets.Select(t => new TicketDto 
                        //{ 
                        //    OfficeId = t.OfficeId, 
                        //    MaterialQuantity = t.MaterialQuantity,
                        //    FreightQuantity = t.FreightQuantity
                        //}).ToList(),
                        //SharedOrderLines = x.SharedOrderLines.Select(s => new OrderLineShareDto { OfficeId = s.OfficeId }).ToList(),
                        MaterialPricePerUnit = x.MaterialPricePerUnit,
                        FreightPricePerUnit = x.FreightPricePerUnit,
                        IsMaterialPricePerUnitOverridden = x.IsMaterialPricePerUnitOverridden,
                        IsFreightPricePerUnitOverridden = x.IsFreightPricePerUnitOverridden,
                        HasQuoteBasedPricing = x.Service.QuoteServices.Any(y => y.QuoteId == order.QuoteId
                            && (y.MaterialUomId == x.MaterialUomId || y.FreightUomId == x.FreightUomId) && y.LoadAtId == x.LoadAtId),
                        ServiceId = x.ServiceId,
                        ServiceName = x.Service.Service1,
                        IsTaxable = x.Service.IsTaxable,
                        LoadAtId = x.LoadAtId,
                        LoadAt = x.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = x.LoadAt.Name,
                            StreetAddress = x.LoadAt.StreetAddress,
                            City = x.LoadAt.City,
                            State = x.LoadAt.State
                        },
                        DeliverToId = x.DeliverToId,
                        DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = x.DeliverTo.Name,
                            StreetAddress = x.DeliverTo.StreetAddress,
                            City = x.DeliverTo.City,
                            State = x.DeliverTo.State
                        },
                        MaterialUomId = x.MaterialUomId,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomId = x.FreightUomId,
                        FreightUomName = x.FreightUom.Name,
                        Designation = x.Designation,
                        MaterialPrice = x.MaterialPrice,
                        FreightPrice = x.FreightPrice,
                        IsMaterialPriceOverridden = x.IsMaterialPriceOverridden,
                        IsFreightPriceOverridden = x.IsFreightPriceOverridden,
                        LeaseHaulerRate = x.LeaseHaulerRate,
                        JobNumber = x.JobNumber,
                        Note = x.Note,
                        IsMultipleLoads = x.IsMultipleLoads,
                        NumberOfTrucks = x.NumberOfTrucks,
                        OrderId = x.OrderId,
                        ProductionPay = allowProductionPay && x.ProductionPay,
                        StaggeredTimeKind = x.StaggeredTimeKind,
                        StaggeredTimeInterval = x.StaggeredTimeInterval,
                        IsTimeStaggeredForTrucks = x.OrderLineTrucks.Any(olt => olt.TimeOnJob != null),
                        FirstStaggeredTimeOnJob = x.FirstStaggeredTimeOnJob,
                        TimeOnJob = x.TimeOnJob,
                        HasTickets = x.Tickets.Any(),
                        HasOpenDispatches = x.Dispatches.Any(d => !Dispatch.ClosedDispatchStatuses.Contains(d.Status)),
                    })
                    .OrderBy(input.Sorting)
                    //.PageBy(input)
                    .ToListAsync();

                var timezone = await GetTimezone();

                foreach (var orderLine in orderLines)
                {
                    if (orderLine.Id.HasValue)
                    {
                        orderLine.CanOverrideTotals = await _orderLineRepository.CanOverrideTotals(orderLine.Id.Value, OfficeId);
                        orderLine.QuoteId = order.QuoteId;
                    }

                    orderLine.TimeOnJob = orderLine.TimeOnJob?.ConvertTimeZoneTo(timezone);
                    orderLine.FirstStaggeredTimeOnJob = orderLine.FirstStaggeredTimeOnJob?.ConvertTimeZoneTo(timezone);
                    //previously, it was also setting the date portion of the above two fields to match Order.DeliveryDate. We might need to implement that again if any historical data is incorrect.
                }

                return new PagedResultDto<OrderLineEditDto>(
                    totalCount,
                    orderLines);
            }
            else if (input.QuoteId.HasValue)
            {
                var orderLines = await _quoteServiceRepository.GetAll()
                    .Where(x => x.QuoteId == input.QuoteId)
                    .Select(x => new OrderLineEditDto
                    {
                        Id = x.Id,
                        QuoteServiceId = x.Id,
                        LoadAtId = x.LoadAtId,
                        LoadAt = x.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = x.LoadAt.Name,
                            StreetAddress = x.LoadAt.StreetAddress,
                            City = x.LoadAt.City,
                            State = x.LoadAt.State
                        },
                        LoadAtNamePlain = x.LoadAt.Name + x.LoadAt.StreetAddress + x.LoadAt.City + x.LoadAt.State, //for sorting
                        DeliverToId = x.DeliverToId,
                        DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = x.DeliverTo.Name,
                            StreetAddress = x.DeliverTo.StreetAddress,
                            City = x.DeliverTo.City,
                            State = x.DeliverTo.State
                        },
                        DeliverToNamePlain = x.DeliverTo.Name + x.DeliverTo.StreetAddress + x.DeliverTo.City + x.DeliverTo.State, //for sorting
                        ServiceId = x.ServiceId,
                        ServiceName = x.Service.Service1,
                        IsTaxable = x.Service.IsTaxable,
                        MaterialUomId = x.MaterialUomId,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomId = x.FreightUomId,
                        FreightUomName = x.FreightUom.Name,
                        Designation = x.Designation,
                        MaterialPricePerUnit = x.PricePerUnit,
                        FreightPricePerUnit = x.FreightRate,
                        LeaseHaulerRate = x.LeaseHaulerRate,
                        //Quantity = x.Quantity, //Do not default quantities. They will have to fill that in.
                        //MaterialQuantity = x.MaterialQuantity,
                        //FreightQuantity = x.FreightQuantity,
                        JobNumber = x.JobNumber,
                        Note = x.Note,
                        CanOverrideTotals = true,
                        QuoteId = input.QuoteId.Value,
                        HasQuoteBasedPricing = x.Service.QuoteServices.Any(y => y.QuoteId == input.QuoteId
                            && (y.MaterialUomId == x.MaterialUomId || y.FreightUomId == x.FreightUomId) && y.LoadAtId == x.LoadAtId)
                    })
                    .OrderBy(input.Sorting)
                    .ToListAsync();

                var defaultToProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.DefaultToProductionPay);
                var preventProductionPayOnHourlyJobs = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.PreventProductionPayOnHourlyJobs);

                var i = 1;
                foreach (var orderLine in orderLines)
                {
                    orderLine.Id = null;
                    orderLine.LineNumber = i++;
                    orderLine.ProductionPay = defaultToProductionPay && (!preventProductionPayOnHourlyJobs || orderLine.FreightUomName?.ToLower().TrimEnd('s') != "hour");
                }

                return new PagedResultDto<OrderLineEditDto>(orderLines.Count, orderLines);
            }
            else
            {
                throw new ArgumentNullException(nameof(input.OrderId));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<OrderLineEditDto> GetOrderLineForEdit(GetOrderLineForEditInput input)
        {
            OrderLineEditDto orderLineEditDto;

            if (input.Id.HasValue)
            {
                var canOverrideTotals = await _orderLineRepository.CanOverrideTotals(input.Id.Value, OfficeId);
                var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);

                orderLineEditDto = await _orderLineRepository.GetAll()
                    .Select(x => new OrderLineEditDto
                    {
                        Id = x.Id,
                        OrderId = x.OrderId,
                        QuoteId = x.Order.QuoteId,
                        QuoteServiceId = x.QuoteServiceId,
                        LineNumber = x.LineNumber,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightQuantity = x.FreightQuantity,
                        MaterialPricePerUnit = x.MaterialPricePerUnit,
                        FreightPricePerUnit = x.FreightPricePerUnit,
                        IsMaterialPricePerUnitOverridden = x.IsMaterialPricePerUnitOverridden,
                        IsFreightPricePerUnitOverridden = x.IsFreightPricePerUnitOverridden,
                        ServiceId = x.ServiceId,
                        ServiceName = x.Service.Service1,
                        LoadAtId = x.LoadAtId,
                        LoadAt = x.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = x.LoadAt.Name,
                            StreetAddress = x.LoadAt.StreetAddress,
                            City = x.LoadAt.City,
                            State = x.LoadAt.State
                        },
                        DeliverToId = x.DeliverToId,
                        DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = x.DeliverTo.Name,
                            StreetAddress = x.DeliverTo.StreetAddress,
                            City = x.DeliverTo.City,
                            State = x.DeliverTo.State
                        },
                        MaterialUomId = x.MaterialUomId,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomId = x.FreightUomId,
                        FreightUomName = x.FreightUom.Name,
                        Designation = x.Designation,
                        MaterialPrice = x.MaterialPrice,
                        FreightPrice = x.FreightPrice,
                        IsMaterialPriceOverridden = x.IsMaterialPriceOverridden,
                        IsFreightPriceOverridden = x.IsFreightPriceOverridden,
                        LeaseHaulerRate = x.LeaseHaulerRate,
                        JobNumber = x.JobNumber,
                        Note = x.Note,
                        NumberOfTrucks = x.NumberOfTrucks,
                        TimeOnJob = x.TimeOnJob,
                        StaggeredTimeKind = x.StaggeredTimeKind,
                        IsMultipleLoads = x.IsMultipleLoads,
                        ProductionPay = allowProductionPay && x.ProductionPay,
                        CanOverrideTotals = canOverrideTotals,
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.Id.Value);

                if (orderLineEditDto == null)
                {
                    throw await GetOrderLineNotFoundException(new EntityDto(input.Id.Value));
                }
            }
            else if (input.OrderId.HasValue)
            {
                var order = await _orderRepository.GetAll()
                    .Select(x => new
                    {
                        x.Id,
                        x.QuoteId,
                        OrderLinesCount = x.OrderLines.Count,
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.OrderId);

                if (order == null)
                {
                    throw await GetOrderNotFoundException(new EntityDto(input.OrderId.Value));
                }

                orderLineEditDto = new OrderLineEditDto
                {
                    OrderId = order.Id,
                    QuoteId = order.QuoteId,
                    LineNumber = order.OrderLinesCount + 1,
                    CanOverrideTotals = true,
                    ProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.DefaultToProductionPay)
                };
            }
            else
            {
                return new OrderLineEditDto
                {
                    CanOverrideTotals = true
                };
            }

            var timezone = await GetTimezone();
            orderLineEditDto.TimeOnJob = orderLineEditDto.TimeOnJob?.ConvertTimeZoneTo(timezone);
            orderLineEditDto.FirstStaggeredTimeOnJob = orderLineEditDto.FirstStaggeredTimeOnJob?.ConvertTimeZoneTo(timezone);

            return orderLineEditDto;
        }

        private async Task<Exception> GetOrderNotFoundException(EntityDto input)
        {
            if (await IsOrderDeleted(input))
            {
                return new EntityDeletedException("Order", "This order has been deleted and can’t be edited");
            }

            return new Exception($"Order with id {input.Id} wasn't found and is not deleted");
        }

        private async Task<Exception> GetOrderLineNotFoundException(EntityDto input)
        {
            var deletedOrderLine = await _orderLineRepository.GetDeletedEntity(input, CurrentUnitOfWork);
            if (deletedOrderLine == null)
            {
                return new Exception($"OrderLine with id {input.Id} wasn't found and is not deleted");
            }

            if (await IsOrderDeleted(new EntityDto(deletedOrderLine.OrderId)))
            {
                return new EntityDeletedException("Order", "This order has been deleted and can’t be edited");
            }

            return new EntityDeletedException("OrderLine", "This order line has been deleted and can’t be edited");
        }

        public async Task<bool> IsOrderDeleted(EntityDto input)
        {
            return await _orderRepository.IsEntityDeleted(input, CurrentUnitOfWork);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<JobEditDto> GetJobForEdit(GetJobForEditInput input)
        {
            var orderLine = await GetOrderLineForEdit(new GetOrderLineForEditInput
            {
                Id = input.OrderLineId
            });

            var order = await GetOrderForEdit(new NullableIdDto
            {
                Id = orderLine.OrderId == 0 ? null : orderLine.OrderId
            });

            if (input.OrderLineId == null)
            {
                if (input.DeliveryDate.HasValue)
                {
                    order.DeliveryDate = input.DeliveryDate;
                }
                if (input.Shift.HasValue)
                {
                    order.Shift = input.Shift;
                }
                if (input.OfficeId.HasValue)
                {
                    order.OfficeId = input.OfficeId.Value;
                    order.OfficeName = input.OfficeName;
                }
            }

            return new JobEditDto
            {
                OrderId = order.Id,
                OrderLineId = orderLine.Id,
                DeliveryDate = order.DeliveryDate,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                ChargeTo = order.ChargeTo,
                PONumber = order.PONumber,
                SpectrumNumber = order.SpectrumNumber,
                Directions = order.Directions,
                Priority = order.Priority,
                QuoteId = order.QuoteId,
                QuoteName = order.QuoteName,
                Shift = order.Shift,
                OfficeId = order.OfficeId,
                FocusFieldId = input.FocusFieldId,
                
                DeliverToId = orderLine.DeliverToId,
                DeliverTo = orderLine.DeliverTo,
                DeliverToNamePlain = orderLine.DeliverToNamePlain,
                LoadAtId = orderLine.LoadAtId,
                LoadAt = orderLine.LoadAt,
                LoadAtNamePlain = orderLine.LoadAtNamePlain,
                CanOverrideTotals = orderLine.CanOverrideTotals,
                IsTimeStaggeredForTrucks = orderLine.IsTimeStaggeredForTrucks,
                IsTaxable = orderLine.IsTaxable,
                JobNumber = orderLine.JobNumber,
                IsFreightPriceOverridden = orderLine.IsFreightPriceOverridden,
                Designation = orderLine.Designation,
                MaterialPrice = orderLine.MaterialPrice,
                MaterialUomId = orderLine.MaterialUomId,
                MaterialUomName = orderLine.MaterialUomName,
                MaterialQuantity = orderLine.MaterialQuantity,
                MaterialPricePerUnit = orderLine.MaterialPricePerUnit,
                IsMaterialPriceOverridden = orderLine.IsMaterialPriceOverridden,
                IsMaterialPricePerUnitOverridden = orderLine.IsMaterialPricePerUnitOverridden,
                FreightPrice = orderLine.FreightPrice,
                FreightUomId = orderLine.FreightUomId,
                FreightUomName = orderLine.FreightUomName,
                FreightQuantity = orderLine.FreightQuantity,
                FreightPricePerUnit = orderLine.FreightPricePerUnit,
                IsFreightPricePerUnitOverridden = orderLine.IsFreightPricePerUnitOverridden,
                FirstStaggeredTimeOnJob = orderLine.FirstStaggeredTimeOnJob,
                HasOpenDispatches = orderLine.HasOpenDispatches,
                HasQuoteBasedPricing = orderLine.HasQuoteBasedPricing,
                HasTickets = orderLine.HasTickets,
                IsMultipleLoads = orderLine.IsMultipleLoads,
                LeaseHaulerRate = orderLine.LeaseHaulerRate,
                Note = orderLine.Note,
                NumberOfTrucks = orderLine.NumberOfTrucks,
                ProductionPay = orderLine.ProductionPay,
                ServiceId = orderLine.ServiceId,
                ServiceName = orderLine.ServiceName,
                StaggeredTimeInterval = orderLine.StaggeredTimeInterval,
                StaggeredTimeKind = orderLine.StaggeredTimeKind,
                TimeOnJob = orderLine.TimeOnJob,
                UpdateStaggeredTime = orderLine.UpdateStaggeredTime,
                QuoteServiceId = orderLine.QuoteServiceId
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<ResetOverriddenOrderLineValuesOutput> ResetOverriddenOrderLineValues(EntityDto input)
        {
            var orderLineUpdater = _orderLineUpdaterFactory.Create(input.Id);
            var orderLine = await orderLineUpdater.GetEntityAsync();
            await orderLineUpdater.UpdateFieldAsync(o => o.IsMaterialPriceOverridden, false);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsFreightPriceOverridden, false);
            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialPrice, orderLine.MaterialPricePerUnit * orderLine.MaterialQuantity ?? 0);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightPrice, orderLine.FreightPricePerUnit * orderLine.FreightQuantity ?? 0);

            await orderLineUpdater.SaveChangesAsync();
            await CurrentUnitOfWork.SaveChangesAsync();

            var orderTaxDetails = await _orderTaxCalculator.CalculateTotalsAsync(orderLine.OrderId);

            return new ResetOverriddenOrderLineValuesOutput
            {
                MaterialTotal = orderLine.MaterialPrice,
                FreightTotal = orderLine.FreightPrice,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<EditOrderLineOutput> EditOrderLine(OrderLineEditDto model)
        {
            var orderLine = await EditOrderLineInternal(model);

            await CurrentUnitOfWork.SaveChangesAsync();
            model.Id = orderLine.Id;

            var orderTaxDetails = await _orderTaxCalculator.CalculateTotalsAsync(model.OrderId);

            return new EditOrderLineOutput
            {
                OrderLineId = orderLine.Id,
                OrderTaxDetails = new OrderTaxDetailsDto(orderTaxDetails)
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task EditOrderLines(List<OrderLineEditDto> modelList)
        {
            foreach (var model in modelList)
            {
                await EditOrderLineInternal(model);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            if (modelList.Any())
            {
                await _orderTaxCalculator.CalculateTotalsAsync(modelList.First().OrderId);
            }
        }

        private async Task<OrderLine> EditOrderLineInternal(OrderLineEditDto model)
        {
            var orderLineUpdater = _orderLineUpdaterFactory.Create(model.Id ?? 0);
            var orderLine = await orderLineUpdater.GetEntityAsync();

            if (orderLine.Id == 0)
            {
                await orderLineUpdater.UpdateFieldAsync(o => o.OrderId, model.OrderId);
            }
            var order = await orderLineUpdater.GetOrderAsync();
            var date = order.DeliveryDate ?? await GetToday();
            var timezone = await GetTimezone();

            if (model.UpdateStaggeredTime)
            {
                await orderLineUpdater.UpdateFieldAsync(o => o.StaggeredTimeKind, model.StaggeredTimeKind);
                await orderLineUpdater.UpdateFieldAsync(o => o.StaggeredTimeInterval, model.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? model.StaggeredTimeInterval : null);

                var firstStaggeredTimeOnJobUtc = model.StaggeredTimeKind == StaggeredTimeKind.SetInterval
                    ? date.AddTimeOrNull(model.FirstStaggeredTimeOnJob)?.ConvertTimeZoneFrom(timezone)
                    : (DateTime?)null;
                await orderLineUpdater.UpdateFieldAsync(o => o.FirstStaggeredTimeOnJob, firstStaggeredTimeOnJobUtc);

                if (model.StaggeredTimeKind != StaggeredTimeKind.None)
                {
                    model.TimeOnJob = null;
                    await orderLineUpdater.UpdateFieldAsync(o => o.TimeOnJob, null);
                }
            }

            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialQuantity, model.MaterialQuantity);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightQuantity, model.FreightQuantity);
            await orderLineUpdater.UpdateFieldAsync(o => o.LineNumber, model.LineNumber);
            await orderLineUpdater.UpdateFieldAsync(o => o.QuoteServiceId, model.QuoteServiceId);
            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialPricePerUnit, model.MaterialPricePerUnit);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightPricePerUnit, model.FreightPricePerUnit);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsMaterialPricePerUnitOverridden, model.IsMaterialPricePerUnitOverridden);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsFreightPricePerUnitOverridden, model.IsFreightPricePerUnitOverridden);
            await orderLineUpdater.UpdateFieldAsync(o => o.ServiceId, model.ServiceId);
            await orderLineUpdater.UpdateFieldAsync(o => o.LoadAtId, model.LoadAtId);
            await orderLineUpdater.UpdateFieldAsync(o => o.DeliverToId, model.DeliverToId);
            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialUomId, model.MaterialUomId);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightUomId, model.FreightUomId);
            await orderLineUpdater.UpdateFieldAsync(o => o.Designation, model.Designation);
            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialPrice, model.MaterialPrice);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightPrice, model.FreightPrice);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsMaterialPriceOverridden, model.IsMaterialPriceOverridden);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsFreightPriceOverridden, model.IsFreightPriceOverridden);
            await orderLineUpdater.UpdateFieldAsync(o => o.LeaseHaulerRate, model.LeaseHaulerRate);
            await orderLineUpdater.UpdateFieldAsync(o => o.JobNumber, model.JobNumber);
            await orderLineUpdater.UpdateFieldAsync(o => o.Note, model.Note);

            await orderLineUpdater.UpdateFieldAsync(o => o.TimeOnJob, date.AddTimeOrNull(model.TimeOnJob)?.ConvertTimeZoneFrom(timezone));

            await orderLineUpdater.UpdateFieldAsync(o => o.NumberOfTrucks, model.NumberOfTrucks.Round(2));
            await orderLineUpdater.UpdateFieldAsync(o => o.IsMultipleLoads, model.IsMultipleLoads);
            await orderLineUpdater.UpdateFieldAsync(o => o.ProductionPay, model.ProductionPay);

            await orderLineUpdater.SaveChangesAsync();

            return orderLine;
        }

        public async Task<decimal> GetOrderLineUtilization(int orderLineId) =>
            await _orderLineScheduledTrucksUpdater.GetOrderLineUtilization(orderLineId);

        public async Task EnsureOrderLineCanBeDeletedAsync(EntityDto input)
        {
            var record = await _orderLineRepository.GetAll()
                .Where(ol => ol.Id == input.Id)
                .Select(ol => new
                {
                    IsComplete = ol.IsComplete,
                    HasRelatedData =
                        ol.Order.IsClosed ||
                        ol.OrderLineTrucks.Any() ||
                        ol.SharedOrderLines.Any() ||
                        ol.Tickets.Any() ||
                        ol.ReceiptLines.Any() ||
                        ol.Dispatches.Any() ||
                        ol.HaulingCompanyOrderLineId != null
                })
                .SingleAsync();

            if (record.HasRelatedData)
            {
                throw new UserFriendlyException(L("Order_Delete_Error_HasRelatedData"));
            }

            if (record.IsComplete)
            {
                throw new UserFriendlyException(L("Order_Delete_Error_HasCompletedOrderLines"));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<DeleteOrderLineOutput> DeleteOrderLine(DeleteOrderLineInput input)
        {
            await EnsureOrderLineCanBeDeletedAsync(input);

            var orderLine = await _orderLineRepository.GetAsync(input.Id);

            await _orderLineRepository.DeleteAsync(orderLine);
            await CurrentUnitOfWork.SaveChangesAsync();

            var order = await _orderTaxCalculator.CalculateTotalsAsync(input.OrderId);

            var remainingOrderLines = _orderLineRepository.GetAll()
                .Where(ol => ol.Id != input.Id && ol.OrderId == input.OrderId).ToList();

            int i = 1;
            remainingOrderLines.ForEach(x => x.LineNumber = i++);

            if (orderLine.MaterialCompanyOrderLineId.HasValue
                && orderLine.MaterialCompanyTenantId.HasValue)
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                using (CurrentUnitOfWork.SetTenantId(orderLine.MaterialCompanyTenantId))
                {
                    var materialOrderLine = await _orderLineRepository.GetAsync(orderLine.MaterialCompanyOrderLineId.Value);
                    materialOrderLine.HaulingCompanyOrderLineId = null;
                    materialOrderLine.HaulingCompanyTenantId = null;
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }

            return new DeleteOrderLineOutput
            {
                OrderTaxDetails = new OrderTaxDetailsDto(order)
            };
        }

        public async Task<IOrderTaxDetails> CalculateOrderTotals(OrderTaxDetailsDto orderTaxDetails)
        {
            List<OrderLineTaxDetailsDto> orderLines;

            if (orderTaxDetails.Id != 0)
            {
                orderLines = await _orderLineRepository.GetAll()
                    .Where(x => x.OrderId == orderTaxDetails.Id)
                    .Select(x => new OrderLineTaxDetailsDto
                    {
                        FreightPrice = x.FreightPrice,
                        MaterialPrice = x.MaterialPrice,
                        IsTaxable = x.Service.IsTaxable
                    })
                    .ToListAsync();
            }
            else if (orderTaxDetails.OrderLines != null)
            {
                orderLines = orderTaxDetails.OrderLines;
            }
            else
            {
                orderLines = new List<OrderLineTaxDetailsDto>();
            }

            await _orderTaxCalculator.CalculateTotalsAsync(orderTaxDetails, orderLines);

            return orderTaxDetails;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<StaggeredTimesDto> GetStaggeredTimesForEdit(NullableIdDto input)
        {
            if (!input.Id.HasValue)
            {
                return new StaggeredTimesDto();
            }

            var model = await _orderLineRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new StaggeredTimesDto
                {
                    StaggeredTimeKind = x.StaggeredTimeKind,
                    StaggeredTimeInterval = x.StaggeredTimeInterval,
                    FirstStaggeredTimeOnJob = x.FirstStaggeredTimeOnJob == null && x.StaggeredTimeKind == StaggeredTimeKind.None ? x.TimeOnJob : x.FirstStaggeredTimeOnJob,
                }).FirstAsync();

            model.FirstStaggeredTimeOnJob = model.FirstStaggeredTimeOnJob?.ConvertTimeZoneTo(await GetTimezone());

            return model;
        }

        public async Task<StaggeredTimesDto> SetStaggeredTimes(StaggeredTimesDto model)
        {
            if (model.OrderLineId == null)
            {
                return model;
            }

            var orderLineUpdater = _orderLineUpdaterFactory.Create(model.OrderLineId.Value);
            var orderLine = await orderLineUpdater.GetEntityAsync();
            var order = await orderLineUpdater.GetOrderAsync();
            var date = order.DeliveryDate ?? await GetToday();
            var timezone = await GetTimezone();

            await orderLineUpdater.UpdateFieldAsync(o => o.StaggeredTimeKind, model.StaggeredTimeKind);
            await orderLineUpdater.UpdateFieldAsync(o => o.StaggeredTimeInterval, model.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? model.StaggeredTimeInterval : null);

            var firstStaggeredTimeOnJobUtc = model.StaggeredTimeKind == StaggeredTimeKind.SetInterval
                    ? date.AddTimeOrNull(model.FirstStaggeredTimeOnJob)?.ConvertTimeZoneFrom(timezone)
                    : (DateTime?)null;
            await orderLineUpdater.UpdateFieldAsync(o => o.FirstStaggeredTimeOnJob, firstStaggeredTimeOnJobUtc);

            if (model.StaggeredTimeKind != StaggeredTimeKind.None)
            {
                await orderLineUpdater.UpdateFieldAsync(o => o.TimeOnJob, null);
            }

            await orderLineUpdater.SaveChangesAsync();
            await CurrentUnitOfWork.SaveChangesAsync();

            return model;
        }

        private IQueryable<Receipt> GetReceiptsQuery(GetReceiptReportInput input)
        {
            return _receiptRepository.GetAll()
                .WhereIf(input.StartDate.HasValue,
                    x => x.DeliveryDate >= input.StartDate)
                .WhereIf(input.EndDate.HasValue,
                    x => x.DeliveryDate <= input.EndDate)
                .WhereIf(input.OfficeId.HasValue,
                    x => x.OfficeId == input.OfficeId)
                .WhereIf(input.CustomerId.HasValue,
                    x => x.CustomerId == input.CustomerId);
            //.Where(x => x.ReceiptLines.Any(l => l.Tickets.Any(a => a.OfficeId == OfficeId)) || x.Receipts.Any(r => r.OfficeId == OfficeId)); //&& a.ActualQuantity != null
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_Receipts)]
        public async Task<PagedResultDto<ReceiptReportDto>> GetReceipts(GetReceiptReportInput input)
        {
            var query = GetReceiptsQuery(input);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new ReceiptReportDto
                {
                    ReceiptId = x.Id,
                    OrderId = x.OrderId,
                    DeliveryDate = x.DeliveryDate,
                    CustomerName = x.Customer.Name,
                    SalesTaxRate = x.SalesTaxRate,
                    CODTotal = x.Total,
                    FreightTotal = x.FreightTotal,
                    MaterialTotal = x.MaterialTotal,
                    SalesTax = x.SalesTax,
                    IsShared = x.Order.OrderLines.Any(ol => ol.SharedOrderLines.Any(sol => sol.OfficeId != ol.Order.LocationId)) || x.Order.SharedOrders.Any(so => so.OfficeId != x.Order.LocationId),
                    Items = x.ReceiptLines.Select(l => new ReceiptReportItemDto
                    {
                        IsTaxable = l.Service.IsTaxable,
                        ActualMaterialQuantity = l.MaterialQuantity,
                        ActualFreightQuantity = l.FreightQuantity,
                        FreightPricePerUnit = l.FreightRate,
                        MaterialPricePerUnit = l.MaterialRate,
                        ReceiptLineMaterialPrice = l.MaterialAmount,
                        ReceiptLineFreightPrice = l.FreightAmount,
                        OrderLineMaterialPrice = l.OrderLine == null ? 0 : l.OrderLine.MaterialPrice,
                        OrderLineFreightPrice = l.OrderLine == null ? 0 : l.OrderLine.FreightPrice,
                        IsOrderLineMaterialPriceOverridden = l.OrderLine == null ? false : l.OrderLine.IsMaterialPriceOverridden,
                        IsOrderLineFreightPriceOverridden = l.OrderLine == null ? false : l.OrderLine.IsFreightPriceOverridden
                    }).ToList()
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            //await CalculateTotalsAsync<ReceiptsDto<ReceiptsItemDto>, ReceiptsItemDto>(items);

            return new PagedResultDto<ReceiptReportDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_Receipts)]
        public async Task<FileDto> ExportReceiptsToExcel(GetReceiptReportInput input)
        {
            var query = GetReceiptsQuery(input);

            var items = await query
                .Select(x => new ReceiptExcelReportDto
                {
                    ReceiptId = x.Id,
                    OrderId = x.OrderId,
                    DeliveryDate = x.DeliveryDate,
                    CustomerName = x.Customer.Name,
                    SalesTaxRate = x.SalesTaxRate,
                    CODTotal = x.Total,
                    FreightTotal = x.FreightTotal,
                    MaterialTotal = x.MaterialTotal,
                    SalesTax = x.SalesTax,
                    IsShared = x.Order.OrderLines.Any(ol => ol.SharedOrderLines.Any(sol => sol.OfficeId != ol.Order.LocationId)) || x.Order.SharedOrders.Any(so => so.OfficeId != x.Order.LocationId),
                    Items = x.ReceiptLines.Select(l => new ReceiptExcelReportItemDto
                    {
                        IsTaxable = l.Service.IsTaxable,
                        ActualMaterialQuantity = l.MaterialQuantity,
                        ActualFreightQuantity = l.FreightQuantity,
                        FreightPricePerUnit = l.FreightRate,
                        MaterialPricePerUnit = l.MaterialRate,
                        ReceiptLineMaterialPrice = l.MaterialAmount,
                        ReceiptLineFreightPrice = l.FreightAmount,
                        OrderLineMaterialPrice = l.OrderLine == null ? 0 : l.OrderLine.MaterialPrice,
                        OrderLineFreightPrice = l.OrderLine == null ? 0 : l.OrderLine.FreightPrice,
                        IsOrderLineMaterialPriceOverridden = l.OrderLine == null ? false : l.OrderLine.IsMaterialPriceOverridden,
                        IsOrderLineFreightPriceOverridden = l.OrderLine == null ? false : l.OrderLine.IsFreightPriceOverridden,
                        Name = l.Service.Service1,
                        Designation = l.Designation,
                        LoadAt = l.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = l.LoadAt.Name,
                            StreetAddress = l.LoadAt.StreetAddress,
                            City = l.LoadAt.City,
                            State = l.LoadAt.State
                        },
                        DeliverTo = l.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = l.DeliverTo.Name,
                            StreetAddress = l.DeliverTo.StreetAddress,
                            City = l.DeliverTo.City,
                            State = l.DeliverTo.State
                        },
                        MaterialUomName = l.MaterialUom.Name,
                        FreightUomName = l.FreightUom.Name
                    }).ToList()
                })
                .OrderBy(input.Sorting)
                //.PageBy(input)
                .ToListAsync();

            if (items.Count == 0)
            {
                throw new UserFriendlyException("There is no data to export.");
            }

            //await CalculateTotalsAsync<ReceiptsDto<ReceiptsReportItemDto>, ReceiptsReportItemDto>(items);

            return await _receiptsExcelExporter.ExportToFileAsync(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task SetOrderIsBilled(SetOrderIsBilledInput input)
        {
            var existingBillingRecords = await _billedOrderRepository.GetAll()
                .Where(x => x.OrderId == input.OrderId && x.OfficeId == OfficeId)
                .ToListAsync();
            //only one record is expected, but we'll want to delete both records in case a duplicate occurs somehow

            if (input.IsBilled)
            {
                if (!existingBillingRecords.Any())
                {
                    _billedOrderRepository.Insert(new BilledOrder
                    {
                        OrderId = input.OrderId,
                        OfficeId = OfficeId
                    });
                }
            }
            else
            {
                if (existingBillingRecords.Any())
                {
                    existingBillingRecords.ForEach(async x => await _billedOrderRepository.DeleteAsync(x));
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<Document> GetWorkOrderReport(GetWorkOrderReportInput input)
        {
            if (input.Id == null && input.Date == null && input.Ids?.Any() != true)
            {
                throw new ArgumentNullException(nameof(input.Id), "At least one of (Id, Date) should be set");
            }

            var logoPath = await _binaryObjectManager.GetLogoAsBase64StringAsync(await GetCurrentTenantAsync());
            var paidImagePath = Path.Combine(_hostingEnvironment.WebRootPath, "Common/Images/Paid.png");
            var staggeredTimeImagePath = Path.Combine(_hostingEnvironment.WebRootPath, "Common/Images/far-clock.png");
            var timeZone = await GetTimezone();
            var showDriverNamesOnPrintedOrder = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ShowDriverNamesOnPrintedOrder);
            var spectrumNumberLabel = await SettingManager.GetSettingValueAsync(AppSettings.General.UserDefinedField1);
            var showSignatureColumn = (DispatchVia)await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DispatchVia) == DispatchVia.DriverApplication
                    && !await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.HideTicketControlsInDriverApp);
            var shiftDictionary = await SettingManager.GetShiftDictionary();
            var currentCulture = await SettingManager.GetCurrencyCultureAsync();
            input.Date = input.Date?.Date;

            var data = await GetWorkOrderReportQuery(input).ToListAsync();

            data.ForEach(x =>
            {
                //TODO remove items with actual quantity of 0 when UseActualAmount is true
                //if (x.Items.Count > 1)
                //    // ReSharper disable once CompareOfFloatsByEqualityOperator
                //    x.Items.RemoveAll(s =>
                //        (s.MaterialQuantity ?? 0) == 0 && (s.FreightQuantity ?? 0) == 0 && s.NumberOfTrucks == 0 &&
                //        (!input.UseActualAmount || (s.ActualQuantity ?? 0) == 0)
                //    );
                x.LogoPath = logoPath;
                x.PaidImagePath = paidImagePath;
                x.StaggeredTimeImagePath = staggeredTimeImagePath;
                x.HidePrices = input.HidePrices;
                x.SplitRateColumn = input.SplitRateColumn;
                x.ShowPaymentStatus = input.ShowPaymentStatus;
                x.ShowSpectrumNumber = input.ShowSpectrumNumber && !spectrumNumberLabel.IsNullOrEmpty();
                x.SpectrumNumberLabel = spectrumNumberLabel;
                x.ShowOfficeName = input.ShowOfficeName;
                x.UseActualAmount = input.UseActualAmount;
                x.UseReceipts = input.UseReceipts;
                x.ShowDeliveryInfo = input.ShowDeliveryInfo;
                x.IncludeTickets = input.IncludeTickets;
                x.TimeZone = timeZone;
                x.ShowDriverNamesOnPrintedOrder = showDriverNamesOnPrintedOrder;
                x.OrderShiftName = x.OrderShift.HasValue && shiftDictionary.ContainsKey(x.OrderShift.Value) ? shiftDictionary[x.OrderShift.Value] : "";
                x.ShowSignatureColumn = showSignatureColumn;
                x.CurrencyCulture = currentCulture;
            });

            if (input.ShowDeliveryInfo)
            {
                foreach (var load in data
                    .Where(x => x.DeliveryInfoItems != null)
                    .SelectMany(x => x.DeliveryInfoItems)
                    .Where(x => x.Load != null)
                    .Select(x => x.Load))
                {
                    if (load.SignatureId.HasValue && load.Signature == null)
                    {
                        load.Signature = await _binaryObjectManager.GetImageAsBase64StringAsync(load.SignatureId.Value);
                    }
                }
            }

            if (input.IncludeTickets)
            {
                foreach (var ticket in data
                    .Where(x => x.DeliveryInfoItems != null)
                    .SelectMany(x => x.DeliveryInfoItems))
                {
                    if (ticket.TicketPhotoId.HasValue)
                    {
                        ticket.TicketPhoto = await _binaryObjectManager.GetImageAsBase64StringAsync(ticket.TicketPhotoId.Value);
                    }
                }
            }

            var workOrderReportGenerator = new WorkOrderReportGenerator(_orderTaxCalculator);

            return await workOrderReportGenerator.GenerateReport(data);
        }

        [AbpAuthorize(AppPermissions.Pages_PrintOrders)]
        public async Task<bool> DoesWorkOrderReportHaveData(GetWorkOrderReportInput input)
        {
            return await GetWorkOrderReportQuery(input).AnyAsync();
        }

        private IQueryable<WorkOrderReportDto> GetWorkOrderReportQuery(GetWorkOrderReportInput input)
        {
            if (input.UseReceipts)
            {
                return _receiptRepository.GetAll()
                    .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                    .WhereIf(input.Ids?.Any() == true, x => input.Ids.Contains(x.Id))
                    .WhereIf(input.Date.HasValue, x => x.Order.DeliveryDate == input.Date)
                    //.Where(x => x.OfficeId == OfficeId)
                    .WhereIf(input.TruckId.HasValue, x => x.Order.OrderLines.Any(ol => ol.OrderLineTrucks.Any(olt => olt.TruckId == input.TruckId)))
                    .GetWorkOrderReportDtoQuery(input, OfficeId);
            }
            else
            {
                return _orderRepository.GetAll()
                    .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                    .WhereIf(input.Ids?.Any() == true, x => input.Ids.Contains(x.Id))
                    .WhereIf(input.Date.HasValue, x => x.DeliveryDate == input.Date && (x.LocationId == OfficeId || x.SharedOrders.Any(s => s.OfficeId == OfficeId) || x.OrderLines.Any(ol => ol.SharedOrderLines.Any(s => s.OfficeId == OfficeId))))
                    .WhereIf(input.TruckId.HasValue, x => x.OrderLines.Any(ol => ol.OrderLineTrucks.Any(olt => olt.TruckId == input.TruckId)))
                    .GetWorkOrderReportDtoQuery(input, OfficeId);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<byte[]> GetOrderSummaryReport(GetOrderSummaryReportInput input)
        {
            input.Date = input.Date.Date;

            var items = await GetOrderSummaryReportQuery(input).GetOrderSummaryReportItems(await SettingManager.GetShiftDictionary(), _orderTaxCalculator);

            var data = new OrderSummaryReportDto
            {
                Date = input.Date,
                ShowLoadColumns = input.ShowLoadColumns,
                HidePrices = input.HidePrices,
                Items = items,
                UseShifts = await SettingManager.UseShifts(),
                CurrencyCulture = await SettingManager.GetCurrencyCultureAsync()
            };

            return OrderSummaryReportGenerator.GenerateReport(data);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<bool> DoesOrderSummaryReportHaveData(GetOrderSummaryReportInput input)
        {
            return await GetOrderSummaryReportQuery(input).AnyAsync();
        }

        private IQueryable<OrderLine> GetOrderSummaryReportQuery(GetOrderSummaryReportInput input)
        {
            return _orderLineRepository.GetAll()
                .Where(x =>
                    x.Order.DeliveryDate == input.Date &&
                    !x.Order.IsPending &&
                    (x.Order.LocationId == OfficeId || x.SharedOrderLines.Any(s => s.OfficeId == OfficeId))
                    && (x.MaterialQuantity > 0 || x.FreightQuantity > 0 || x.NumberOfTrucks > 0)
                );
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_PaymentReconciliation)]
        public async Task<byte[]> GetPaymentReconciliationReport(GetPaymentReconciliationReportInput input)
        {
            var timeZone = await GetTimezone();
            var currencyCulture = await SettingManager.GetCurrencyCultureAsync();
            var startDateFilter = input.StartDate.ConvertTimeZoneFrom(timeZone);
            var endDateFilter = input.EndDate.ConvertTimeZoneFrom(timeZone);
            var heartlandPublicKeyId = await _paymentAppService.GetHeartlandPublicKeyIdAsync();

            try
            {
                await _paymentAppService.UpdatePaymentsFromHeartland(new UpdatePaymentsFromHeartlandInput
                {
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    AllOffices = input.AllOffices
                });
            }
            catch (Exception e)
            {
                Logger.Error("Exception on UpdatePyamentsFromHeartland", e);
                throw new UserFriendlyException("Error", "Unable to receive transactions from Heartland. Please try again later.", e);
            }

            var items = await _paymentRepository.GetAll()
                .WhereIf(!input.AllOffices,
                    x => x.PaymentHeartlandKeyId == heartlandPublicKeyId
                        //|| x.OrderPayments.Any(o => o.Order.LocationId == OfficeId || o.Order.SharedOrders.Any(s => s.OfficeId == OfficeId)))
                        || x.OrderPayments.Any(o => o.OfficeId == OfficeId))
                .Where(x => x.AuthorizationCaptureDateTime >= startDateFilter && x.AuthorizationCaptureDateTime < endDateFilter.AddDays(1))
                .Where(x => !x.IsCancelledOrRefunded)
                .Select(x => new PaymentReconciliationReportItemDto
                {
                    PaymentId = x.Id,
                    CaptureAmount = x.AuthorizationCaptureAmount,
                    TransactionId = x.AuthorizationCaptureTransactionId,
                    TransactionDate = x.AuthorizationCaptureDateTime,
                    TransactionType = x.TransactionType,
                    CardLast4 = x.CardLast4,
                    CardType = x.CardType,
                    AuthorizationAmount = x.AuthorizationAmount,
                    BatchSummaryId = x.BatchSummaryId
                }).ToListAsync();

            var paymentIds = items.Select(x => x.PaymentId).ToList();

            var orderDetails = await _orderPaymentRepository.GetAll()
                .Where(x => paymentIds.Contains(x.PaymentId))
                .Select(x => new
                {
                    PaymentId = x.PaymentId,
                    OrderId = x.Order.Id,
                    OfficeId = x.Order.LocationId,
                    OfficeName = x.Order.Office.Name,
                    CustomerName = x.Order.Customer.Name,
                    DeliveryDate = x.Order.DeliveryDate,
                })
                .ToListAsync();

            foreach (var order in orderDetails)
            {
                var payment = items.First(x => x.PaymentId == order.PaymentId);
                payment.PaymentId = order.PaymentId;
                payment.OrderId = order.OrderId;
                payment.OfficeId = order.OfficeId;
                payment.OfficeName = order.OfficeName;
                payment.CustomerName = order.CustomerName;
                payment.DeliveryDate = order.DeliveryDate;
            }

            items.ForEach(x =>
            {
                x.TimeZone = timeZone;
                x.CurrencyCulture = currencyCulture;
            });

            return PaymentReconciliationReportGenerator.GenerateReport(new PaymentReconciliationReportDto
            {
                Items = items,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                AllOffices = input.AllOffices,
                OfficeName = Session.OfficeName
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<EmailOrderReportDto> GetEmailReceiptReportModel(EntityDto input)
        {
            var user = await _userRepository.GetAll()
                .Where(x => x.Id == Session.UserId)
                .Select(x => new
                {
                    Email = x.EmailAddress,
                    FirstName = x.Name,
                    LastName = x.Surname,
                    PhoneNumber = x.PhoneNumber
                })
                .FirstAsync();

            var receipt = await _receiptRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.Order.DeliveryDate,
                    x.ReceiptDate,
                    x.Shift,
                    ContactEmail = x.Order.CustomerContact.Email
                })
                .FirstOrDefaultAsync();

            if (receipt == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);

            var subject = await SettingManager.GetSettingValueAsync(AppSettings.Receipt.EmailSubjectTemplate);

            var body = (await SettingManager.GetSettingValueAsync(AppSettings.Receipt.EmailBodyTemplate))
                    .ReplaceTokensInTemplate(new TemplateTokenDto()
                    {
                        DeliveryDate = receipt.DeliveryDate?.ToShortDateString() ?? "",
                        Shift = await SettingManager.GetShiftName(receipt.Shift) ?? "",
                        CompanyName = companyName,
                    })
                    .Replace("{ReceiptDate}", receipt.ReceiptDate.ToShortDateString())
                    .Replace("{Order.DateTime}", receipt.DeliveryDate?.ToShortDateString() ?? "") // Support both the new {DeliveryDate} and old {Order.DateTime}
                    ;

            return new EmailOrderReportDto
            {
                Id = input.Id,
                UseReceipts = true,
                From = user.Email,
                To = receipt.ContactEmail,
                CC = user.Email,
                Subject = subject,
                Body = body
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<EmailOrderReportDto> GetEmailOrderReportModel(EntityDto input)
        {
            var user = await _userRepository.GetAll()
                .Where(x => x.Id == Session.UserId)
                .Select(x => new
                {
                    Email = x.EmailAddress,
                    FirstName = x.Name,
                    LastName = x.Surname,
                    PhoneNumber = x.PhoneNumber
                })
                .FirstAsync();

            var order = await _orderRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.DeliveryDate,
                    x.Shift,
                    ContactEmail = x.CustomerContact.Email,
                    x.IsPending,
                    ProjectName = x.Project.Name
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);

            var subject = order.IsPending
                ? QuoteAppService.ReplaceEmailSubjectTemplateTokens(await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailSubjectTemplate), order.ProjectName)
                : await SettingManager.GetSettingValueAsync(AppSettings.Order.EmailSubjectTemplate);

            var body = order.IsPending
                ? QuoteAppService.ReplaceEmailBodyTemplateTokens(await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailBodyTemplate), user.FirstName, user.LastName, user.PhoneNumber, companyName)
                : (await SettingManager.GetSettingValueAsync(AppSettings.Order.EmailBodyTemplate))
                    .ReplaceTokensInTemplate(new TemplateTokenDto()
                    {
                        DeliveryDate = order.DeliveryDate?.ToShortDateString() ?? "",
                        Shift = await SettingManager.GetShiftName(order.Shift) ?? "",
                        CompanyName = companyName,
                    })
                    .Replace("{Order.DateTime}", order.DeliveryDate?.ToShortDateString() ?? "") // Support both the new {DeliveryDate} and old {Order.DateTime}
                    ;

            return new EmailOrderReportDto
            {
                Id = input.Id,
                From = user.Email,
                To = order.ContactEmail,
                CC = user.Email,
                Subject = subject,
                Body = body
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task EmailOrderReport(EmailOrderReportDto input)
        {
            var report = input.UseReceipts
                ? await GetWorkOrderReport(new GetWorkOrderReportInput
                {
                    Id = input.Id,
                    UseReceipts = true,
                    ShowPaymentStatus = true,
                    ShowSpectrumNumber = true,
                    ShowOfficeName = true
                })
                : await GetWorkOrderReport(new GetWorkOrderReportInput
                {
                    Id = input.Id,
                    ShowPaymentStatus = true
                });
            var message = new MailMessage
            {
                From = new MailAddress(input.From),
                Subject = input.Subject,
                Body = input.Body,
                IsBodyHtml = false
            };
            foreach (var to in EmailHelper.SplitEmailAddresses(input.To))
            {
                message.To.Add(to);
            }
            foreach (var cc in EmailHelper.SplitEmailAddresses(input.CC))
            {
                message.CC.Add(cc);
            }
            string filename;
            if (input.UseReceipts)
            {
                filename = "Receipt";
            }
            else
            {
                var orderDetails = await _orderRepository.GetAll()
                    .Where(x => x.Id == input.Id)
                    .Select(x => new
                    {
                        ProjectName = x.Project.Name,
                        x.IsPending
                    })
                    .FirstAsync();

                filename = !orderDetails.IsPending
                    ? "Order"
                    : string.IsNullOrEmpty(orderDetails.ProjectName)
                        ? "Quote"
                        : QuoteAppService.ReplaceEmailSubjectTemplateTokens(await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailSubjectTemplate), orderDetails.ProjectName);
            }
            filename = Utilities.RemoveInvalidFileNameChars(filename);
            filename += ".pdf";

            using (var stream = new MemoryStream())
            {
                report.SaveToMemoryStream(stream);
                message.Attachments.Add(new Attachment(stream, filename));

                var trackableEmailId = await _trackableEmailSender.SendTrackableAsync(message);
                if (!input.UseReceipts)
                {
                    var order = await _orderRepository.GetAsync(input.Id);
                    order.LastQuoteEmailId = trackableEmailId;
                    await _orderEmailRepository.InsertAsync(new OrderEmail
                    {
                        EmailId = trackableEmailId,
                        OrderId = order.Id
                    });
                }
            }
        }

        private async Task DecrementOrderLineNumbers(int orderId, int aboveLineNumber)
        {
            var orderLines = await _orderLineRepository.GetAll()
                       .Where(x => x.OrderId == orderId && x.LineNumber > aboveLineNumber)
                       .ToListAsync();

            foreach (var item in orderLines)
            {
                item.LineNumber -= 1;
            }
        }
    }
}
