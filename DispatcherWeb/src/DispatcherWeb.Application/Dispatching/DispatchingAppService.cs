using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.Timing.Timezone;
using Abp.UI;
using Castle.Core.Internal;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dispatching.Exporting;
using DispatcherWeb.Dispatching.Reports;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.DriverMessages.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Storage;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitsOfMeasure;
using DispatcherWeb.Url;
using DispatcherWeb.WebPush;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MigraDoc.DocumentObjectModel;
using Twilio.Exceptions;

namespace DispatcherWeb.Dispatching
{
    public class DispatchingAppService : DispatcherWebAppServiceBase, IDispatchingAppService
    {
        public const int MaxNumberOfDispatches = 99;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Load> _loadRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<UnitOfMeasure> _unitOfMeasureRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRepository<DriverApplicationLog> _driverApplicationLogRepository;
        private readonly IRepository<DeferredBinaryObject, Guid> _deferredBinaryObjectRepository;
        private readonly ISmsSender _smsSender;
        private readonly IAppNotifier _appNotifier;
        private readonly IWebUrlService _webUrlService;
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IDispatchListCsvExporter _dispatchListCsvExporter;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IWebPushSender _webPushSender;
        private readonly IDriverApplicationPushSender _driverApplicationPushSender;
        private readonly IDriverApplicationLogger _driverApplicationLogger;
        private readonly IDriverApplicationAuthProvider _driverApplicationAuthProvider;
        private readonly IPushSubscriptionManager _pushSubscriptionManager;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IDispatchSender _dispatchSender;
        private readonly IFuelSurchargeCalculator _fuelSurchargeCalculator;

        public DispatchingAppService(
            IRepository<Truck> truckRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Load> loadRepository,
            IRepository<Driver> driverRepository,
            IRepository<UnitOfMeasure> unitOfMeasureRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<Office> officeRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IRepository<DriverApplicationLog> driverApplicationLogRepository,
            IRepository<DeferredBinaryObject, Guid> deferredBinaryObjectRepository,
            ISmsSender smsSender,
            IAppNotifier appNotifier,
            IWebUrlService webUrlService,
            ITimeZoneConverter timeZoneConverter,
            IDispatchListCsvExporter dispatchListCsvExporter,
            IBinaryObjectManager binaryObjectManager,
            OrderTaxCalculator orderTaxCalculator,
            IWebPushSender webPushSender,
            IDriverApplicationPushSender driverApplicationPushSender,
            IDriverApplicationLogger driverApplicationLogger,
            IDriverApplicationAuthProvider driverApplicationAuthProvider,
            IPushSubscriptionManager pushSubscriptionManager,
            ISyncRequestSender syncRequestSender,
            IDispatchSender dispatchSender,
            IFuelSurchargeCalculator fuelSurchargeCalculator
        )
        {
            _truckRepository = truckRepository;
            _orderLineRepository = orderLineRepository;
            _dispatchRepository = dispatchRepository;
            _loadRepository = loadRepository;
            _driverRepository = driverRepository;
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _ticketRepository = ticketRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _officeRepository = officeRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _driverApplicationLogRepository = driverApplicationLogRepository;
            _deferredBinaryObjectRepository = deferredBinaryObjectRepository;
            _smsSender = smsSender;
            _appNotifier = appNotifier;
            _webUrlService = webUrlService;
            _timeZoneConverter = timeZoneConverter;
            _dispatchListCsvExporter = dispatchListCsvExporter;
            _binaryObjectManager = binaryObjectManager;
            _orderTaxCalculator = orderTaxCalculator;
            _webPushSender = webPushSender;
            _driverApplicationPushSender = driverApplicationPushSender;
            _driverApplicationLogger = driverApplicationLogger;
            _driverApplicationAuthProvider = driverApplicationAuthProvider;
            _pushSubscriptionManager = pushSubscriptionManager;
            _syncRequestSender = syncRequestSender;
            _dispatchSender = dispatchSender;
            _fuelSurchargeCalculator = fuelSurchargeCalculator;
        }

        public async Task<string> TestSignalR()
        {
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.EmployeeTime, new ChangedEmployeeTime
                {
                    Id = 1,
                    UserId = 2,
                    TruckId = 3,
                    Guid = Guid.NewGuid(),
                    DriverId = null
                })
            );
            return "ok";
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        [HttpPost]
        public async Task<PagedResultDto<DispatchListDto>> GetDispatchPagedList(GetDispatchPagedListInput input)
        {
            var query = GetFilteredDispatchQuery(input);

            var totalCount = await query.CountAsync();

            var rawItems = await query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            var items = GetDispatchDtoList(rawItems);

            return new PagedResultDto<DispatchListDto>(totalCount, items);
        }

        private IQueryable<RawDispatchDto> GetFilteredDispatchQuery(IGetDispatchListFilter input)
        {
            var filteredQuery = _dispatchRepository.GetAll()
                    .Where(d => d.Status != DispatchStatus.Canceled || d.Loads.Any())
                    .WhereIf(input.OfficeId.HasValue, d => d.OrderLine.Order.LocationId == input.OfficeId.Value)
                    .WhereIf(input.DateBegin.HasValue, d => d.OrderLine.Order.DeliveryDate >= input.DateBegin.Value)
                    .WhereIf(input.DateEnd.HasValue, d => d.OrderLine.Order.DeliveryDate < input.DateEnd.Value.AddDays(1))
                    .WhereIf(!input.TruckIds.IsNullOrEmpty(), d => input.TruckIds.Contains(d.TruckId))
                    .WhereIf(!input.DriverIds.IsNullOrEmpty(), d => input.DriverIds.Contains(d.DriverId))
                    .WhereIf(!input.Statuses.IsNullOrEmpty(), d => input.Statuses.Contains(d.Status))
                    .WhereIf(input.CustomerId.HasValue, d => d.OrderLine.Order.CustomerId == input.CustomerId.Value)
                    .WhereIf(input.OrderLineId.HasValue, d => d.OrderLineId == input.OrderLineId);

            var query = filteredQuery.ToRawDispatchDto();

            return query
                .WhereIf(input.MissingTickets, d => d.Status == DispatchStatus.Completed && (d.Quantity == null || d.Quantity == 0));

        }

