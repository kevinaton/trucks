using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApp.Dispatches.Dto;
using DispatcherWeb.DriverApp.Loads.Dto;
using DispatcherWeb.DriverApp.Locations.Dto;
using DispatcherWeb.DriverApp.Tickets.Dto;
using DispatcherWeb.SyncRequests;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.Dispatches
{
    [AbpAuthorize]
    public class DispatchAppService : DispatcherWebDriverAppAppServiceBase, IDispatchAppService
    {
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IDispatchingAppService _dispatchingAppService;

        public DispatchAppService(
            IRepository<Dispatch> dispatchRepository,
            ISyncRequestSender syncRequestSender,
            DispatcherWeb.Dispatching.IDispatchingAppService dispatchingAppService
            )
        {
            _dispatchRepository = dispatchRepository;
            _syncRequestSender = syncRequestSender;
            _dispatchingAppService = dispatchingAppService;
        }

        public async Task<IPagedResult<DispatchDto>> Get(GetInput input)
        {
            var allowProductionPay = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay, Session.GetTenantId());

            var query = _dispatchRepository.GetAll()
                .Where(x => x.Driver.UserId == Session.UserId)
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                .WhereIf(input.Id == null, x => Dispatch.OpenStatuses.Contains(x.Status) || x.Status == DispatchStatus.Completed)
                .WhereIf(input.TruckId.HasValue, x => x.TruckId == input.TruckId)
                .WhereIf(input.OrderDateBegin.HasValue, x => x.OrderLine.Order.DeliveryDate >= input.OrderDateBegin)
                .WhereIf(input.OrderDateEnd.HasValue, x => x.OrderLine.Order.DeliveryDate <= input.OrderDateEnd)
                .WhereIf(input.ModifiedAfterDateTime.HasValue, d => d.CreationTime > input.ModifiedAfterDateTime.Value || (d.LastModificationTime != null && d.LastModificationTime > input.ModifiedAfterDateTime.Value))
                .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                    .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                    .ThenByDescending(d => d.Status == DispatchStatus.Sent)
                    .ThenBy(d => d.SortOrder)
                .Select(di => new DispatchDto
                {
                    Id = di.Id,
                    TenantId = di.TenantId,
                    CustomerName = di.OrderLine.Order.Customer.Name,
                    CustomerContact = di.OrderLine.Order.CustomerContact == null ? null : new CustomerContactDto
                    {
                        Name = di.OrderLine.Order.CustomerContact.Name,
                        PhoneNumber = di.OrderLine.Order.CustomerContact.PhoneNumber
                    },
                    OrderDate = di.OrderLine.Order.DeliveryDate.Value,
                    Shift = di.OrderLine.Order.Shift,
                    Status = di.Status,
                    Designation = di.OrderLine.Designation,
                    Item = di.OrderLine.Service.Service1,
                    LoadAt = di.OrderLine.LoadAt == null ? null : new LocationDto
                    {
                        Name = di.OrderLine.LoadAt.Name,
                        Latitude = di.OrderLine.LoadAt.Latitude,
                        Longitude = di.OrderLine.LoadAt.Longitude,
                        AddressObject = new LocationAddressDto
                        {
                            StreetAddress = di.OrderLine.LoadAt.StreetAddress,
                            City = di.OrderLine.LoadAt.City,
                            State = di.OrderLine.LoadAt.State,
                            ZipCode = di.OrderLine.LoadAt.ZipCode,
                            CountryCode = di.OrderLine.LoadAt.CountryCode
                        },
                    },
                    DeliverTo = di.OrderLine.DeliverTo == null ? null : new LocationDto
                    {
                        Name = di.OrderLine.DeliverTo.Name,
                        Latitude = di.OrderLine.DeliverTo.Latitude,
                        Longitude = di.OrderLine.DeliverTo.Longitude,
                        AddressObject = new LocationAddressDto
                        {
                            StreetAddress = di.OrderLine.DeliverTo.StreetAddress,
                            City = di.OrderLine.DeliverTo.City,
                            State = di.OrderLine.DeliverTo.State,
                            ZipCode = di.OrderLine.DeliverTo.ZipCode,
                            CountryCode = di.OrderLine.DeliverTo.CountryCode
                        },
                    },
                    CustomerNotification = di.OrderLine.RequiresCustomerNotification ? new CustomerNotificationDto()
                    {
                        ContactName = di.OrderLine.CustomerNotificationContactName,
                        PhoneNumber = di.OrderLine.CustomerNotificationPhoneNumber,
                    } : null,
                    MaterialQuantity = di.OrderLine.MaterialQuantity,
                    FreightQuantity = di.OrderLine.FreightQuantity,
                    JobNumber = di.OrderLine.JobNumber,
                    Note = di.Note,
                    IsCOD = di.OrderLine.Order.Customer.IsCod,
                    ChargeTo = di.OrderLine.Order.ChargeTo,
                    MaterialUOM = di.OrderLine.MaterialUom.Name,
                    FreightUOM = di.OrderLine.FreightUom.Name,
                    LastModifiedDateTime = di.LastModificationTime.HasValue && di.LastModificationTime.Value > di.CreationTime ? di.LastModificationTime.Value : di.CreationTime,
                    ProductionPay = allowProductionPay && di.OrderLine.ProductionPay,
                    TimeOnJob = di.TimeOnJob,
                    IsMultipleLoads = di.IsMultipleLoads,
                    Loads = di.Loads.Select(l => new LoadDto
                    {
                        Id = l.Id,
                        DispatchId = l.DispatchId,
                        SourceDateTime = l.SourceDateTime,
                        SourceLatitude = l.SourceLatitude,
                        SourceLongitude = l.SourceLongitude,
                        DestinationDateTime = l.DestinationDateTime,
                        DestinationLatitude = l.DestinationLatitude,
                        DestinationLongitude = l.DestinationLongitude,
                        SignatureId = l.SignatureId,
                        SignatureName = l.SignatureName,
                    }).ToList(),
                    Tickets = di.Loads.SelectMany(l => l.Tickets).Select(t => new TicketDto
                    {
                        Id = t.Id,
                        LoadId = t.LoadId,
                        DispatchId = di.Id,
                        Quantity = t.Quantity,
                        TicketDateTime = t.TicketDateTime,
                        TicketNumber = t.TicketNumber,
                        TicketPhotoId = t.TicketPhotoId,
                        TicketPhotoFilename = t.TicketPhotoFilename,
                    }).ToList(),
                    TruckId = di.TruckId,
                    TruckCode = di.Truck.TruckCode,
                    OrderLineTruckId = di.OrderLineTruckId,
                    //WasMultipleLoads = di.WasMultipleLoads,
                    //SignatureId = di.Loads.OrderByDescending(l => l.Id).Select(l => l.SignatureId).FirstOrDefault(), //.DefaultIfEmpty((Guid?)null)
                    //Loaded = di.Loads.OrderByDescending(l => l.Id).Select(l => l.SourceDateTime).FirstOrDefault(),
                    AcknowledgedDateTime = di.Acknowledged,
                    //HasTickets = di.Loads.Any(l => l.TicketId != null),
                    //Guid = di.Guid,
                    SortOrder = di.SortOrder,
                    //NumberOfAddedLoads = di.NumberOfAddedLoads,
                    //NumberOfLoadsToFinish = di.NumberOfLoadsToFinish,
                });

            var totalCount = await query.CountAsync();
            var items = await query
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<DispatchDto>(
                totalCount,
                items);
        }

        public async Task<DispatchDto> Post(DispatchDto model)
        {
            var dispatch = await _dispatchRepository.GetAsync(model.Id);

            if (dispatch == null)
            {
                throw new UserFriendlyException($"Dispatch with id {model.Id} wasn't found");
            }

            if (!await _dispatchRepository.GetAll().AnyAsync(x => x.Id == model.Id && x.Driver.UserId == Session.UserId))
            {
                throw new UserFriendlyException($"You cannot modify dispatches assigned to other users");
            }

            var oldDispatchStatus = dispatch.Status;

            dispatch.Status = model.Status;
            dispatch.Acknowledged = model.AcknowledgedDateTime;
            dispatch.IsMultipleLoads = model.IsMultipleLoads;

            if (dispatch.Status != oldDispatchStatus)
            {
                if (dispatch.Status == DispatchStatus.Completed)
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                    await _dispatchingAppService.RunPostDispatchCompletionLogic(model.Id);
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            //todo send notifications etc, add status validation if needed
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatch.ToChangedEntity())
                //.SetIgnoreForDeviceId(input.DeviceId) //TODO add DeviceId to all DriverApp requests
                .AddLogMessage("Dispatch was updated from Driver App"));

            return (await Get(new GetInput { Id = model.Id })).Items.FirstOrDefault();
        }
    }
}