        [RemoteService(IsEnabled = false)]
        public static IQueryable<RawDispatchDto> ToRawDispatchDto(IQueryable<Dispatch> query)
        {
            return
                from d in query
                from l in d.Loads.DefaultIfEmpty()
                from t in l.Tickets.DefaultIfEmpty()
                select new RawDispatchDto
                {
                    Id = d.Id,
                    DriverId = d.DriverId,
                    TruckCode = d.Truck.TruckCode,
                    DriverLastFirstName = d.Driver.LastName + ", " + d.Driver.FirstName,
                    Sent = d.Sent,
                    Acknowledged = d.Acknowledged,
                    Loaded = l.SourceDateTime,
                    Delivered = l.DestinationDateTime,
                    Status = d.Status,
                    CustomerName = d.OrderLine.Order.Customer.Name,
                    QuoteName = d.OrderLine.Order.Quote.Name,
                    JobNumber = d.OrderLine.JobNumber,
                    LoadAtNamePlain = d.OrderLine.LoadAt.Name + d.OrderLine.LoadAt.StreetAddress + d.OrderLine.LoadAt.City + d.OrderLine.LoadAt.State, //for sorting
                    LoadAt = d.OrderLine.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = d.OrderLine.LoadAt.Name,
                        StreetAddress = d.OrderLine.LoadAt.StreetAddress,
                        City = d.OrderLine.LoadAt.City,
                        State = d.OrderLine.LoadAt.State
                    },
                    DeliverToNamePlain = d.OrderLine.DeliverTo.Name + d.OrderLine.DeliverTo.StreetAddress + d.OrderLine.DeliverTo.City + d.OrderLine.DeliverTo.State, //for sorting
                    DeliverTo = d.OrderLine.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = d.OrderLine.DeliverTo.Name,
                        StreetAddress = d.OrderLine.DeliverTo.StreetAddress,
                        City = d.OrderLine.DeliverTo.City,
                        State = d.OrderLine.DeliverTo.State
                    },
                    Item = d.OrderLine.Service.Service1,
                    Quantity = t != null ? t.Quantity : (decimal?)null,
                    Uom = t != null ? t.UnitOfMeasure.Name : null,
                    Guid = d.Guid,
                    IsMultipleLoads = d.IsMultipleLoads,
                };
        }

        private List<DispatchListDto> GetDispatchDtoList(List<RawDispatchDto> rawItems)
        {
            return rawItems.Select(d => new DispatchListDto
            {
                Id = d.Id,
                TruckCode = d.TruckCode,
                DriverLastFirstName = d.DriverLastFirstName,
                Sent = d.Sent,
                Acknowledged = d.Acknowledged,
                Loaded = d.Loaded,
                Delivered = d.Delivered,
                Status = d.Status.GetDisplayName(),
                DispatchStatus = d.Status,
                CustomerName = d.CustomerName,
                QuoteName = d.QuoteName,
                JobNumber = d.JobNumber,
                LoadAt = d.LoadAt,
                LoadAtNamePlain = d.LoadAtNamePlain,
                DeliverTo = d.DeliverTo,
                DeliverToNamePlain = d.DeliverToNamePlain,
                Item = d.Item,
                Quantity = d.Quantity,
                Uom = d.Uom,
                Cancelable = d.Status != DispatchStatus.Completed && d.Status != DispatchStatus.Canceled,
                Guid = d.Guid,
                ShortGuid = d.Guid.ToShortGuid(),
                IsMultipleLoads = d.IsMultipleLoads,
            }).ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        [HttpPost]
        public async Task<FileDto> GetDispatchesToCsv(GetDispatchesToCsvInput input)
        {
            var query = GetFilteredDispatchQuery(input);

            var rawItems = await query
                .OrderBy(input.Sorting)
                .ToListAsync();
            var items = GetDispatchDtoList(rawItems);
            if (items.Count == 0)
            {
                throw new UserFriendlyException("There is no data to export!");
            }
            Debug.Assert(AbpSession.UserId != null, "AbpSession.UserId != null");
            items.ForEach(item =>
            {
                item.Sent = _timeZoneConverter.Convert(item.Sent, AbpSession.TenantId, AbpSession.UserId.Value);
                item.Acknowledged = _timeZoneConverter.Convert(item.Acknowledged, AbpSession.TenantId, AbpSession.UserId.Value);
                item.Loaded = _timeZoneConverter.Convert(item.Loaded, AbpSession.TenantId, AbpSession.UserId.Value);
                item.Delivered = _timeZoneConverter.Convert(item.Delivered, AbpSession.TenantId, AbpSession.UserId.Value);
            });

            return _dispatchListCsvExporter.ExportToFile(items);
        }

        public async Task<bool> CanAddDispatchBasedOnTime(CanAddDispatchBasedOnTimeInput input)
        {
            return await _dispatchSender.CanAddDispatchBasedOnTime(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task<SendDispatchMessageDto> CreateSendDispatchMessageDto(int orderLineId, bool firstDispatchForDay = false)
        {
            return await _dispatchSender.CreateSendDispatchMessageDto(orderLineId, firstDispatchForDay);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        [AbpAuthorize(AppPermissions.Pages_SendOrdersToDrivers)]
        public async Task SendOrdersToDrivers(SendOrdersToDriversInput input)
        {
            await _dispatchSender.SendOrdersToDrivers(input);
        }

        private async Task<bool> ShouldSendOrdersToDriversImmediately()
        {
            var dispatchVia = (DispatchVia)await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DispatchVia);
            return dispatchVia == DispatchVia.None || dispatchVia == DispatchVia.SimplifiedSms;
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task<bool> GetDispatchTruckStatus(int dispatchId)
        {
            var truckStatus = await _dispatchRepository.GetAll()
                .Where(d => d.Id == dispatchId)
                .Select(d => new
                {
                    d.Truck.IsOutOfService,
                    DriverAssignment = d.Truck.DriverAssignments.Where(da => da.Date == d.OrderLine.Order.DeliveryDate).FirstOrDefault(),
                })
                .FirstAsync();
            return !truckStatus.IsOutOfService &&
                   (truckStatus.DriverAssignment == null || truckStatus.DriverAssignment.DriverId.HasValue);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task DuplicateDispatch(DuplicateDispatchInput input)
        {
            DispatchSender.ValidateNumberOfDispatches(input.NumberOfDispatches);

            var dispatch = await _dispatchRepository.GetAll()
                .Where(d => d.Id == input.DispatchId)
                .Select(d => new
                {
                    d.TruckId,
                    d.DriverId,
                    d.OrderLineId,
                    d.OrderLineTruckId,
                    d.PhoneNumber,
                    d.EmailAddress,
                    d.Message,
                    MultipleLoads = d.IsMultipleLoads,
                    d.OrderNotifyPreferredFormat,
                    d.TimeOnJob
                })
                .FirstAsync();

            await _dispatchSender.EnsureCanCreateDispatchAsync(dispatch.OrderLineId, 1, input.NumberOfDispatches, dispatch.MultipleLoads);

            var oldActiveDispatch = await GetFirstOpenDispatch(dispatch.DriverId);

            var affectedDispatches = new List<Dispatch>();

            for (int i = 0; i < input.NumberOfDispatches; i++)
            {
                var acknowledgementGuid = Guid.NewGuid();
                var dispatchMessage = ReplaceAcknowledgementUrl(dispatch.Message, acknowledgementGuid);

                var affectedDispatch = _dispatchSender.AddDispatch(new Dto.DispatchSender.DispatchEditDto
                {
                    TruckId = dispatch.TruckId,
                    DriverId = dispatch.DriverId,
                    OrderLineId = dispatch.OrderLineId,
                    OrderLineTruckId = dispatch.OrderLineTruckId,
                    PhoneNumber = dispatch.PhoneNumber,
                    EmailAddress = dispatch.EmailAddress,
                    OrderNotifyPreferredFormat = dispatch.OrderNotifyPreferredFormat,
                    Message = dispatchMessage,
                    IsMultipleLoads = dispatch.MultipleLoads,
                    WasMultipleLoads = dispatch.MultipleLoads,
                    Guid = acknowledgementGuid,
                    Status = DispatchStatus.Created,
                    TimeOnJob = dispatch.TimeOnJob
                });
                affectedDispatches.Add(affectedDispatch);
            }
            await _dispatchSender.CleanUp();

            var newActiveDispatch = await GetFirstOpenDispatch(dispatch.DriverId);

            await CurrentUnitOfWork.SaveChangesAsync();
            await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatch.DriverId)
            {
                LogMessage = $"Duplicated dispatch {input.DispatchId}, created dispatche(s) {string.Join(", ", affectedDispatches.Select(x => x.Id))}"
            });
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch, affectedDispatches.Select(x => x.ToChangedEntity()))
                    .AddLogMessage($"Duplicated dispatch {input.DispatchId}"));

            await _dispatchSender.SendSmsOrEmail(new SendSmsOrEmailInput
            {
                TruckId = dispatch.TruckId,
                DriverId = dispatch.DriverId,
                PhoneNumber = dispatch.PhoneNumber,
                EmailAddress = dispatch.EmailAddress,
                OrderNotifyPreferredFormat = dispatch.OrderNotifyPreferredFormat,
                ActiveDispatchWasChanged = oldActiveDispatch?.Id != newActiveDispatch?.Id
            });
        }

        private string ReplaceAcknowledgementUrl(string message, Guid acknowledgementGuid)
        {
            if (message.IsNullOrEmpty())
            {
                return message;
            }
            string newAcknowledgementUrl = GetAcknowledgementUrl(acknowledgementGuid);
            int shortGuidLength = acknowledgementGuid.ToShortGuid().Length;
            string urlWithoutGuid = newAcknowledgementUrl.Substring(0, newAcknowledgementUrl.Length - shortGuidLength);
            if (message.Contains(urlWithoutGuid))
            {
                var urlIndex = message.IndexOf(urlWithoutGuid, StringComparison.Ordinal);
                return string.Concat(message.AsSpan(0, urlIndex), newAcknowledgementUrl, message.AsSpan(urlIndex + newAcknowledgementUrl.Length));
            }
            return message;
        }

        public async Task SendDispatchMessageNonInteractive(SendDispatchMessageNonInteractiveInput input)
        {
            var dto = await CreateSendDispatchMessageDto(input.OrderLineId);
            await SendDispatchMessage(new SendDispatchMessageInput
            {
                OrderLineId = dto.OrderLineId,
                Message = dto.Message,
                OrderLineTruckIds = dto.OrderLineTrucks
                    .Where(x => input.SelectedOrderLineTruckId == null || x.OrderLineTruckId == input.SelectedOrderLineTruckId)
                    .Select(x => x.OrderLineTruckId)
                    .ToArray(),
                NumberOfDispatches = 1,
                IsMultipleLoads = false,
                AddDispatchBasedOnTime = false,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task SendDispatchMessage(SendDispatchMessageInput input)
        {
            await _dispatchSender.SendDispatchMessage(input);
        }

        public async Task ReorderDispatches(ReorderDispatchesInput input)
        {
            var dispatches = await _dispatchRepository.GetAll()
                .Where(x => input.OrderedDispatchIds.Contains(x.Id))
                .ToListAsync();

            if (dispatches.Count != input.OrderedDispatchIds.Count)
            {
                throw new ApplicationException("At least one of the dispatches weren't found to ReorderDispatches");
            }

            var sortOrders = new Queue<int>(dispatches.Select(x => x.SortOrder).OrderBy(x => x));
            foreach (var dispatchId in input.OrderedDispatchIds)
            {
                dispatches.First(d => d.Id == dispatchId).SortOrder = sortOrders.Dequeue();
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            foreach (var dispatchGroup in dispatches.GroupBy(d => d.DriverId))
            {
                await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchGroup.Key)
                {
                    LogMessage = $"Reordered dispatch(es) {string.Join(", ", dispatchGroup.OrderBy(x => x.SortOrder).Select(x => x.Id))}"
                });
            }
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch, dispatches.Select(x => x.ToChangedEntity()))
                    //.SetIgnoreForCurrentUser(true)
                    .AddLogMessage("Reordered dispatch(es)"));
        }

        [RemoteService(IsEnabled = false)]
        public async Task<GetNextDispatchTodayResult> GetNextDispatchToday(Guid acknowledgedDispatchGuid)
        {
            CurrentDispatchDto dispatch;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                dispatch = await _dispatchRepository.GetAll()
                    .Where(d => d.Guid == acknowledgedDispatchGuid)
                    .Select(d => new CurrentDispatchDto
                    {
                        TenantId = d.TenantId,
                        TruckId = d.TruckId,
                        DriverId = d.DriverId,
                        OrderDate = d.OrderLine.Order.DeliveryDate,
                        Status = d.Status,
                        IsDeleted = d.IsDeleted,
                    })
                    .FirstOrDefaultAsync();
            }

            ThrowArgumentExceptionIfDispatchIsNull();
            ThrowArgumentExceptionIfDispatchIsNotDeletedAndStatusIsNotCompletedOrCanceled();
            ThrowArgumentExceptionIfDispatchIsDeletedAndStatusIsNotCreatedOrSent();

            using (CurrentUnitOfWork.SetTenantId(dispatch.TenantId))
            using (AbpSession.Use(dispatch.TenantId, null))
            {
                if (await DispatchWasAcknowledgedOrCreatedToday())
                {
                    var nextDispatch = await GetNextDispatchInAnyActiveStatus(dispatch.TruckId, dispatch.DriverId);
                    return nextDispatch != null ? new GetNextDispatchTodayResult(nextDispatch.Guid) : new GetNextDispatchTodayResult();
                }
                return new GetNextDispatchTodayResult();
            }

            // Local functions
            async Task<bool> DispatchWasAcknowledgedOrCreatedToday()
            {
                var timeZone = await GetTimezone();
                var today = await GetToday();
                return dispatch.OrderDate >= today;
            }

            async Task<Dispatch> GetNextDispatchInAnyActiveStatus(int truckId, int driverId)
            {
                return await _dispatchRepository.GetAll()
                    .Where(d => d.TruckId == truckId && d.DriverId == driverId && (d.Status == DispatchStatus.Created || Dispatch.ActiveStatuses.Contains(d.Status)))
                    .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                    .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                    .ThenBy(d => d.SortOrder)
                    .FirstOrDefaultAsync();
            }


            void ThrowArgumentExceptionIfDispatchIsNull()
            {
                if (dispatch == null)
                {
                    throw new ArgumentException($"There is no dispatch with Guid={acknowledgedDispatchGuid}");
                }
            }

            void ThrowArgumentExceptionIfDispatchIsNotDeletedAndStatusIsNotCompletedOrCanceled()
            {
                if (!dispatch.IsDeleted && dispatch.Status != DispatchStatus.Completed && dispatch.Status != DispatchStatus.Canceled)
                {
                    throw new ArgumentException("The dispatch must be completed or canceled!");
                }
            }
            void ThrowArgumentExceptionIfDispatchIsDeletedAndStatusIsNotCreatedOrSent()
            {
                if (dispatch.IsDeleted && dispatch.Status != DispatchStatus.Created && dispatch.Status != DispatchStatus.Sent)
                {
                    throw new ArgumentException("The dispatch must be completed or canceled!");
                }
            }
        }

        public async Task<RawDispatchDto> GetFirstOpenDispatch(int driverId)
        {
            return await _dispatchRepository.GetAll()
                .Where(d => d.DriverId == driverId && Dispatch.OpenStatuses.Contains(d.Status))
                //.OrderBy(d => !Dispatch.ActiveStatuses.Contains(d.Status)) //active statuses first
                .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                .ThenByDescending(d => d.Status == DispatchStatus.Sent)
                .ThenBy(d => d.SortOrder)
                .ToRawDispatchDto()
                .FirstOrDefaultAsync();
        }

        private string GetAcknowledgementUrl(Guid acknowledgementGuid)
        {
            string siteUrl = _webUrlService.GetSiteRootAddress();
            return $"{siteUrl}app/acknowledge/{acknowledgementGuid.ToShortGuid()}";
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        [RemoteService(IsEnabled = false)]
        public async Task DeleteUnacknowledgedDispatches(DeleteUnacknowledgedDispatchesInput input)
        {
            var unacknowledgedStatuses = new[] { DispatchStatus.Created, DispatchStatus.Sent };
            var dispatchesToDelete = await _dispatchRepository.GetAll()
                .Where(d => d.OrderLineId == input.OrderLineId && d.TruckId == input.TruckId && unacknowledgedStatuses.Contains(d.Status))
                .ToListAsync();

            var activeDispatchesForDrivers = new List<RawDispatchDto>();
            foreach (var dispatch in dispatchesToDelete.GroupBy(x => x.DriverId))
            {
                activeDispatchesForDrivers.Add(await GetFirstOpenDispatch(dispatch.Key));
            }

            //dispatchesToDelete.ForEach(async d => await _dispatchRepository.DeleteAsync(d));
            dispatchesToDelete.ForEach(SetDispatchEntityStatusToCanceled);
            await CurrentUnitOfWork.SaveChangesAsync();
            foreach (var dispatchGroup in dispatchesToDelete.GroupBy(x => x.DriverId))
            {
                await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchGroup.Key)
                {
                    LogMessage = $"Deleted unacknowledged dispatch(es) {string.Join(", ", dispatchGroup.Select(x => x.Id))}"
                });
            }
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch, dispatchesToDelete.Select(x => x.ToChangedEntity()), ChangeType.Removed)
                    .AddLogMessage("Deleted unacknowledged dispatch(es)"));

            foreach (var dispatch in dispatchesToDelete)
            {
                var activeDispatchWasChanged = false;
                var oldActiveDispatch = activeDispatchesForDrivers.FirstOrDefault(x => x.DriverId == dispatch.DriverId);
                if (oldActiveDispatch != null)
                {
                    var newActiveDispatch = await GetFirstOpenDispatch(dispatch.DriverId);
                    activeDispatchWasChanged = oldActiveDispatch?.Id != newActiveDispatch?.Id;
                }

                await _dispatchSender.SendSmsOrEmail(new SendSmsOrEmailInput
                {
                    TruckId = dispatch.TruckId,
                    DriverId = dispatch.DriverId,
                    PhoneNumber = dispatch.PhoneNumber,
                    EmailAddress = dispatch.EmailAddress,
                    OrderNotifyPreferredFormat = dispatch.OrderNotifyPreferredFormat,
                    ActiveDispatchWasChanged = activeDispatchWasChanged
                });
            }
        }

        private static void SetDispatchEntityStatusToCanceled(Dispatch dispatch)
        {
            dispatch.Status = DispatchStatus.Canceled;
            dispatch.Canceled = Clock.Now;
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task CancelDispatches(CancelDispatchesInput input)
        {
            List<Dispatch> dispatchesToCancel;
            if (input.OrderLineId.HasValue)
            {
                dispatchesToCancel = await _dispatchRepository.GetAll()
                    .Where(d => d.OrderLineId == input.OrderLineId && input.CancelDispatchStatuses.Contains(d.Status))
                    .WhereIf(input.TruckId.HasValue, d => d.TruckId == input.TruckId.Value)
                    .ToListAsync();
            }
            else if (input.TruckId.HasValue && input.Date.HasValue)
            {
                dispatchesToCancel = await _dispatchRepository.GetAll()
                    .Where(d => d.TruckId == input.TruckId.Value
                        && d.OrderLine.Order.DeliveryDate == input.Date
                        && d.OrderLine.Order.Shift == input.Shift
                        && input.CancelDispatchStatuses.Contains(d.Status))
                    .ToListAsync();
            }
            else
            {
                throw new ApplicationException("Either OrderLineId or (TruckId, Date) are required");
            }

            var activeDispatchesForDrivers = new List<RawDispatchDto>();
            foreach (var dispatch in dispatchesToCancel.GroupBy(x => x.DriverId))
            {
                activeDispatchesForDrivers.Add(await GetFirstOpenDispatch(dispatch.Key));
            }

            dispatchesToCancel.ForEach(SetDispatchEntityStatusToCanceled);
            await CurrentUnitOfWork.SaveChangesAsync();
            foreach (var dispatchGroup in dispatchesToCancel.GroupBy(x => x.DriverId))
            {
                await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchGroup.Key)
                {
                    LogMessage = $"Canceled dispatch(es) {string.Join(", ", dispatchGroup.Select(x => x.Id))}"
                });
            }
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch, dispatchesToCancel.Select(x => x.ToChangedEntity()), ChangeType.Removed)
                    .AddLogMessage("Canceled dispatch(es)"));

            foreach (var dispatch in dispatchesToCancel)
            {
                var activeDispatchWasChanged = false;
                var oldActiveDispatch = activeDispatchesForDrivers.FirstOrDefault(x => x.DriverId == dispatch.DriverId);
                if (oldActiveDispatch != null)
                {
                    var newActiveDispatch = await GetFirstOpenDispatch(dispatch.DriverId);
                    activeDispatchWasChanged = oldActiveDispatch?.Id != newActiveDispatch?.Id;
                }

                await _dispatchSender.SendSmsOrEmail(new SendSmsOrEmailInput
                {
                    TruckId = dispatch.TruckId,
                    DriverId = dispatch.DriverId,
                    PhoneNumber = dispatch.PhoneNumber,
                    EmailAddress = dispatch.EmailAddress,
                    OrderNotifyPreferredFormat = dispatch.OrderNotifyPreferredFormat,
                    ActiveDispatchWasChanged = activeDispatchWasChanged
                });
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task CancelDispatch(CancelDispatchDto cancelDispatch)
        {
            var dispatchEntity = await _dispatchRepository.GetAll()
                .FirstAsync(x => x.Id == cancelDispatch.DispatchId);
            if (dispatchEntity == null)
            {
                throw new ApplicationException($"Dispatch with Id={cancelDispatch.DispatchId} not found!");
            }

            if (dispatchEntity.Status == DispatchStatus.Completed)
            {
                throw new UserFriendlyException(L("CannotChangeDispatchStatus"));
            }

            if (!await IsAllowedToCancelDispatch(cancelDispatch.DispatchId))
            {
                throw new UserFriendlyException(L("CannotCancelDispatchesWithTickets"));
            }

            var oldActiveDispatch = await GetFirstOpenDispatch(dispatchEntity.DriverId);

            SetDispatchEntityStatusToCanceled(dispatchEntity);
            await CurrentUnitOfWork.SaveChangesAsync();
            var logMessage = $"Canceled dispatch {dispatchEntity.Id}";

            var newActiveDispatch = await GetFirstOpenDispatch(dispatchEntity.DriverId);
            var syncRequest = new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity(), ChangeType.Removed);

            if (cancelDispatch.CancelAllDispatchesForDriver)
            {
                var dispatchesToCancel = await _dispatchRepository.GetAll()
                    .Where(d => d.DriverId == dispatchEntity.DriverId && d.Status == DispatchStatus.Created)
                    .ToListAsync();
                dispatchesToCancel.ForEach(SetDispatchEntityStatusToCanceled);
                logMessage += $"; Canceled all dispatches for driver, dispatches {string.Join(", ", dispatchesToCancel.Select(x => x.Id))}";
                await CurrentUnitOfWork.SaveChangesAsync();

                syncRequest
                    .AddChanges(EntityEnum.Dispatch, dispatchesToCancel.Select(x => x.ToChangedEntity()), ChangeType.Removed)
                    .AddLogMessage("Canceled all dispatches for driver");
            }
            else
            {
                syncRequest.AddLogMessage("Canceled dispatch");
            }

            await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchEntity.DriverId)
            {
                LogMessage = logMessage
            });
            await _syncRequestSender.SendSyncRequest(syncRequest);

            if (!cancelDispatch.CancelAllDispatchesForDriver)
            {
                await _dispatchSender.SendSmsOrEmail(new SendSmsOrEmailInput
                {
                    TruckId = dispatchEntity.TruckId,
                    DriverId = dispatchEntity.DriverId,
                    PhoneNumber = dispatchEntity.PhoneNumber,
                    EmailAddress = dispatchEntity.EmailAddress,
                    OrderNotifyPreferredFormat = dispatchEntity.OrderNotifyPreferredFormat,
                    ActiveDispatchWasChanged = oldActiveDispatch?.Id != newActiveDispatch?.Id
                });
            }
        }

        private async Task<bool> IsAllowedToCancelDispatch(int dispatchId)
        {
            return !await _loadRepository.GetAll()
                .AnyAsync(l => l.DispatchId == dispatchId && l.Tickets.Any());
        }

        public async Task CancelDispatchForDriver(CancelDispatchForDriverInput input)
        {
            var dispatchEntity = await _dispatchRepository.GetAll()
                .Include(x => x.Loads)
                    .ThenInclude(x => x.Tickets)
                .Where(x => x.Id == input.DispatchId)
                .FirstOrDefaultAsync();

            if (dispatchEntity == null || dispatchEntity.Status == DispatchStatus.Completed || dispatchEntity.Status == DispatchStatus.Canceled)
            {
                return;
            }

            try
            {
                if (!await IsAllowedToCancelDispatch(input.DispatchId))
                {
                    dispatchEntity.Status = DispatchStatus.Completed;
                    return;
                }

                dispatchEntity.Status = DispatchStatus.Canceled;
                dispatchEntity.Canceled = input.Info.ActionTimeInUtc;

                if (!dispatchEntity.WasMultipleLoads)
                {
                    if (dispatchEntity.Loads != null)
                    {
                        foreach (var load in dispatchEntity.Loads)
                        {
                            if (!load.Tickets.Any())
                            {
                                _loadRepository.Delete(load);
                            }
                        }
                    }

                }
            }
            finally
            {
                await CurrentUnitOfWork.SaveChangesAsync();

                //await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchEntity.DriverId)
                //{
                //    LogMessage = $"Canceled dispatch {dispatchEntity.Id}"
                //});
                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity(), ChangeType.Removed)
                    .SetIgnoreForDeviceId(input.Info?.DeviceId)
                    .AddLogMessage("Canceled dispatch"));
            }
        }

        public async Task MarkDispatchComplete(MarkDispatchCompleteInput input)
        {
            var dispatchEntity = await _dispatchRepository.GetAll()
                .Include(x => x.Loads)
                .Where(x => x.Id == input.DispatchId)
                .FirstOrDefaultAsync();

            if (dispatchEntity == null || dispatchEntity.Status == DispatchStatus.Completed || dispatchEntity.Status == DispatchStatus.Canceled)
            {
                return;
            }

            dispatchEntity.Status = DispatchStatus.Completed;

            await CurrentUnitOfWork.SaveChangesAsync();

            //await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchEntity.DriverId)
            //{
            //    LogMessage = $"Canceled dispatch {dispatchEntity.Id}"
            //});
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity(), ChangeType.Removed)
                .SetIgnoreForDeviceId(input.Info?.DeviceId)
                .AddLogMessage("Marked dispatch complete"));
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task<bool> OtherDispatchesExist(OtherDispatchesExistInput input)
        {
            var dispatchInfo = await _dispatchRepository.GetAll()
                .Where(d => d.Id == input.DispatchId)
                .Select(d => new
                {
                    d.DriverId,
                })
                .FirstAsync();
            return await _dispatchRepository.GetAll()
                .AnyAsync(d => d.Id != input.DispatchId && d.DriverId == dispatchInfo.DriverId && d.Status == DispatchStatus.Created);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task<ViewDispatchDto> ViewDispatch(int dispatchId)
        {
            return await _dispatchRepository.GetAll()
                .Where(d => d.Id == dispatchId)
                .Select(d => new ViewDispatchDto
                {
                    Id = d.Id,
                    TruckCode = d.Truck.TruckCode,
                    CustomerName = d.OrderLine.Order.Customer.Name,
                    Item = d.OrderLine.Service.Service1,
                    Tickets = d.Loads.OrderByDescending(l => l.Id).Select(l => l.Tickets.Select(t => new ViewDispatchTicketDto
                    {
                        Quantity = t.Quantity,
                        Designation = d.OrderLine.Designation,
                        TicketUomId = t.UnitOfMeasureId,
                        MaterialUomId = d.OrderLine.MaterialUomId,
                        FreightUomId = d.OrderLine.FreightUomId,
                    }).ToList()).FirstOrDefault(),
                    MaterialUomId = d.OrderLine.MaterialUomId,
                    FreightUomId = d.OrderLine.FreightUomId,
                    Status = d.Status,
                    Sent = _timeZoneConverter.Convert(d.Sent, AbpSession.TenantId, AbpSession.UserId.Value),
                    Loaded = _timeZoneConverter.Convert(d.Loads.OrderByDescending(l => l.Id).Select(l => l.SourceDateTime).FirstOrDefault(), AbpSession.TenantId, AbpSession.UserId.Value),
                    Delivered = _timeZoneConverter.Convert(d.Loads.OrderByDescending(l => l.Id).Select(l => l.DestinationDateTime).FirstOrDefault(), AbpSession.TenantId, AbpSession.UserId.Value),
                })
                .FirstAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task<List<TruckDispatchListItemDto>> TruckDispatchList(TruckDispatchListInput input)
        {
            var dispatchQuery = _dispatchRepository.GetAll()
                .Where(d => d.Status != DispatchStatus.Canceled && d.Status != DispatchStatus.Completed)
                .WhereIf(input.OfficeId.HasValue, d => d.OrderLine.Order.LocationId == input.OfficeId.Value)
                .WhereIf(input.DispatchIds?.Any() == true, d => input.DispatchIds.Contains(d.Id))
                .WhereIf(input.DriverIds?.Any() == true, d => input.DriverIds.Contains(d.DriverId))
                .WhereIf(input.TruckIds?.Any() == true, d => input.TruckIds.Contains(d.TruckId));

            var truckDispatchPlainList = await dispatchQuery
                .Select(d => new
                {
                    SortOrder = d.SortOrder,
                    OfficeId = d.OrderLine.Order.LocationId,
                    OfficeName = d.OrderLine.Order.Office.Name,
                    TruckId = d.TruckId,
                    TruckCode = d.Truck.TruckCode,
                    DriverId = d.DriverId,
                    LastName = d.Driver.LastName,
                    FirstName = d.Driver.FirstName,
                    UserId = d.UserId,

                    Id = d.Id,
                    ShortGuid = d.Guid.ToShortGuid(),
                    Guid = d.Guid,
                    Status = d.Status,
                    DeliveryDate = d.OrderLine.Order.DeliveryDate.Value,
                    Shift = d.OrderLine.Order.Shift,
                    TimeOnJobUtc = d.TimeOnJob,
                    CustomerName = d.OrderLine.Order.Customer.Name,
                    LoadAt = d.OrderLine.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = d.OrderLine.LoadAt.Name,
                        StreetAddress = d.OrderLine.LoadAt.StreetAddress,
                        City = d.OrderLine.LoadAt.City,
                        State = d.OrderLine.LoadAt.State
                    },
                    DeliverTo = d.OrderLine.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = d.OrderLine.DeliverTo.Name,
                        StreetAddress = d.OrderLine.DeliverTo.StreetAddress,
                        City = d.OrderLine.DeliverTo.City,
                        State = d.OrderLine.DeliverTo.State
                    },
                    Designation = d.OrderLine.Designation,
                    Item = d.OrderLine.Service.Service1,
                    MaterialUom = d.OrderLine.MaterialUom.Name,
                    FreightUom = d.OrderLine.FreightUom.Name,
                    Created = d.CreationTime,
                    Acknowledged = d.Acknowledged,
                    Sent = d.Sent,
                    Loaded = d.Loads.OrderByDescending(l => l.Id).Select(l => l.SourceDateTime).FirstOrDefault(),
                    Complete = d.Loads.OrderByDescending(l => l.Id).Select(l => l.DestinationDateTime).FirstOrDefault(),
                    d.IsMultipleLoads,
                    d.WasMultipleLoads
                })
                .ToListAsync();

            var timeZone = await GetTimezone();

            Debug.Assert(AbpSession.UserId != null, "AbpSession.UserId != null");
            var truckDispatchList = (
                from d in truckDispatchPlainList
                group d by new { OfficeId = d.OfficeId, OfficeName = d.OfficeName, d.TruckId, d.TruckCode, d.DriverId, d.LastName, d.FirstName } into truckDriverGroup
                orderby truckDriverGroup.Key.OfficeId, truckDriverGroup.Key.TruckCode, truckDriverGroup.Key.LastName, truckDriverGroup.Key.FirstName
                select new TruckDispatchListItemDto
                {
                    OfficeId = truckDriverGroup.Key.OfficeId,
                    OfficeName = truckDriverGroup.Key.OfficeName,
                    TruckId = truckDriverGroup.Key.TruckId,
                    TruckCode = truckDriverGroup.Key.TruckCode,
                    DriverId = truckDriverGroup.Key.DriverId,
                    LastName = truckDriverGroup.Key.LastName,
                    FirstName = truckDriverGroup.Key.FirstName,
                    Dispatches = truckDriverGroup
                        .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                        .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                        .ThenBy(d => d.SortOrder)
                        .Select(d => new TruckDispatchListItemDto.TruckDispatch
                        {
                            Id = d.Id,
                            SortOrder = d.SortOrder,
                            ShortGuid = d.Guid.ToShortGuid(),
                            Guid = d.Guid,
                            Status = d.Status,
                            DeliveryDate = d.DeliveryDate,
                            Shift = d.Shift,
                            TimeOnJob = d.TimeOnJobUtc?.ConvertTimeZoneTo(timeZone),
                            CustomerName = d.CustomerName,
                            LoadAt = d.LoadAt,
                            DeliverTo = d.DeliverTo,
                            Item = d.Item,
                            MaterialUom = d.MaterialUom,
                            FreightUom = d.FreightUom,
                            Created = d.Created.ConvertTimeZoneTo(timeZone),
                            Acknowledged = d.Acknowledged?.ConvertTimeZoneTo(timeZone),
                            Sent = d.Sent?.ConvertTimeZoneTo(timeZone),
                            Loaded = d.Loaded?.ConvertTimeZoneTo(timeZone),
                            Complete = d.Complete?.ConvertTimeZoneTo(timeZone),
                            IsMultipleLoads = d.IsMultipleLoads,
                            WasMultipleLoads = d.WasMultipleLoads
                        }).ToList()
                })
                .ToList();

            var nowTimeInUserTimeZone = Clock.Now.ConvertTimeZoneTo(timeZone);
            var today = nowTimeInUserTimeZone.Date;
            var todayOfUserInUtcTimezone = today.ConvertTimeZoneFrom(timeZone);

            if (input.View.IsIn(DispatchListViewEnum.DriversNotClockedIn,
                DispatchListViewEnum.TrucksWithDriversAndNoDispatches,
                DispatchListViewEnum.AllTrucks))
            {
                if (input.DispatchIds?.Any() != true)
                {
                    var driverAssignments = await _driverAssignmentRepository.GetAll()
                        .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                        .WhereIf(input.DriverIds?.Any() == true, x => x.DriverId != null && input.DriverIds.Contains(x.DriverId.Value))
                        .WhereIf(input.TruckIds?.Any() == true, x => input.TruckIds.Contains(x.TruckId))
                        .Where(x => x.Date == today)
                        .Select(x => new
                        {
                            OfficeId = x.OfficeId,
                            OfficeName = x.Office.Name,
                            TruckId = x.TruckId,
                            TruckCode = x.Truck.TruckCode,
                            DriverId = x.DriverId,
                            LastName = x.Driver.LastName,
                            FirstName = x.Driver.FirstName,
                            UserId = x.Driver.UserId,
                        }).ToListAsync();

                    foreach (var driverAssignment in driverAssignments)
                    {
                        if (truckDispatchList.Any(x => x.TruckId == driverAssignment.TruckId && x.DriverId == driverAssignment.DriverId))
                        {
                            continue;
                        }

                        truckDispatchList.Add(new TruckDispatchListItemDto
                        {
                            OfficeId = driverAssignment.OfficeId,
                            OfficeName = driverAssignment.OfficeName,
                            TruckId = driverAssignment.TruckId,
                            TruckCode = driverAssignment.TruckCode,
                            DriverId = driverAssignment.DriverId,
                            LastName = driverAssignment.LastName,
                            FirstName = driverAssignment.FirstName
                        });
                    }
                }

                if (input.View == DispatchListViewEnum.AllTrucks && input.DispatchIds?.Any() != true)
                {
                    var leaseHaulerFeatureEnabled = await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature);

                    var truckQuery = _truckRepository.GetAll()
                        .Where(x => x.IsActive && !x.IsOutOfService && x.VehicleCategory.IsPowered)
                        .WhereIf(input.TruckIds?.Any() == true, x => input.TruckIds.Contains(x.Id));

                    var trucks = await truckQuery
                        .Where(x => x.LocationId != null)
                        .WhereIf(!leaseHaulerFeatureEnabled, x => x.LocationId != null && x.LeaseHaulerTruck.AlwaysShowOnSchedule != true)
                        .WhereIf(input.OfficeId.HasValue, x => x.LocationId == input.OfficeId)
                        .Select(x => new
                        {
                            OfficeId = x.LocationId,
                            OfficeName = x.Office.Name,
                            TruckId = x.Id,
                            TruckCode = x.TruckCode
                        }).ToListAsync();

                    var leaseHaulerTrucks = await truckQuery
                        .Where(x => x.LocationId == null)
                        .SelectMany(x => x.AvailableLeaseHaulerTrucks)
                        .WhereIf(input.OfficeId.HasValue, a => a.OfficeId == input.OfficeId)
                        .Where(a => a.Date == today)
                        .Select(x => new
                        {
                            x.TruckId,
                            x.Truck.TruckCode,
                            x.DriverId,
                            x.Driver.FirstName,
                            x.Driver.LastName,
                            x.OfficeId,
                            OfficeName = x.Office.Name
                        })
                        .ToListAsync();

                    foreach (var truck in leaseHaulerTrucks)
                    {
                        if (truckDispatchList.Any(x => x.TruckId == truck.TruckId))
                        {
                            continue;
                        }

                        truckDispatchList.Add(new TruckDispatchListItemDto
                        {
                            OfficeId = truck.OfficeId,
                            OfficeName = truck.OfficeName,
                            TruckId = truck.TruckId,
                            TruckCode = truck.TruckCode,
                            DriverId = truck.DriverId,
                            FirstName = truck.FirstName,
                            LastName = truck.LastName,
                            IsExternal = true
                        });
                    }

                    foreach (var truck in trucks)
                    {
                        if (truckDispatchList.Any(x => x.TruckId == truck.TruckId))
                        {
                            continue;
                        }

                        truckDispatchList.Add(new TruckDispatchListItemDto
                        {
                            OfficeId = truck.OfficeId,
                            OfficeName = truck.OfficeName,
                            TruckId = truck.TruckId,
                            TruckCode = truck.TruckCode
                        });
                    }
                }
            }

            if (truckDispatchList.Any())
            {
                var dateRange = truckDispatchPlainList.Select(x => x.DeliveryDate)
                    .Union(new[] { today }).Distinct().ToList();
                var driverIdRange = truckDispatchList.Select(x => x.DriverId).Distinct().ToList();
                //var shiftRange = truckDispatchPlainList.Select(x => x.Shift).Distinct().ToList();
                var driverStartTimes = await _driverAssignmentRepository.GetAll()
                    .Where(x => x.DriverId.HasValue && driverIdRange.Contains(x.DriverId.Value) && dateRange.Contains(x.Date))
                    //.WhereIf(shiftRange.Any(), x => shiftRange.Contains(x.Shift))
                    .OrderBy(x => x.StartTime == null)
                    .ThenBy(x => x.StartTime)
                    .Select(x => new
                    {
                        x.DriverId,
                        x.TruckId,
                        x.Date,
                        x.Shift,
                        StartTimeUtc = x.StartTime
                    }).ToListAsync();

                var userIdList = await _driverRepository.GetAll()
                    .Where(x => driverIdRange.Contains(x.Id))
                    .Select(x => new { x.UserId, DriverId = x.Id })
                    .ToListAsync();
                var userIdRange = userIdList.Select(x => x.UserId).ToList();

                Debug.Assert(AbpSession.TenantId != null, "AbpSession.TenantId != null");
                var earliestDate = dateRange.Min().ConvertTimeZoneFrom(timeZone);
                var latestDate = dateRange.Max().AddDays(1).ConvertTimeZoneFrom(timeZone);
                var driverClockIns = await _employeeTimeRepository.GetAll()
                    .Where(x => userIdRange.Contains(x.UserId) && x.StartDateTime >= earliestDate && x.StartDateTime < latestDate)
                    .Select(x => new
                    {
                        x.StartDateTime,
                        x.EndDateTime,
                        x.IsImported,
                        x.UserId
                    }).ToListAsync();

                var officeSettings = await _officeRepository.GetAll()
                    .WhereIf(input.OfficeId.HasValue, x => x.Id == input.OfficeId)
                    .Select(x => new
                    {
                        x.Id,
                        DefaultStartTimeUtc = x.DefaultStartTime
                    }).ToListAsync();

                foreach (var truck in truckDispatchList)
                {
                    foreach (var dispatch in truck.Dispatches)
                    {
                        if (truck.IsExternal)
                        {
                            if (dispatch.TimeOnJob.HasValue)
                            {
                                dispatch.StartTime = dispatch.DeliveryDate.Add(dispatch.TimeOnJob.Value.TimeOfDay);
                            }
                        }
                        else
                        {
                            var startTimeRecord = driverStartTimes
                                .FirstOrDefault(x => x.Date == dispatch.DeliveryDate
                                                    && x.Shift == dispatch.Shift
                                                    && x.DriverId == truck.DriverId
                                                    && x.TruckId == truck.TruckId);
                            if (startTimeRecord?.StartTimeUtc != null)
                            {
                                var startTime = startTimeRecord.StartTimeUtc.Value.ConvertTimeZoneTo(timeZone);
                                dispatch.StartTime = startTimeRecord.Date.Date.Add(startTime.TimeOfDay);
                            }

                            if (dispatch.StartTime == null)
                            {
                                var officeSetting = officeSettings.FirstOrDefault(x => x.Id == truck.OfficeId);
                                if (officeSetting?.DefaultStartTimeUtc != null)
                                {
                                    var defaultStartTime = officeSetting.DefaultStartTimeUtc.Value.ConvertTimeZoneTo(timeZone);
                                    dispatch.StartTime = dispatch.DeliveryDate.Add(defaultStartTime.TimeOfDay);
                                }
                                else
                                {
                                    var defaultStartTime = (await SettingManager.GetSettingValueAsync<DateTime>(AppSettings.DispatchingAndMessaging.DefaultStartTime)).ConvertTimeZoneTo(timeZone);
                                    dispatch.StartTime = dispatch.DeliveryDate.Add(defaultStartTime.TimeOfDay);
                                }
                            }
                        }
                    }

                    truck.StartTime = driverStartTimes.FirstOrDefault(x => x.Date == today && x.TruckId == truck.TruckId)?.StartTimeUtc?.ConvertTimeZoneTo(timeZone);
                    truck.UserId = userIdList.FirstOrDefault(x => x.DriverId == truck.DriverId)?.UserId;
                    if (truck.UserId != null)
                    {
                        truck.IsClockedIn = driverClockIns.Any(x => x.EndDateTime == null
                                                                && !x.IsImported
                                                                && x.UserId == truck.UserId
                                                                && x.StartDateTime >= todayOfUserInUtcTimezone
                                                                && x.StartDateTime < todayOfUserInUtcTimezone.AddDays(1));

                        truck.HasClockedInToday = driverClockIns.Any(x => x.UserId == truck.UserId
                                                            && x.StartDateTime >= todayOfUserInUtcTimezone
                                                            && x.StartDateTime < todayOfUserInUtcTimezone.AddDays(1));
                    }
                }
            }

            if (input.View == DispatchListViewEnum.OpenDispatches)
            {
                truckDispatchList.RemoveAll(x => !x.Dispatches.Any());
            }
            else if (input.View == DispatchListViewEnum.DriversNotClockedIn)
            {
                truckDispatchList.RemoveAll(x => x.HasClockedInToday);
            }
            else if (input.View == DispatchListViewEnum.UnacknowledgedDispatches)
            {
                truckDispatchList.RemoveAll(x => x.Dispatches.FirstOrDefault()?.Status.IsIn(DispatchStatus.Sent, DispatchStatus.Created) != true);
            }
            else if (input.View == DispatchListViewEnum.TrucksWithDriversAndNoDispatches)
            {
                truckDispatchList.RemoveAll(x => x.DriverId == null || x.Dispatches.Any());
            }
            else if (input.View == DispatchListViewEnum.AllTrucks)
            {
                //
            }

            return truckDispatchList
                .OrderBy(x => x.OfficeName)
                .ThenBy(x => x.TruckCode)
                .ThenBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();
        }

        public async Task<List<TruckDispatchClockInInfoDto>> GetTruckDispatchClockInInfo(GetTruckDispatchClockInInfoInput input)
        {
            var result = new List<TruckDispatchClockInInfoDto>();

            var timeZone = await GetTimezone();
            var today = await GetToday();
            var todayOfUserInUtcTimezone = today.ConvertTimeZoneFrom(timeZone);

            var todayClockIns = await _employeeTimeRepository.GetAll()
                .Where(x => input.UserIds.Contains(x.UserId)
                    && x.StartDateTime >= todayOfUserInUtcTimezone
                    && x.StartDateTime < todayOfUserInUtcTimezone.AddDays(1))
                .Select(x => new
                {
                    x.StartDateTime,
                    x.EndDateTime,
                    x.IsImported,
                    x.UserId
                }).ToListAsync();

            foreach (var userId in input.UserIds)
            {
                result.Add(new TruckDispatchClockInInfoDto
                {
                    UserId = userId,
                    IsClockedIn = todayClockIns.Any(x => x.EndDateTime == null
                                                            && !x.IsImported
                                                            && x.UserId == userId
                                                            && x.StartDateTime >= todayOfUserInUtcTimezone
                                                            && x.StartDateTime < todayOfUserInUtcTimezone.AddDays(1)),
                    HasClockedInToday = todayClockIns.Any(x => x.UserId == userId
                                                        && x.StartDateTime >= todayOfUserInUtcTimezone
                                                        && x.StartDateTime < todayOfUserInUtcTimezone.AddDays(1))
                });
            }

            return result;
        }

        public async Task<List<TruckDispatchDriverAssignmentInfoDto>> GetTruckDispatchDriverAssignmentInfo(GetTruckDispatchDriverAssignmentInfoInput input)
        {
            var today = await GetToday();
            var driverStartTimes = await _driverAssignmentRepository.GetAll()
                    .WhereIf(input.TruckIds?.Any() == true, x => input.TruckIds.Contains(x.TruckId) && x.Date == today)
                    .OrderBy(x => x.StartTime == null)
                    .ThenBy(x => x.StartTime)
                    .Select(x => new
                    {
                        x.TruckId,
                        x.DriverId,
                        //x.Shift,
                        StartTimeUtc = x.StartTime
                    }).ToListAsync();

            var timezone = await GetTimezone();
            return driverStartTimes.GroupBy(x => x.TruckId).Select(x => new TruckDispatchDriverAssignmentInfoDto
            {
                TruckId = x.Key,
                StartTime = x.First().StartTimeUtc?.ConvertTimeZoneTo(timezone)
            }).ToList();
        }

        [AbpAllowAnonymous]
        public async Task<List<DriverLoadDestinationInfoDto>> GetDriverDispatchesForCurrentUser(GetDriverDispatchesForCurrentUserInput input)
        {
            var authInfo = await _driverApplicationAuthProvider.AuthDriverByDriverGuidIfNeeded(input.DriverGuid);
            var allowProductionPay = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay, authInfo.TenantId);
            var items = await _dispatchRepository.GetAll()
                    .WhereIf(input.UpdatedAfterDateTime.HasValue, d => d.CreationTime > input.UpdatedAfterDateTime.Value || (d.LastModificationTime != null && d.LastModificationTime > input.UpdatedAfterDateTime.Value))
                    .WhereIf(input.UpdatedAfterDateTime == null, d => d.Status != DispatchStatus.Canceled && d.Status != DispatchStatus.Completed)
                    .Where(d => d.DriverId == authInfo.DriverId && (Dispatch.ActiveStatuses.Contains(d.Status) || d.Status == DispatchStatus.Created || d.Status == DispatchStatus.Canceled || d.Status == DispatchStatus.Completed))
                    //.OrderBy(d => !Dispatch.ActiveStatuses.Contains(d.Status)) //active statuses first
                    .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                    .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                    .ThenByDescending(d => d.Status == DispatchStatus.Sent)
                    .ThenBy(d => d.SortOrder)
            .Select(di => new DriverLoadDestinationInfoDto
            {
                CustomerName = di.OrderLine.Order.Customer.Name,
                Date = di.OrderLine.Order.DeliveryDate.Value,
                Shift = di.OrderLine.Order.Shift,
                Item = di.OrderLine.Service.Service1,
                Designation = di.OrderLine.Designation,
                TimeOnJob = di.TimeOnJob,
                TruckCode = di.Truck.TruckCode,
                LoadAtName = di.OrderLine.LoadAt.Name,
                LoadAt = di.OrderLine.LoadAt == null ? null : new LocationAddressDto
                {
                    StreetAddress = di.OrderLine.LoadAt.StreetAddress,
                    City = di.OrderLine.LoadAt.City,
                    State = di.OrderLine.LoadAt.State,
                    ZipCode = di.OrderLine.LoadAt.ZipCode,
                    CountryCode = di.OrderLine.LoadAt.CountryCode
                },
                LoadAtLatitude = di.OrderLine.LoadAt.Latitude,
                LoadAtLongitude = di.OrderLine.LoadAt.Longitude,
                ChargeTo = di.OrderLine.Order.ChargeTo,
                //LoadId = withTicket ? di.Load != null ? di.Load.Id : (int?)null : null,
                //TicketNumber = withTicket ? di.Load != null ? di.Load.TicketNumber : null : null,
                //MaterialAmount = withTicket ? di.Load != null ? di.Load.TicketMaterialQuantity : 0 : 0,
                //FreightAmount = withTicket ? di.Load != null ? di.Load.TicketFreightQuantity : 0 : 0,
                MaterialUomName = di.OrderLine.MaterialUom.Name,
                FreightUomName = di.OrderLine.FreightUom.Name,
                MaterialQuantity = di.OrderLine.MaterialQuantity,
                FreightQuantity = di.OrderLine.FreightQuantity,
                JobNumber = di.OrderLine.JobNumber,
                Note = di.Note,
                IsMultipleLoads = di.IsMultipleLoads,
                WasMultipleLoads = di.WasMultipleLoads,
                SignatureId = di.Loads.OrderByDescending(l => l.Id).Select(l => l.SignatureId).FirstOrDefault(), //.DefaultIfEmpty((Guid?)null)
                Loaded = di.Loads.OrderByDescending(l => l.Id).Select(l => l.SourceDateTime).FirstOrDefault(),
                Acknowledged = di.Acknowledged,
                HasTickets = di.Loads.Any(l => l.Tickets.Any()),
                DeliverToName = di.OrderLine.DeliverTo.Name,
                DeliverTo = di.OrderLine.DeliverTo == null ? null : new LocationAddressDto
                {
                    StreetAddress = di.OrderLine.DeliverTo.StreetAddress,
                    City = di.OrderLine.DeliverTo.City,
                    State = di.OrderLine.DeliverTo.State,
                    ZipCode = di.OrderLine.DeliverTo.ZipCode,
                    CountryCode = di.OrderLine.DeliverTo.CountryCode
                },
                DeliverToLatitude = di.OrderLine.DeliverTo.Latitude,
                DeliverToLongitude = di.OrderLine.DeliverTo.Longitude,
                DispatchStatus = di.Status,
                DispatchId = di.Id,
                Guid = di.Guid,
                TenantId = di.TenantId,
                LastUpdateDateTime = di.LastModificationTime.HasValue && di.LastModificationTime.Value > di.CreationTime ? di.LastModificationTime.Value : di.CreationTime,
                Id = di.Id,
                SortOrder = di.SortOrder,
                NumberOfAddedLoads = di.NumberOfAddedLoads,
                NumberOfLoadsToFinish = di.NumberOfLoadsToFinish,
                ProductionPay = allowProductionPay && di.OrderLine.ProductionPay
            }).ToListAsync();

            var timezone = await GetTimezone(authInfo.TenantId, authInfo.UserId);
            items.ForEach(item =>
            {
                item.Acknowledged = item.Acknowledged?.ConvertTimeZoneTo(timezone);
                item.TimeOnJob = item.TimeOnJob?.ConvertTimeZoneTo(timezone);
            });

            return items;
        }

        [AbpAllowAnonymous]
        public async Task<int?> GetTenantIdFromDispatch(Guid dispatchGuid)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                var tenantId = await _dispatchRepository.GetAll()
                    .Where(d => d.Guid == dispatchGuid)
                    .Select(x => x.TenantId)
                    .FirstOrDefaultAsync();

                return tenantId;
            }
        }

        [AbpAllowAnonymous]
        public async Task<DriverInfoBaseDto> GetDriverInfo(GetDriverInfoInput input)
        {
            Logger.Debug($"TenantId={AbpSession.TenantId}");
            var dispatch = await _dispatchRepository.GetAll()
                .Where(d => d.Guid == input.AcknowledgeGuid)
                .FirstOrDefaultAsync();
            if (dispatch == null)
            {
                if (await IsDispatchDeleted(input.AcknowledgeGuid))
                {
                    return new DriverInfoDeletedDto();
                }
                return new DriverInfoNotFoundDto();
            }

            if (!dispatch.Status.IsIn(DispatchStatus.Completed, DispatchStatus.Canceled))
            {
                var openDispatch = await GetFirstOpenDispatch(dispatch.DriverId);
                if (openDispatch != null && openDispatch.Guid != input.AcknowledgeGuid)
                {
                    return new DriverInfoErrorAndRedirect
                    {
                        Message = L("CompleteExistingDispatchesBeforeFutureDispatchesWithUrl"),
                        UrlText = L("CapitalClickHere"),
                        RedirectUrl = GetAcknowledgementUrl(openDispatch.Guid)
                    };
                }
            }

            var driverInfoQuery = _dispatchRepository.GetAll()
                    .Where(d => d.Guid == input.AcknowledgeGuid)
                    .Select(d => new
                    {
                        d.Status,
                        CustomerName = d.OrderLine.Order.Customer.Name,
                        d.OrderLine.Order.DeliveryDate,
                        d.OrderLine.Order.Shift,
                        d.OrderLine.Order.ChargeTo,
                        d.OrderLine.Designation,
                        Item = d.OrderLine.Service.Service1,
                        d.OrderLine.MaterialQuantity,
                        d.OrderLine.FreightQuantity,
                        LoadAtName = d.OrderLine.LoadAt.Name,
                        LoadAtAddress = new
                        {
                            d.OrderLine.LoadAt.StreetAddress,
                            d.OrderLine.LoadAt.City,
                            d.OrderLine.LoadAt.State,
                            d.OrderLine.LoadAt.ZipCode,
                            d.OrderLine.LoadAt.CountryCode,
                        },
                        DeliverToName = d.OrderLine.DeliverTo.Name,
                        DeliverToAddress = new
                        {
                            d.OrderLine.DeliverTo.StreetAddress,
                            d.OrderLine.DeliverTo.City,
                            d.OrderLine.DeliverTo.State,
                            d.OrderLine.DeliverTo.ZipCode,
                            d.OrderLine.DeliverTo.CountryCode,
                        },
                        Load = d.Loads
                            .OrderByDescending(l => l.Id)
                            .Select(l => new
                            {
                                l.Id,
                                TicketNumber = l.Tickets.FirstOrDefault() != null ? l.Tickets.FirstOrDefault().TicketNumber : null,
                                TicketQuantity = l.Tickets.FirstOrDefault() != null ? l.Tickets.FirstOrDefault().Quantity : 0,
                                SignatureId = l.SignatureId
                            }).FirstOrDefault(),
                        MaterialUomName = d.OrderLine.MaterialUom.Name,
                        FreightUomName = d.OrderLine.FreightUom.Name,
                        d.Note,
                        d.IsMultipleLoads,
                        d.WasMultipleLoads
                    })
                ;

            //if (await SettingManager.DispatchViaSms() && dispatch.Status == DispatchStatus.Created && await DoesNotHaveDispathesPermissions(dispatch))
            //{
            //    throw new ApplicationException("Dispatch wasn't sent yet or a user doesn't have edit permissions!");
            //}

            using (CurrentUnitOfWork.SetTenantId(dispatch.TenantId))
            using (AbpSession.Use(dispatch.TenantId, null))
            {
                DriverInfoDto driverInfoDto;

                switch (dispatch.Status)
                {
                    case DispatchStatus.Created:
                    case DispatchStatus.Sent:
                        driverInfoDto = await GetDriverLoadInfo();
                        break;
                    case DispatchStatus.Acknowledged:
                        driverInfoDto = await GetDriverLoadInfo();
                        break;
                    case DispatchStatus.Loaded:
                        driverInfoDto = input.EditTicket ? (DriverInfoDto)await GetDriverLoadInfo(input.EditTicket) : await GetDriverDestinationInfoDto();
                        break;
                    case DispatchStatus.Completed:
                        driverInfoDto = new DriverInfoCompletedDto();
                        break;
                    case DispatchStatus.Canceled:
                        driverInfoDto = new DriverInfoCanceledDto();
                        break;
                    default:
                        throw new ApplicationException($"Unexpected dispatch status: {dispatch.Status}");
                }
                driverInfoDto.DispatchStatus = dispatch.Status;
                driverInfoDto.DispatchId = dispatch.Id;
                driverInfoDto.Guid = dispatch.Guid;
                driverInfoDto.TenantId = dispatch.TenantId;
                driverInfoDto.IsMultipleLoads = dispatch.IsMultipleLoads;
                driverInfoDto.WasMultipleLoads = dispatch.WasMultipleLoads;

                return driverInfoDto;
            }

            // Local functions
            async Task<DriverLoadInfoDto> GetDriverLoadInfo(bool withTicket = false)
            {
                return await driverInfoQuery.Select(di => new DriverLoadInfoDto
                {
                    CustomerName = di.CustomerName,
                    Date = di.DeliveryDate.Value,
                    Shift = di.Shift,
                    Item = di.Item,
                    Designation = di.Designation,
                    LoadAtName = di.LoadAtName,
                    LoadAt = di.LoadAtAddress == null ? null : new LocationAddressDto
                    {
                        StreetAddress = di.LoadAtAddress.StreetAddress,
                        City = di.LoadAtAddress.City,
                        State = di.LoadAtAddress.State,
                        ZipCode = di.LoadAtAddress.ZipCode,
                        CountryCode = di.LoadAtAddress.CountryCode
                    },
                    ChargeTo = di.ChargeTo,
                    LoadId = withTicket ? di.Load != null ? di.Load.Id : (int?)null : null,
                    TicketNumber = withTicket ? di.Load != null ? di.Load.TicketNumber : null : null,
                    Amount = withTicket ? di.Load != null ? di.Load.TicketQuantity : 0 : 0,
                    MaterialUomName = di.MaterialUomName,
                    FreightUomName = di.FreightUomName,
                    MaterialQuantity = di.MaterialQuantity,
                    FreightQuantity = di.FreightQuantity,
                    Note = di.Note,
                    IsMultipleLoads = di.IsMultipleLoads,
                    WasMultipleLoads = di.WasMultipleLoads,
                    SignatureId = di.Load != null ? di.Load.SignatureId : (Guid?)null
                })
                .FirstAsync();
            }
            async Task<DriverDestinationInfoDto> GetDriverDestinationInfoDto()
            {
                return await driverInfoQuery.Select(di => new DriverDestinationInfoDto
                {
                    CustomerName = di.CustomerName,
                    DeliverToName = di.DeliverToName,
                    DeliverTo = di.DeliverToAddress == null ? null : new LocationAddressDto
                    {
                        StreetAddress = di.DeliverToAddress.StreetAddress,
                        City = di.DeliverToAddress.City,
                        State = di.DeliverToAddress.State,
                        ZipCode = di.DeliverToAddress.ZipCode,
                        CountryCode = di.DeliverToAddress.CountryCode
                    },
                    Date = di.DeliveryDate.Value,
                    Shift = di.Shift,
                    Note = di.Note,
                    SignatureId = di.Load != null ? di.Load.SignatureId : (Guid?)null
                })
                .FirstAsync();
            }
            async Task<bool> IsDispatchDeleted(Guid guid)
            {
                ISoftDelete entity;
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    entity = await _dispatchRepository.GetAll().FirstOrDefaultAsync(d => d.Guid == guid);
                }
                return entity != null && entity.IsDeleted;
            }
        }

        private async Task<bool> DoesNotHaveDispathesPermissions(Dispatch dispatch) =>
                (AbpSession.UserId == null || !await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_Dispatches_Edit));

        [AbpAllowAnonymous]
        public async Task UpdateDispatchTicket(DispatchTicketDto dispatchTicket)
        {
            if (!await SettingManager.DispatchViaDriverApplication())
            {
                var dispatch = await _dispatchRepository.GetAll()
                    .Select(x => new
                    {
                        x.Guid,
                        x.OrderLine.Order.DeliveryDate
                    })
                    .FirstOrDefaultAsync(d => d.Guid == dispatchTicket.Guid);

                if (dispatch == null)
                {
                    return;
                }

                var today = await GetToday();
                if (dispatch.DeliveryDate > today)
                {
                    throw new UserFriendlyException(L("YouCantLoadFutureDispatch"));
                }
            }

            await UpdateDispatchTicket2(new UpdateDispatchTicketInput
            {
                DispatchTicket = dispatchTicket
            });
        }

        [AbpAllowAnonymous]
        public async Task UpdateDispatchTicket2(UpdateDispatchTicketInput input)
        {
            await LoadDispatch(input);
            await ModifyDispatchTicket(input);
        }

        public async Task LoadDispatch(UpdateDispatchTicketInput input)
        {
            var dispatchEntity = await _dispatchRepository.GetAll()
                .Include(d => d.Truck)
                .Include(d => d.OrderLine)
                    .ThenInclude(ol => ol.Order)
                .FirstOrDefaultAsync(d => d.Guid == input.DispatchTicket.Guid);

            Logger.Info($"[Dispatching] DriverId {input.Info?.DriverId} ({AbpSession.UserId}) loaded dispatchGuid {input.DispatchTicket.Guid} ({dispatchEntity?.Id}) in status {dispatchEntity?.Status.ToString()}");

            if (dispatchEntity == null)
            {
                return;
            }

            if (dispatchEntity.Status == DispatchStatus.Canceled)
            {
                if (input.Info == null)
                {
                    //request didn't come from driver application
                    return;
                }
            }

            using (CurrentUnitOfWork.SetTenantId(dispatchEntity.TenantId))
            using (AbpSession.Use(dispatchEntity.TenantId, null))
            {
                Load loadEntity;
                if (input.DispatchTicket.IsEdit)
                {
                    loadEntity = await _loadRepository.GetAll()
                        .Include(l => l.Tickets)
                        .Where(l => l.DispatchId == dispatchEntity.Id)
                        .OrderByDescending(l => l.Id)
                        .FirstOrDefaultAsync();
                }
                else if (input.DispatchTicket.LoadId != null)
                {
                    loadEntity = await _loadRepository.GetAll()
                        .Include(l => l.Tickets)
                        .Where(l => l.Id == input.DispatchTicket.LoadId && l.DispatchId == dispatchEntity.Id)
                        .OrderByDescending(l => l.Id)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    loadEntity = await _loadRepository.GetAll()
                        .Include(l => l.Tickets)
                        .Where(l => l.DispatchId == dispatchEntity.Id && l.SourceDateTime == null)
                        .OrderByDescending(l => l.Id)
                        .FirstOrDefaultAsync() ?? new Load { DispatchId = dispatchEntity.Id };
                }

                loadEntity.SourceLatitude = input.DispatchTicket.SourceLatitude;
                loadEntity.SourceLongitude = input.DispatchTicket.SourceLongitude;

                var ticket = loadEntity.Tickets.LastOrDefault();

                if (dispatchEntity.Status == DispatchStatus.Acknowledged)
                {
                    await ChangeDispatchStatusToLoadedAsync(dispatchEntity, loadEntity, input.Info);
                }
                else
                {
                    Logger.Warn($"[Dispatching] DriverId {input.Info?.DriverId} ({AbpSession.UserId}) tried to load dispatchId {dispatchEntity.Id} but it is in status {dispatchEntity.Status.ToString()}");
                    loadEntity.SourceDateTime = loadEntity.SourceDateTime ?? input.Info?.ActionTimeInUtc ?? Clock.Now;
                    if (ticket != null)
                    {
                        ticket.TicketDateTime = ticket.TicketDateTime ?? input.Info?.ActionTimeInUtc ?? Clock.Now;
                    }
                }

                await _loadRepository.InsertOrUpdateAsync(loadEntity);
                await CurrentUnitOfWork.SaveChangesAsync();

                await _orderTaxCalculator.CalculateTotalsAsync(dispatchEntity.OrderLine.OrderId);
                if (ticket != null)
                {
                    await _fuelSurchargeCalculator.RecalculateTicket(ticket.Id);
                }

                await CurrentUnitOfWork.SaveChangesAsync();
                if (input.Info == null)
                {
                    await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchEntity.DriverId)
                    {
                        LogMessage = $"Loaded dispatch {dispatchEntity.Id}"
                    });
                }
                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity())
                    .SetIgnoreForDeviceId(input.Info?.DeviceId)
                    .AddLogMessage("Loaded dispatch"));
            }
        }

        public async Task ModifyDispatchTicket(UpdateDispatchTicketInput input)
        {
            var dispatchEntity = await _dispatchRepository.GetAll()
                .Include(d => d.Truck)
                .Include(d => d.OrderLine)
                    .ThenInclude(ol => ol.Order)
                .FirstOrDefaultAsync(d => d.Guid == input.DispatchTicket.Guid);

            Logger.Info($"[Dispatching] DriverId {input.Info?.DriverId} ({AbpSession.UserId}) modified ticket for dispatchGuid {input.DispatchTicket.Guid} ({dispatchEntity?.Id}) in status {dispatchEntity?.Status.ToString()}");

            if (dispatchEntity == null)
            {
                return;
            }

            if (dispatchEntity.Status == DispatchStatus.Canceled)
            {
                Logger.Warn($"[Dispatching] DriverId {input.Info?.DriverId} ({AbpSession.UserId}) modified ticket for dispatchGuid {input.DispatchTicket.Guid} ({dispatchEntity?.Id}) but it is canceled");
                if (input.Info == null)
                {
                    //request didn't come from driver application
                    return;
                }
            }

            using (CurrentUnitOfWork.SetTenantId(dispatchEntity.TenantId))
            using (AbpSession.Use(dispatchEntity.TenantId, null))
            {
                Load loadEntity;
                if (input.DispatchTicket.IsEdit || dispatchEntity.Status == DispatchStatus.Loaded || input.DispatchTicket.DispatchStatus == DispatchStatus.Loaded)
                {
                    loadEntity = await _loadRepository.GetAll()
                        .Include(l => l.Tickets)
                        .Where(l => l.DispatchId == dispatchEntity.Id)
                        .OrderByDescending(l => l.Id)
                        .FirstOrDefaultAsync();
                }
                else if (input.DispatchTicket.LoadId != null)
                {
                    loadEntity = await _loadRepository.GetAll()
                        .Include(l => l.Tickets)
                        .Where(l => l.Id == input.DispatchTicket.LoadId && l.DispatchId == dispatchEntity.Id)
                        .OrderByDescending(l => l.Id)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    loadEntity = await _loadRepository.GetAll()
                        .Include(l => l.Tickets)
                        .Where(l => l.DispatchId == dispatchEntity.Id && l.SourceDateTime == null)
                        .OrderByDescending(l => l.Id)
                        .FirstOrDefaultAsync() ?? new Load { DispatchId = dispatchEntity.Id };
                }

                var orderTotalsBeforeUpdate = await GetOrderTotalsAsync(dispatchEntity.OrderLineId);

                var ticket = loadEntity.Tickets.LastOrDefault();
                if (ticket == null)
                {
                    var timeZone = await GetTimezone();
                    ticket = new Ticket
                    {
                        OrderLineId = dispatchEntity.OrderLineId,
                        OfficeId = dispatchEntity.OrderLine.Order.LocationId,
                        LoadAtId = dispatchEntity.OrderLine.LoadAtId,
                        DeliverToId = dispatchEntity.OrderLine.DeliverToId,
                        TruckId = dispatchEntity.TruckId,
                        TruckCode = dispatchEntity.Truck.TruckCode,
                        CustomerId = dispatchEntity.OrderLine.Order.CustomerId,
                        ServiceId = dispatchEntity.OrderLine.ServiceId,
                        DriverId = dispatchEntity.DriverId,
                        TicketDateTime = input.Info?.ActionTimeInUtc ?? Clock.Now, //dispatchEntity.OrderLine.Order.DeliveryDate?.Date.Add(Clock.Now.ConvertTimeZoneTo(timezone).TimeOfDay).ConvertTimeZoneFrom(timezone), //(dispatchEntity.OrderLine.Order.DateTime ?? DateTime.Today).Date.Add(input.Info.ActionTime.TimeOfDay),
                        UnitOfMeasureId = dispatchEntity.OrderLine.Designation.MaterialOnly() || dispatchEntity.OrderLine.Designation == DesignationEnum.FreightAndMaterial
                            ? dispatchEntity.OrderLine.MaterialUomId
                            : dispatchEntity.OrderLine.FreightUomId,
                        TenantId = AbpSession.TenantId ?? 0
                    };
                    loadEntity.Tickets.Add(ticket);

                    var truckDetails = await _truckRepository.GetAll()
                        .Where(x => x.Id == ticket.TruckId)
                        .Select(x => new
                        {
                            LeaseHaulerId = (int?)x.LeaseHaulerTruck.LeaseHaulerId
                        }).FirstOrDefaultAsync();
                    ticket.CarrierId = truckDetails?.LeaseHaulerId;
                }
                if (!input.DispatchTicket.CreateNewTicket)
                {
                    ticket.TicketNumber = input.DispatchTicket.TicketNumber;
                }
                ticket.Quantity = input.DispatchTicket.Amount > 0
                    ? input.DispatchTicket.Amount.Value
                    : input.DispatchTicket.FreightAmount > 0
                    ? input.DispatchTicket.FreightAmount.Value
                    : input.DispatchTicket.MaterialAmount ?? 0;

                if (input.DispatchTicket.TicketPhotoId.HasValue)
                {
                    ticket.TicketPhotoId = input.DispatchTicket.TicketPhotoId;
                    ticket.TicketPhotoFilename = input.DispatchTicket.TicketPhotoFilename;
                }
                if (input.DispatchTicket.DeferredPhotoId.HasValue)
                {
                    var existingDeferredDataId = await PopDataIdFromExistingDeferred(input.DispatchTicket.DeferredPhotoId.Value);
                    if (existingDeferredDataId != null)
                    {
                        ticket.TicketPhotoId = existingDeferredDataId;
                    }
                    else
                    {
                        ticket.DeferredTicketPhotoId = input.DispatchTicket.DeferredPhotoId;
                    }
                    ticket.TicketPhotoFilename = input.DispatchTicket.TicketPhotoFilename;
                }

                await _loadRepository.InsertOrUpdateAsync(loadEntity);
                await CurrentUnitOfWork.SaveChangesAsync();

                if (input.DispatchTicket.CreateNewTicket && ticket != null)
                {
                    ticket.TicketNumber = "G-" + (ticket.Id);
                }

                await CurrentUnitOfWork.SaveChangesAsync();
                await _orderTaxCalculator.CalculateTotalsAsync(dispatchEntity.OrderLine.OrderId);
                await _fuelSurchargeCalculator.RecalculateTicket(ticket.Id);

                await CurrentUnitOfWork.SaveChangesAsync();
                if (input.Info == null)
                {
                    await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchEntity.DriverId)
                    {
                        LogMessage = $"Modified dispatch ticket, dispatch {dispatchEntity.Id}"
                    });
                }
                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity())
                    .SetIgnoreForDeviceId(input.Info?.DeviceId)
                    .AddLogMessage("Modified dispatch ticket"));

                await NotifyDispatchersAfterTicketUpdateIfNeeded(dispatchEntity.OrderLineId, orderTotalsBeforeUpdate);
            }
        }

        [RemoteService(false)]
        public async Task NotifyDispatchersAfterTicketUpdateIfNeeded(int orderLineId, GetOrderTotalsResult orderTotalsBeforeUpdate)
        {
            var orderTotalsAfterUpdate = await GetOrderTotalsAsync(orderLineId);

            if (!orderTotalsBeforeUpdate.AmountExceedsQuantity && orderTotalsAfterUpdate.AmountExceedsQuantity)
            {
                var orderDetails = await _orderLineRepository.GetAll()
                    .Where(x => x.Id == orderLineId)
                    .Select(x => new
                    {
                        OfficeId = x.Order.LocationId,
                        SharedOfficeIds = x.SharedOrderLines.Select(s => s.OfficeId),
                        CustomerName = x.Order.Customer.Name
                    })
                    .FirstOrDefaultAsync();

                await _appNotifier.SendPriorityNotification(
                    new SendPriorityNotificationInput(
                        L("OrderHasReachedRequestedAmount").Replace("{CustomerName}", orderDetails.CustomerName),
                        NotificationSeverity.Warn,
                        orderDetails.SharedOfficeIds.Union(new[] { orderDetails.OfficeId }).ToArray()
                    )
                    {
                        OnlineFilter = true,
                        RoleFilter = new[] { StaticRoleNames.Tenants.Dispatching }
                    });
            }
        }

        [RemoteService(false)]
        public async Task<GetOrderTotalsResult> GetOrderTotalsAsync(int orderLineId)
        {
            var data = await _orderLineRepository.GetAll()
                    .Where(x => x.Id == orderLineId)
                    .Select(ol => new
                    {
                        FreightQuantity = ol.FreightQuantity,
                        MaterialQuantity = ol.MaterialQuantity,
                        DeliveryDate = ol.Order.DeliveryDate,
                        Tickets = ol.Tickets.Select(x => new
                        {
                            x.Quantity,
                            x.TicketDateTime
                        }).ToList()
                    }).FirstOrDefaultAsync();

            if (data == null)
            {
                return null;
            }

            var timezone = await GetTimezone();
            foreach (var ticket in data.Tickets.ToList())
            {
                if (ticket.TicketDateTime?.ConvertTimeZoneTo(timezone).Date != data.DeliveryDate)
                {
                    data.Tickets.Remove(ticket);
                }
            }

            return new GetOrderTotalsResult
            {
                FreightQuantity = data.FreightQuantity,
                MaterialQuantity = data.MaterialQuantity,
                ActualAmount = data.Tickets.Sum(t => t.Quantity)
            };
        }

        [AbpAllowAnonymous]
        public async Task AddSignature(AddSignatureInput input)
        {
            var dispatchEntity = await _dispatchRepository.GetAll()
                .FirstOrDefaultAsync(d => d.Guid == input.Guid);
            if (dispatchEntity == null)
            {
                //throw new UserFriendlyException("Dispatch wasn't found");
                return;
            }

            using (CurrentUnitOfWork.SetTenantId(dispatchEntity.TenantId))
            using (AbpSession.Use(dispatchEntity.TenantId, null))
            {
                Load loadEntity = await _loadRepository.GetAll().Where(l => l.DispatchId == dispatchEntity.Id && l.DestinationDateTime == null).OrderByDescending(l => l.Id).FirstOrDefaultAsync();
                if (loadEntity == null)
                {
                    return;
                }
                if (!input.Signature.IsNullOrEmpty())
                {
                    loadEntity.SignatureName = input.SignatureName;
                    loadEntity.SignatureId = await _binaryObjectManager.UploadDataUriStringAsync(input.Signature, AbpSession.TenantId);
                }
                if (!input.SignatureName.IsNullOrEmpty())
                {
                    loadEntity.SignatureName = input.SignatureName;
                }
                if (input.DeferredSignatureId != null)
                {
                    var existingDeferredDataId = await PopDataIdFromExistingDeferred(input.DeferredSignatureId.Value);
                    if (existingDeferredDataId != null)
                    {
                        loadEntity.SignatureId = existingDeferredDataId;
                    }
                    else
                    {
                        loadEntity.DeferredSignatureId = input.DeferredSignatureId;
                    }
                }
            }
        }

        private async Task<Guid?> PopDataIdFromExistingDeferred(Guid deferredId)
        {
            var existingDeferred = await _deferredBinaryObjectRepository.GetAll()
                .Where(x => x.Id == deferredId)
                .FirstOrDefaultAsync();

            if (existingDeferred == null)
            {
                return null;
            }

            var dataId = existingDeferred.BinaryObjectId;

            _deferredBinaryObjectRepository.Delete(existingDeferred);

            return dataId;
        }

        [AbpAllowAnonymous]
        public async Task<CompleteDispatchResult> CompleteDispatch(CompleteDispatchDto completeDispatch)
        {
            var dispatchEntity = await _dispatchRepository.GetAll()
                .FirstOrDefaultAsync(d => d.Guid == completeDispatch.Guid);

            Logger.Info($"[Dispatching] DriverId {completeDispatch.Info?.DriverId} ({AbpSession.UserId}) completed dispatchGuid {completeDispatch.Guid} ({dispatchEntity?.Id}) in status {dispatchEntity?.Status.ToString()}");


            if (dispatchEntity == null)
            {
                return new CompleteDispatchResult { NotFound = true };
            }

            if (dispatchEntity.Status == DispatchStatus.Canceled)
            {
                Logger.Warn($"[Dispatching] DriverId {completeDispatch.Info?.DriverId} ({AbpSession.UserId}) completed dispatchGuid {completeDispatch.Guid} ({dispatchEntity?.Id}) but it was canceled");
                if (completeDispatch.Info == null)
                {
                    //request didn't come from driver application
                    return new CompleteDispatchResult { IsCanceled = true };
                }
            }

            if (dispatchEntity.Status == DispatchStatus.Completed)
            {
                return new CompleteDispatchResult { IsCompleted = true };
            }

            using (CurrentUnitOfWork.SetTenantId(dispatchEntity.TenantId))
            using (AbpSession.Use(dispatchEntity.TenantId, null))
            {
                Load loadEntity = await _loadRepository.GetAll().Where(l => l.DispatchId == dispatchEntity.Id && l.DestinationDateTime == null).OrderByDescending(l => l.Id).FirstOrDefaultAsync();
                if (loadEntity == null)
                {
                    Logger.Warn($"[Dispatching] Dispatch {dispatchEntity.Id}: Load with null DestinationDateTime wasn't found.");
                    if (dispatchEntity.Status == DispatchStatus.Canceled)
                    {
                        loadEntity = await _loadRepository.GetAll().Where(l => l.DispatchId == dispatchEntity.Id).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                        if (loadEntity == null)
                        {
                            Logger.Warn($"[Dispatching] Dispatch {dispatchEntity.Id}: No loads exist for dispatch. Exiting CompleteDispatch");
                            return new CompleteDispatchResult { IsCanceled = true };
                        }
                        //otherwise we continue with the last completed load and overwrite its values with the values from the driver
                    }
                    else
                    {
                        Logger.Error($"[Dispatching] Dispatch {dispatchEntity.Id}: No loads with null DestinationDateTime exist for dispatch. Exiting CompleteDispatch");
                        return new CompleteDispatchResult { NotFound = true };
                    }
                }
                loadEntity.DestinationLatitude = completeDispatch.DestinationLatitude;
                loadEntity.DestinationLongitude = completeDispatch.DestinationLongitude;
                CompleteDispatchResult result;
                if (!(completeDispatch.IsMultipleLoads ?? dispatchEntity.IsMultipleLoads) || completeDispatch.ContinueMultiload != true || dispatchEntity.Status == DispatchStatus.Canceled)
                {
                    if (dispatchEntity.IsMultipleLoads && completeDispatch.ContinueMultiload == false)
                    {
                        UncheckMultipleLoads(dispatchEntity);
                    }

                    var sendSmsResult = await ChangeDispatchStatusToCompleted(dispatchEntity, loadEntity, completeDispatch.Info);

                    result = new CompleteDispatchResult()
                    {
                        NextDispatch = sendSmsResult is SendSmsResultSuccess || sendSmsResult is SendSmsResultNextDispatch,
                        NextDispatchShortGuid = (sendSmsResult as SendSmsResultNextDispatch)?.NextDispatchGuid.ToShortGuid(),
                    };
                }
                else
                {
                    ChangeMultipleLoadsDispatchStatusToAcknowledged(dispatchEntity, loadEntity, completeDispatch.Info);
                    dispatchEntity.Loads.Add(new Load());
                    await CurrentUnitOfWork.SaveChangesAsync();
                    result = new CompleteDispatchResult()
                    {
                        NextDispatch = true,
                        NextDispatchShortGuid = dispatchEntity.Guid.ToShortGuid(),
                    };
                }

                await CurrentUnitOfWork.SaveChangesAsync();
                if (completeDispatch.Info == null)
                {
                    await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchEntity.DriverId)
                    {
                        LogMessage = $"Completed dispatch {dispatchEntity.Id}"
                    });
                }
                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity(), ChangeType.Removed)
                    .SetIgnoreForDeviceId(completeDispatch.Info?.DeviceId)
                    .AddLogMessage("Completed dispatch"));
                return result;
            }
        }

        private void ChangeMultipleLoadsDispatchStatusToAcknowledged(Dispatch dispatch, Load load, DriverApplicationActionInfo info)
        {
            if (dispatch.Status != DispatchStatus.Loaded)
            {
                //throw new ApplicationException("Dispatch must be in 'Loaded' status!");
            }

            if (!dispatch.IsMultipleLoads)
            {
                //throw new ApplicationException("Dispatch must have IsMultipleLoads=true!");
            }
            dispatch.Status = DispatchStatus.Acknowledged;
            load.DestinationDateTime = info?.ActionTimeInUtc ?? Clock.Now;
        }

        private bool DispatchIsInAppropriatePreAcknowledgedState(Dispatch dispatch) =>
            dispatch.Status == DispatchStatus.Sent || dispatch.Status == DispatchStatus.Created;

        [AbpAllowAnonymous]
        public async Task AcknowledgeDispatch(AcknowledgeDispatchInput input)
        {
            var dispatch = await _dispatchRepository.GetAsync(input.DispatchId);
            Logger.Info($"[Dispatching] DriverId {input.Info?.DriverId} ({AbpSession.UserId}) acknowledged dispatchId {input.DispatchId} in status {dispatch.Status}");
            if (!DispatchIsInAppropriatePreAcknowledgedState(dispatch))
            {
                Logger.Warn($"[Dispatching] DriverId {input.Info?.DriverId} tried to acknowledge dispatchId {input.DispatchId} but it is in status {dispatch.Status}");
                dispatch.Acknowledged = dispatch.Acknowledged ?? input.Info?.ActionTimeInUtc ?? Clock.Now;
                return;
            }
            ChangeDispatchStatusToAcknowledged(dispatch, input.Info);

            await CurrentUnitOfWork.SaveChangesAsync();
            if (input.Info == null)
            {
                await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatch.DriverId)
                {
                    LogMessage = $"Acknowledged dispatch {dispatch.Id}"
                });
            }
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatch.ToChangedEntity())
                .SetIgnoreForDeviceId(input.Info?.DeviceId)
                .AddLogMessage("Acknowledged dispatch"));
        }

        private void ChangeDispatchStatusToAcknowledged(Dispatch dispatch, DriverApplicationActionInfo info)
        {
            if (!DispatchIsInAppropriatePreAcknowledgedState(dispatch))
            {
                //throw new ApplicationException("Dispatch must be in 'Sent' or 'Created' status!");
            }
            dispatch.Status = DispatchStatus.Acknowledged;
            dispatch.Acknowledged = info?.ActionTimeInUtc ?? Clock.Now;
        }

        private Task ChangeDispatchStatusToLoadedAsync(Dispatch dispatch, Load load, DriverApplicationActionInfo info)
        {
            if (dispatch.Status != DispatchStatus.Acknowledged)
            {
                //throw new ApplicationException("Dispatch must be in 'Acknowledged' status!");
            }
            if (dispatch.Status == DispatchStatus.Acknowledged)
            {
                dispatch.NumberOfAddedLoads++;
            }
            dispatch.Status = DispatchStatus.Loaded;
            load.SourceDateTime = info?.ActionTimeInUtc ?? Clock.Now;

            return Task.CompletedTask;
        }

        private async Task<SendSmsResult> ChangeDispatchStatusToCompleted(Dispatch dispatch, Load load, DriverApplicationActionInfo info)
        {
            var oldActiveDispatch = await GetFirstOpenDispatch(dispatch.DriverId);

            if (dispatch.NumberOfLoadsToFinish > 0 && dispatch.NumberOfLoadsToFinish < dispatch.NumberOfAddedLoads)
            {
                dispatch.Status = DispatchStatus.Canceled;
                //might already be canceled
                dispatch.Canceled = dispatch.Canceled ?? info?.ActionTimeInUtc ?? Clock.Now;
            }
            else
            {
                if (dispatch.Status != DispatchStatus.Canceled || info?.ActionTimeInUtc < dispatch.Canceled)
                {
                    dispatch.Status = DispatchStatus.Completed;
                }
            }
            load.DestinationDateTime = info?.ActionTimeInUtc ?? Clock.Now;

            await CurrentUnitOfWork.SaveChangesAsync();

            await SendCompletedDispatchNotificationIfNeeded(dispatch);

            var newActiveDispatch = await GetFirstOpenDispatch(dispatch.DriverId);

            return await _dispatchSender.SendSmsOrEmail(new SendSmsOrEmailInput
            {
                TruckId = dispatch.TruckId,
                DriverId = dispatch.DriverId,
                PhoneNumber = dispatch.PhoneNumber,
                EmailAddress = dispatch.EmailAddress,
                OrderNotifyPreferredFormat = dispatch.OrderNotifyPreferredFormat,
                SendOrdersToDriversImmediately = false,
                AfterCompleted = true,
                ActiveDispatchWasChanged = oldActiveDispatch?.Id != newActiveDispatch?.Id
            });
        }

        [RemoteService(false)]
        public async Task SendCompletedDispatchNotificationIfNeeded(int dispatchId)
        {
            await CurrentUnitOfWork.SaveChangesAsync();
            var dispatch = await _dispatchRepository.GetAsync(dispatchId);
            await SendCompletedDispatchNotificationIfNeeded(dispatch);
        }

        private async Task SendCompletedDispatchNotificationIfNeeded(Dispatch dispatch)
        {
            await CurrentUnitOfWork.SaveChangesAsync();

            var hasMoreDispatches = await _dispatchRepository.GetAll()
                    .Where(d => d.DriverId == dispatch.DriverId && (Dispatch.ActiveStatuses.Contains(d.Status) || d.Status == DispatchStatus.Created) && d.Id != dispatch.Id)
                    .AnyAsync();

            if (!hasMoreDispatches && !await ShouldSendOrdersToDriversImmediately())
            {
                var orderDetails = await _orderLineRepository.GetAll()
                    .Where(x => x.Id == dispatch.OrderLineId)
                    .Select(x => new
                    {
                        OfficeId = x.Order.LocationId,
                        SharedOfficeIds = x.SharedOrderLines.Select(s => s.OfficeId)
                    }).FirstOrDefaultAsync();
                var driverDetails = await _driverRepository.GetAll()
                    .Where(x => x.Id == dispatch.DriverId)
                    .Select(x => new
                    {
                        FullName = x.FirstName + " " + x.LastName
                    }).FirstOrDefaultAsync();

                if (orderDetails != null && driverDetails != null)
                {
                    await _appNotifier.SendPriorityNotification(
                        new SendPriorityNotificationInput(
                            L("DriverNameHasFinishedDispatches").Replace("{DriverName}", driverDetails.FullName),
                            NotificationSeverity.Warn,
                            orderDetails.SharedOfficeIds.Union(new[] { orderDetails.OfficeId }).ToArray()
                        )
                        {
                            OnlineFilter = true,
                            RoleFilter = new[] { StaticRoleNames.Tenants.Dispatching }
                        });
                }
            }
        }

        [AbpAllowAnonymous]
        public async Task<List<SelectListDto>> GetUnitOfMeasureSelectList(Guid dispatchGuid)
        {
            int tenantId = await _dispatchRepository.GetAll().Where(d => d.Guid == dispatchGuid).Select(d => d.TenantId).FirstAsync();

            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var query = _unitOfMeasureRepository.GetAll()
                    .Select(x => new SelectListDto
                    {
                        Id = x.Id.ToString(),
                        Name = x.Name
                    });

                return await query.ToListAsync();
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task TestDriverDispatchSmsTemplate(TestSmsNumberInput input)
        {
            var lastOrderLine = await DispatchSender.GetOrderLineDataForDispatchMessageQuery(
                        _orderLineRepository.GetAll()
                            .OrderByDescending(ol => ol.Id)
                    )
                    .FirstOrDefaultAsync();

            lastOrderLine ??= new Dto.DispatchSender.OrderLineDto
            {
                Id = 0,
                DeliveryDate = await GetToday(),
                Shift = Shift.Shift1,
                OrderNumber = 0,
                Customer = "CustomerName",
                Directions = "Comments",
                Note = "Note",
                OrderLineTimeOnJobUtc = Clock.Now,
                Service = "Service",
                LoadAt = new LocationNameDto { Name = "LoadAt" },
                Designation = DesignationEnum.MaterialOnly,
                MaterialQuantity = 0,
                FreightQuantity = 0,
                MaterialUom = "Material UOM",
                FreightUom = "Freight UOM",
                DeliverTo = new LocationNameDto { Name = "DeliverTo" }
            };

            string message = await _dispatchSender.CreateDispatchMessageFromTemplate(lastOrderLine);
            try
            {
                await _smsSender.SendAsync(message, input.FullPhoneNumber, false);
            }
            catch (ApiException e)
            {
                Logger.Error(e.ToString());
                throw new UserFriendlyException($"An error occurred while sending the message: {e.Message}");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task EndMultipleLoadsDispatch(int dispatchId)
        {
            await EndMultipleLoadsDispatches(new[] { dispatchId });
        }

        private static void UncheckMultipleLoads(Dispatch dispatch)
        {
            dispatch.IsMultipleLoads = false;

            dispatch.NumberOfLoadsToFinish = dispatch.NumberOfAddedLoads + (IsAcknowledgedOrEarlier(dispatch.Status) ? 1 : 0);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task EndMultipleLoadsDispatches(int[] dispatchIds)
        {
            var dispatches = await _dispatchRepository.GetAll()
                .Where(x => dispatchIds.Contains(x.Id)).ToListAsync();

            foreach (var dispatch in dispatches)
            {
                UncheckMultipleLoads(dispatch);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            foreach (var dispatchGroup in dispatches.GroupBy(x => x.DriverId))
            {
                await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchGroup.Key)
                {
                    LogMessage = $"Ended multiple loads dispatch(es) {string.Join(", ", dispatchGroup.Select(x => x.Id))}"
                });
            }
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChanges(EntityEnum.Dispatch, dispatches.Select(x => x.ToChangedEntity()))
                .AddLogMessage("Ended multiple load dispatch(es)"));
        }

        public async Task CancelOrEndAllDispatches(CancelOrEndAllDispatchesInput input)
        {
            await CancelDispatches(new CancelDispatchesInput
            {
                CancelDispatchStatuses = new[] { DispatchStatus.Created, DispatchStatus.Sent, DispatchStatus.Acknowledged },
                OrderLineId = input.OrderLineId,
                TruckId = input.TruckId
            });

            var loadedDispatchIds = await _dispatchRepository.GetAll()
                    .Where(d => d.OrderLineId == input.OrderLineId && d.Status == DispatchStatus.Loaded && d.IsMultipleLoads)
                    .WhereIf(input.TruckId.HasValue, d => d.TruckId == input.TruckId.Value)
                    .Select(x => x.Id)
                    .ToArrayAsync();

            await EndMultipleLoadsDispatches(loadedDispatchIds);
        }

        public async Task RemoveAllDispatches()
        {
            var statusesToCancel = new[] { DispatchStatus.Created, DispatchStatus.Sent };
            var dispatchesToCancel = await _dispatchRepository.GetAll()
                    .Where(d => statusesToCancel.Contains(d.Status))
                    .ToListAsync();

            dispatchesToCancel.ForEach(SetDispatchEntityStatusToCanceled);


            var loadedDispatchesToEnd = await _dispatchRepository.GetAll()
                    .Where(d => d.Status == DispatchStatus.Loaded && d.IsMultipleLoads)
                    .ToListAsync();

            loadedDispatchesToEnd.ForEach(UncheckMultipleLoads);


            await CurrentUnitOfWork.SaveChangesAsync();
            foreach (var dispatchGroup in dispatchesToCancel.GroupBy(x => x.DriverId))
            {
                await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchGroup.Key)
                {
                    LogMessage = $"Removed all dispatches (canceled) {string.Join(", ", dispatchGroup.Select(x => x.Id))}"
                });
            }
            foreach (var dispatchGroup in loadedDispatchesToEnd.GroupBy(x => x.DriverId))
            {
                await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchGroup.Key)
                {
                    LogMessage = $"Removed all dispatches (ended multiple load) {string.Join(", ", dispatchGroup.Select(x => x.Id))}"
                });
            }
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChanges(EntityEnum.Dispatch, dispatchesToCancel.Select(x => x.ToChangedEntity()), ChangeType.Removed)
                .AddChanges(EntityEnum.Dispatch, loadedDispatchesToEnd.Select(x => x.ToChangedEntity()), ChangeType.Modified)
                .AddLogMessage("Removed all dispatches (canceled or ended multiple load)"));

        }

        private static bool IsAcknowledgedOrEarlier(DispatchStatus status)
        {
            return status.IsIn(DispatchStatus.Acknowledged, DispatchStatus.Sent, DispatchStatus.Created);
        }

        public async Task TestPushSubscription(string payload)
        {
            var driverId = await _driverRepository.GetDriverIdByUserIdOrThrow(AbpSession.GetUserId());
            var subscriptions = await _driverRepository.GetAll()
                .Where(x => x.Id == driverId)
                .SelectMany(x => x.DriverPushSubscriptions)
                .Select(x => x.PushSubscription)
                .ToListAsync();

            foreach (var subscription in subscriptions)
            {
                await _webPushSender.SendAsync(subscription.ToDto(), payload);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_SendSyncRequest)]
        public async Task<bool> SendSilentSyncPushToDrivers(SendSilentSyncPushToDriversInput input)
        {
            //await _syncRequestSender.SendSyncRequest(new SyncRequest()
            //    .AddChange()); //todo send one 'change' for each entity type to force refresh of all entities / or add another ForceRefresh change request type since we don't have an id of the changed record to send
            return await _driverApplicationPushSender.SendPushMessageToDriversImmediately(new SendPushMessageToDriversInput(input.DriverIds)
            {
                LogMessage = $"Manually sent push request by user {AbpSession.UserId}"
            });
        }

        [AbpAllowAnonymous]
        private async Task SaveDriverPushSubscription(DriverApplicationActionInfo info, ModifyDriverPushSubscriptionInput input)
        {
            await _pushSubscriptionManager.AddDriverPushSubscription(new AddDriverPushSubscriptionInput
            {
                PushSubscription = input?.PushSubscription,
                DriverId = info.DriverId,
                DeviceId = info.DeviceId
            });
        }

        [AbpAllowAnonymous]
        private async Task RemoveDriverPushSubscription(DriverApplicationActionInfo info, ModifyDriverPushSubscriptionInput input)
        {
            await _pushSubscriptionManager.RemoveDriverPushSubscription(new RemoveDriverPushSubscriptionInput
            {
                PushSubscription = input?.PushSubscription,
                DriverId = info.DriverId,
                DeviceId = info.DeviceId
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Host_Dashboard)]
        public async Task CleanupPushSubscriptions()
        {
            await _pushSubscriptionManager.CleanupSubscriptions();
        }

        [AbpAllowAnonymous]
        public async Task<bool> ExecuteDriverApplicationAction(ExecuteDriverApplicationActionInput input)
        {
            try
            {
                var authInfo = await _driverApplicationAuthProvider.AuthDriverByDriverGuid(input.DriverGuid);
                using (Session.Use(authInfo.TenantId, authInfo.UserId))
                {
                    var info = new DriverApplicationActionInfo(input, authInfo);
                    info.TimeZone = await SettingManager.GetSettingValueForUserAsync(TimingSettingNames.TimeZone, info.TenantId, info.UserId);
#pragma warning disable CS0618 // Type or member is obsolete
                    //suppressed the warning since the field is used here for backwards compatibility
                    if (info.ActionTime.HasValue)
                    {
                        info.ActionTimeInUtc = info.ActionTime.Value.ConvertTimeZoneFrom(info.TimeZone);
                    }
#pragma warning restore CS0618 // Type or member is obsolete
                    switch (input.Action)
                    {
                        case DriverApplicationAction.ClockIn:
                            await DriverClockIn(info, input.ClockInData);
                            break;
                        case DriverApplicationAction.ClockOut:
                            await DriverClockOut(info);
                            break;
                        case DriverApplicationAction.AcknowledgeDispatch:
                            input.AcknowledgeDispatchData.Info = info;
                            await AcknowledgeDispatch(input.AcknowledgeDispatchData);
                            break;
                        case DriverApplicationAction.LoadDispatch:
                            await LoadDispatch(new UpdateDispatchTicketInput
                            {
                                DispatchTicket = input.LoadDispatchData,
                                Info = info
                            });
                            break;
                        case DriverApplicationAction.CancelDispatch:
                            input.CancelDispatchData.Info = info;
                            await CancelDispatchForDriver(input.CancelDispatchData);
                            break;
                        case DriverApplicationAction.MarkDispatchComplete:
                            input.MarkDispatchCompleteData.Info = info;
                            await MarkDispatchComplete(input.MarkDispatchCompleteData);
                            break;
                        case DriverApplicationAction.ModifyDispatchTicket:
                            await UploadTicketPhotoIfNeeded(input.LoadDispatchData);
                            await ModifyDispatchTicket(new UpdateDispatchTicketInput
                            {
                                DispatchTicket = input.LoadDispatchData,
                                Info = info
                            });
                            break;
                        case DriverApplicationAction.CompleteDispatch:
                            input.CompleteDispatchData.Info = info;
                            await CompleteDispatch(input.CompleteDispatchData);
                            break;
                        case DriverApplicationAction.AddSignature:
                            await AddSignature(input.AddSignatureData);
                            break;
                        case DriverApplicationAction.SaveDriverPushSubscription:
                            await SaveDriverPushSubscription(info, input.PushSubscriptionData);
                            break;
                        case DriverApplicationAction.RemoveDriverPushSubscription:
                            await RemoveDriverPushSubscription(info, input.PushSubscriptionData);
                            break;
                        case DriverApplicationAction.UploadDeferredBinaryObject:
                            await UploadDeferredBinaryObject(info, input.UploadDeferredData);
                            break;
                        case DriverApplicationAction.UploadLogs:
                            await UploadLogs(info, input.UploadLogsData);
                            break;
                        case DriverApplicationAction.ModifyEmployeeTime:
                            //todo need to implement for new flutter app.
                            throw new NotImplementedException();
                        //break;
                        case DriverApplicationAction.RemoveEmployeeTime:
                            //todo need to implement for new flutter app.
                            throw new NotImplementedException();
                        //break;
                        case DriverApplicationAction.AddDriverNote:
                            //todo need to implement for new flutter app.
                            throw new NotImplementedException();
                        //break;
                        default:
                            throw new ApplicationException("Received unexpected DriverApplicationAction");
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                //todo log the exception and alert the admins/devs
                Debug.Write(e);
                throw;
            }
        }

        private LogLevel GetLogLevelEnum(string logLevel)
        {
            switch (logLevel.ToLower())
            {
                case "info": return LogLevel.Information;
                case "warn": return LogLevel.Warning;
                case "error": return LogLevel.Error;
                case "debug": return LogLevel.Debug;
                case "critical": return LogLevel.Critical;
                case "trace": return LogLevel.Trace;
            }
            return LogLevel.None;
        }

        [AbpAllowAnonymous]
        private async Task UploadLogs(DriverApplicationActionInfo info, List<UploadLogsInput> inputList)
        {
            var i = 0;
            foreach (var input in inputList)
            {
                _driverApplicationLogRepository.Insert(new DriverApplicationLog
                {
                    OriginalLogId = input.Id,
                    ServiceWorker = input.Sw,
                    BatchOrder = i++, //in case the records in a batch get their ids out of order when saved, we can then filter by DriverId and order by (DateTime, OrderInBatch) to get the original order of log records
                    DateTime = input.DateTime, //.ConvertTimeZoneFrom(info.TimeZone),
                    DriverId = info.DriverId,
                    Level = GetLogLevelEnum(input.Level),
                    Message = input.Message,
                    TenantId = info.TenantId,
                    UserId = info.UserId,
                    DeviceId = info.DeviceId,
                    DeviceGuid = info.DeviceGuid
                });
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAllowAnonymous]
        public async Task UploadAnonymousLogs(UploadAnonymousLogsInput input)
        {
            var i = 0;
            foreach (var log in input.UploadLogsData)
            {
                var logDateTime = log.DateTime;
                if (input.TimezoneOffset.HasValue)
                {
                    logDateTime = logDateTime.AddMinutes(input.TimezoneOffset.Value);
                }

                _driverApplicationLogRepository.Insert(new DriverApplicationLog
                {
                    OriginalLogId = log.Id,
                    ServiceWorker = log.Sw,
                    BatchOrder = i++, //in case the records in a batch get their ids out of order when saved, we can then filter by DriverId and order by (DateTime, OrderInBatch) to get the original order of log records
                    DateTime = logDateTime,
                    DriverId = null,
                    Level = GetLogLevelEnum(log.Level),
                    Message = log.Message,
                    TenantId = null,
                    UserId = null,
                    DeviceId = input.DeviceId,
                    DeviceGuid = input.DeviceGuid
                });
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAllowAnonymous]
        private async Task UploadDeferredBinaryObject(DriverApplicationActionInfo info, UploadDeferredBinaryObjectInput input)
        {
            var dataId = await _binaryObjectManager.UploadDataUriStringAsync(input.BytesString, info.TenantId);
            if (dataId == null)
            {
                return;
            }
            switch (input.Destination)
            {
                case DeferredBinaryObjectDestination.TicketPhoto:
                    var existingTickets = await _ticketRepository.GetAll().Where(x => x.DeferredTicketPhotoId == input.DeferredId).ToListAsync();
                    if (existingTickets.Any())
                    {
                        existingTickets.ForEach(x =>
                        {
                            x.TicketPhotoId = dataId;
                            x.DeferredTicketPhotoId = null;
                        });
                        return;
                    }
                    break;
                case DeferredBinaryObjectDestination.LoadSignature:
                    var existingLoads = await _loadRepository.GetAll().Where(x => x.DeferredSignatureId == input.DeferredId).ToListAsync();
                    if (existingLoads.Any())
                    {
                        existingLoads.ForEach(x =>
                        {
                            x.SignatureId = dataId;
                            x.DeferredSignatureId = null;
                        });
                        return;
                    }
                    break;
                default:
                    Logger.Error($"UploadDeferredBinaryObject received and unexpected Destination (${input.Destination.ToIntString()}) from the DriverApplication. DriverId: {info.DriverId}; DeviceId: {info.DeviceId}");
                    return;
            }

            var existingDeferred = await _deferredBinaryObjectRepository.GetAll().FirstOrDefaultAsync(x => x.Id == input.DeferredId);
            if (existingDeferred != null)
            {
                var message = $"UploadDeferredBinaryObject: overwriting DeferredBinaryObject.Id {input.DeferredId} (BinaryObjectId, Destination, TenantId): \nwas ({existingDeferred.BinaryObjectId}, {existingDeferred.Destination}, {existingDeferred.TenantId}), \nnew ({dataId.Value}, {input.Destination}, {info.TenantId}); \nDriverId: {info.DriverId}; DeviceId: {info.DeviceId}";
                Logger.Warn(message);
                await _driverApplicationLogger.LogWarn(info.DriverId, message);
                existingDeferred.BinaryObjectId = dataId.Value;
                existingDeferred.Destination = input.Destination;
                existingDeferred.TenantId = info.TenantId;
            }
            else
            {
                await _deferredBinaryObjectRepository.InsertAsync(new DeferredBinaryObject
                {
                    Id = input.DeferredId,
                    BinaryObjectId = dataId.Value,
                    Destination = input.Destination,
                    TenantId = info.TenantId
                });
            }
        }

        [AbpAllowAnonymous]
        private async Task DriverClockIn(DriverApplicationActionInfo info, DriverClockInInput input)
        {
            //previously DriverApplicationAppService.CreateEmployeeTime
            var userDate = info.ActionTimeInUtc.ConvertTimeZoneTo(info.TimeZone).Date;
            var userDayBeginningInUtc = userDate.ConvertTimeZoneFrom(info.TimeZone);

            //NotEndedTodayEmployeeTimeExists
            if (await _employeeTimeRepository.GetAll()
                    .AnyAsync(et => et.UserId == info.UserId && et.StartDateTime >= userDayBeginningInUtc && et.EndDateTime == null && !et.IsImported))
            {
                return;
            }

            //SetPreviousEmployeeTimeEndDateTimeIfNull
            var notEndedEmployeeTime = await GetNotEndedEmployeeTime(info.UserId);
            if (notEndedEmployeeTime != null)
            {
                notEndedEmployeeTime.EndDateTime = notEndedEmployeeTime.StartDateTime.ConvertTimeZoneTo(info.TimeZone).EndOfDay().ConvertTimeZoneFrom(info.TimeZone);
            }

            var truckId = await _driverAssignmentRepository.GetAll()
                    .Where(da => da.DriverId == info.DriverId && da.Date == userDate)
                    .Select(da => (int?)da.TruckId)
                    .FirstOrDefaultAsync();

            var timeClassificationId = await GetValidatedTimeClassificationIdOrNullAsync(input.TimeClassificationId)
                ?? await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId, info.TenantId);

            var employeeTime = new Drivers.EmployeeTime
            {
                UserId = info.UserId,
                StartDateTime = info.ActionTimeInUtc,
                TimeClassificationId = timeClassificationId,
                EquipmentId = truckId,
                Latitude = input.Latitude,
                Longitude = input.Longitude,
                Description = input.Description,
                TenantId = AbpSession.TenantId ?? 0,
                DriverId = info.DriverId
            };
            await _employeeTimeRepository.InsertAsync(employeeTime);

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.EmployeeTime, employeeTime.ToChangedEntity())
                .SetIgnoreForDeviceId(info.DeviceId));
        }

        private async Task<int?> GetValidatedTimeClassificationIdOrNullAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }
            var timeClassification = await _timeClassificationRepository.GetAll()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id
                }).FirstOrDefaultAsync();

            return timeClassification?.Id;
        }

        [AbpAllowAnonymous]
        private async Task DriverClockOut(DriverApplicationActionInfo info)
        {
            //previously DriverApplicationAppService.SetEmployeeTimeEndDateTime
            var notEndedEmployeeTime = await GetNotEndedEmployeeTime(info.UserId);
            if (notEndedEmployeeTime == null)
            {
                //throw new ApplicationException("There is no not finished EmployeeTime!");
                return;
            }

            notEndedEmployeeTime.EndDateTime = info.ActionTimeInUtc;
        }

        [AbpAllowAnonymous]
        private async Task<Drivers.EmployeeTime> GetNotEndedEmployeeTime(long userId)
        {
            return await _employeeTimeRepository.GetAll()
                .FirstOrDefaultAsync(et => et.UserId == userId && et.EndDateTime == null && !et.IsImported);
        }

        [AbpAllowAnonymous]
        private async Task UploadTicketPhotoIfNeeded(DispatchTicketDto input)
        {
            if (input.TicketPhotoId != null || string.IsNullOrEmpty(input.TicketPhotoBase64))
            {
                return;
            }

            input.TicketPhotoId = await _binaryObjectManager.UploadDataUriStringAsync(input.TicketPhotoBase64, AbpSession.TenantId, 12000000);
        }

        public async Task<Document> GetDriverActivityDetailReport(GetDriverActivityDetailReportInput input)
        {
            var timezone = await GetTimezone();

            var drivers = await _driverRepository.GetAll()
                .Where(x => x.UserId.HasValue)
                .WhereIf(input.DriverId.HasValue, x => input.DriverId == x.Id)
                .Select(x => new
                {
                    DriverId = x.Id,
                    DriverName = x.LastName + ", " + x.FirstName,
                    UserId = x.UserId.Value
                }).OrderBy(d => d.DriverName).ToListAsync();

            var userId = input.DriverId.HasValue ? drivers.FirstOrDefault(x => x.DriverId == input.DriverId)?.UserId : null;
            var startDateConverted = input.DateBegin?.ConvertTimeZoneFrom(timezone);
            var endDateConverted = input.DateEnd?.AddDays(1).ConvertTimeZoneFrom(timezone);
            var employeeTimes = await _employeeTimeRepository.GetAll()
                .WhereIf(input.DriverId.HasValue, x => x.UserId == userId)
                .WhereIf(startDateConverted.HasValue, x => x.StartDateTime >= startDateConverted && x.StartDateTime < endDateConverted)
                .Select(x => new
                {
                    x.UserId,
                    TruckId = x.EquipmentId,
                    x.Truck.TruckCode,
                    x.StartDateTime,
                    x.EndDateTime,
                    TimeClassificationName = x.TimeClassification.Name
                })
                .OrderBy(x => x.StartDateTime)
                .ToListAsync();

            var driverAssignments = await _driverAssignmentRepository.GetAll()
                .WhereIf(input.DriverId.HasValue, x => x.DriverId == input.DriverId)
                .WhereIf(input.DateBegin.HasValue && input.DateEnd.HasValue, x => x.Date >= input.DateBegin && x.Date <= input.DateEnd)
                .Select(x => new
                {
                    x.DriverId,
                    x.Date,
                    x.TruckId,
                    StartTimeUtc = x.StartTime
                })
                .OrderBy(x => x.StartTimeUtc == null)
                .ThenBy(x => x.StartTimeUtc)
                .ToListAsync();

            var loads = await _loadRepository.GetAll()
                .WhereIf(input.DriverId.HasValue, x => x.Dispatch.DriverId == input.DriverId)
                .WhereIf(input.DateBegin.HasValue && input.DateEnd.HasValue, x => x.Dispatch.OrderLine.Order.DeliveryDate >= input.DateBegin && x.Dispatch.OrderLine.Order.DeliveryDate <= input.DateEnd)
                .Where(x => x.SourceDateTime.HasValue)
                .Select(x => new
                {
                    Date = x.Dispatch.OrderLine.Order.DeliveryDate,
                    DriverId = (int?)x.Dispatch.DriverId,
                    TruckId = (int?)x.Dispatch.TruckId,
                    TruckCode = x.Dispatch.Truck.TruckCode,
                    CustomerName = x.Dispatch.OrderLine.Order.Customer.Name,
                    LoadAt = x.Dispatch.OrderLine.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = x.Dispatch.OrderLine.LoadAt.Name,
                        StreetAddress = x.Dispatch.OrderLine.LoadAt.StreetAddress,
                        City = x.Dispatch.OrderLine.LoadAt.City,
                        State = x.Dispatch.OrderLine.LoadAt.State
                    },
                    DeliverTo = x.Dispatch.OrderLine.DeliverTo.Name == null ? null : new LocationNameDto
                    {
                        Name = x.Dispatch.OrderLine.DeliverTo.Name,
                        StreetAddress = x.Dispatch.OrderLine.DeliverTo.StreetAddress,
                        City = x.Dispatch.OrderLine.DeliverTo.City,
                        State = x.Dispatch.OrderLine.DeliverTo.State
                    },
                    Tickets = x.Tickets.Select(t => new
                    {
                        TicketNumber = t.TicketNumber,
                        Quantity = (decimal?)t.Quantity,
                        UomName = t.UnitOfMeasure.Name,
                    }).ToList(),
                    LoadTime = x.SourceDateTime,
                    DeliveryTime = x.DestinationDateTime,
                    JobNumber = x.Dispatch.OrderLine.JobNumber,
                    ProductOrService = x.Dispatch.OrderLine.Service.Service1,
                    DispatchId = x.DispatchId,
                    OrderLineId = x.Dispatch.OrderLineId
                })
                .OrderBy(x => x.LoadTime)
                .ToListAsync();

            var pages = new List<DriverActivityDetailReportPageDto>();

            foreach (var employeeTime in employeeTimes)
            {
                var date = employeeTime.StartDateTime.ConvertTimeZoneTo(timezone).Date;
                var driver = drivers.FirstOrDefault(x => x.UserId == employeeTime.UserId);
                var page = pages.FirstOrDefault(x => x.Date == date && x.UserId == employeeTime.UserId);
                if (page == null)
                {
                    var driverId = driver?.DriverId ?? 0;
                    page = new DriverActivityDetailReportPageDto
                    {
                        Date = date,
                        DriverId = driverId,
                        DriverName = driver?.DriverName,
                        UserId = employeeTime.UserId,
                        ScheduledStartTime = driverAssignments.FirstOrDefault(x => x.DriverId == driverId && x.Date == date)?.StartTimeUtc?.ConvertTimeZoneTo(timezone),
                        EmployeeTimes = new List<DriverActivityDetailReportEmployeeTimeDto>(),
                        Loads = new List<DriverActivityDetailReportLoadDto>()
                    };
                    pages.Add(page);
                }
                page.EmployeeTimes.Add(new DriverActivityDetailReportEmployeeTimeDto
                {
                    TruckId = employeeTime.TruckId,
                    TruckCode = employeeTime.TruckCode,
                    ClockInTime = employeeTime.StartDateTime,
                    ClockOutTime = employeeTime.EndDateTime,
                    TimeClassificationName = employeeTime.TimeClassificationName
                });
            }

            foreach (var load in loads)
            {
                var page = pages.FirstOrDefault(x => x.Date == load.Date && x.DriverId == load.DriverId);
                if (page == null)
                {
                    var driver = drivers.FirstOrDefault(x => x.DriverId == load.DriverId);
                    page = new DriverActivityDetailReportPageDto
                    {
                        Date = load.Date ?? Clock.Now,
                        DriverId = load.DriverId ?? 0,
                        DriverName = driver?.DriverName,
                        UserId = driver?.UserId ?? 0,
                        ScheduledStartTime = driverAssignments.FirstOrDefault(x => x.DriverId == load.DriverId && x.Date == load.Date)?.StartTimeUtc?.ConvertTimeZoneTo(timezone),
                        EmployeeTimes = new List<DriverActivityDetailReportEmployeeTimeDto>(),
                        Loads = new List<DriverActivityDetailReportLoadDto>(),
                    };
                    pages.Add(page);
                }

                foreach (var ticket in load.Tickets.DefaultIfEmpty())
                {
                    page.Loads.Add(new DriverActivityDetailReportLoadDto
                    {
                        TruckId = load.TruckId,
                        DispatchId = load.DispatchId,
                        OrderLineId = load.OrderLineId,
                        TruckCode = load.TruckCode,
                        CustomerName = load.CustomerName,
                        DeliverTo = load.DeliverTo,
                        DeliveryTime = load.DeliveryTime,
                        LoadAt = load.LoadAt,
                        LoadTime = load.LoadTime,
                        Quantity = ticket?.Quantity,
                        UomName = ticket?.UomName,
                        TicketNumber = ticket?.TicketNumber,
                        JobNumber = load.JobNumber,
                        ProductOrService = load.ProductOrService
                    });
                }
            }

            pages = pages.OrderBy(p => p.Date).ThenBy(p => p.DriverName).ToList();

            foreach (var page in pages)
            {
                //page.Loads = page.Loads.OrderBy(x => x.LoadTime).ToList();

                for (int i = 0; i < page.Loads.Count - 1; i++)
                {
                    if (page.Loads[i + 1].LoadTime.HasValue && page.Loads[i].LoadTime.HasValue)
                    {
                        page.Loads[i].CycleTime = page.Loads[i + 1].LoadTime.Value - page.Loads[i].LoadTime.Value;
                    }
                }

                if (page.Loads.Any())
                {
                    var lastLoad = page.Loads.Last();
                    if (lastLoad.DeliveryTime.HasValue)
                    {
                        var employeeTime = page.EmployeeTimes
                            .Where(x => x.ClockOutTime > lastLoad.DeliveryTime)
                            .OrderBy(x => x.ClockOutTime)
                            //.FirstOrDefault(x => x.ClockInTime <= lastLoad.LoadTime && x.ClockOutTime > lastLoad.LoadTime);
                            .FirstOrDefault();
                        if (lastLoad.LoadTime.HasValue && employeeTime?.ClockOutTime.HasValue == true)
                        {
                            lastLoad.CycleTime = employeeTime.ClockOutTime.Value - lastLoad.LoadTime.Value;
                        }
                    }
                }
            }

            var reportGenerator = new DriverActivityDetailReportGenerator();
            return reportGenerator.GenerateReport(new DriverActivityDetailReportDto
            {
                Timezone = timezone,
                Pages = pages
            });
        }


        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task<SetDispatchTimeOnJobDto> GetDispatchTimeOnJob(int dispatchId)
        {
            var dispatch = await _dispatchRepository.GetAll()
                .Where(d => d.Id == dispatchId)
                .Select(d => new SetDispatchTimeOnJobDto
                {
                    Id = d.Id,
                    TimeOnJob = d.TimeOnJob
                })
                .FirstAsync();

            dispatch.TimeOnJob = dispatch.TimeOnJob?.ConvertTimeZoneTo(await GetTimezone());

            return dispatch;
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task<SetDispatchTimeOnJobDto> SetDispatchTimeOnJob(SetDispatchTimeOnJobDto input)
        {
            var dispatch = await _dispatchRepository.GetAll()
                .Where(d => d.Id == input.Id)
                .FirstAsync();

            var orderLine = await _orderLineRepository.GetAll()
                .Where(ol => ol.Id == dispatch.OrderLineId)
                .Select(ol => new
                {
                    ol.Order.DeliveryDate
                })
                .FirstAsync();

            input.TimeOnJob = input.TimeOnJob == null ? null
                : (orderLine.DeliveryDate ?? await GetToday()).Date.Add(input.TimeOnJob.Value.TimeOfDay);

            dispatch.TimeOnJob = input.TimeOnJob?.ConvertTimeZoneFrom(await GetTimezone());

            await CurrentUnitOfWork.SaveChangesAsync();
            await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatch.DriverId)
            {
                LogMessage = $"Changed TimeOnJob for dispatch {dispatch.Id}"
            });
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatch.ToChangedEntity())
                .AddLogMessage("Changed TimeOnJob for dispatch"));

            return input;
        }

    }
}
