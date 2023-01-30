using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders.SendOrdersToDrivers.Dto;
using DispatcherWeb.Orders.SendOrdersToDrivers.OrderSenders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Orders.SendOrdersToDrivers
{
    [AbpAuthorize]
    public class SendOrdersToDriversAppService : DispatcherWebAppServiceBase, ISendOrdersToDriversAppService
    {
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IEmailOrderSender _emailOrderSender;
        private readonly ISmsOrderSender _smsOrderSender;
        private readonly IAppNotifier _appNotifier;
        private readonly ILogger<SendOrdersToDriversAppService> _logger;

        public SendOrdersToDriversAppService(
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Order> orderRepository,
            IEmailOrderSender emailOrderSender,
            ISmsOrderSender smsOrderSender,
            IAppNotifier appNotifier,
            ILogger<SendOrdersToDriversAppService> logger
        )
        {
            _driverAssignmentRepository = driverAssignmentRepository;
            _orderRepository = orderRepository;
            _emailOrderSender = emailOrderSender;
            _smsOrderSender = smsOrderSender;
            _appNotifier = appNotifier;
            _logger = logger;
        }

        [Obsolete("Use dispatcher functionality")]
        [AbpAuthorize(AppPermissions.Pages_SendOrdersToDrivers)]
        public async Task<bool> SendOrdersToDrivers(SendOrdersToDriversInput input)
        {
            await CheckOrdersExistForDate(input.DeliveryDate);

            var driverOrders = await GetDriverOrders(input.DeliveryDate, input.Shift);

            bool result = true;
            bool allowSms = await SettingManager.AllowSmsMessages();
            foreach (var driverOrder in driverOrders)
            {
                if (driverOrder.OrderNotifyPreferredFormat == OrderNotifyPreferredFormat.Email || driverOrder.OrderNotifyPreferredFormat == OrderNotifyPreferredFormat.Both)
                {
                    try
                    {
                        if (driverOrder.EmailAddress.IsNullOrEmpty())
                        {
                            await HandleMessageErrorAsync(driverOrder, "An email", " because the driver is missing an email address", NotificationSeverity.Warn);
                        }
                        else
                        {
                            await _emailOrderSender.SendAsync(driverOrder);
                        }
                    }
                    catch (Exception e)
                    {
                        result = false;
                        await HandleMessageErrorAsync(driverOrder, "An email", ". An unknown error occurred.", NotificationSeverity.Error);
                        _logger.LogError(e, $"An error occurred trying to send an email to {driverOrder.EmailAddress} for {driverOrder.DeliveryDate}");
                    }
                }

                if (allowSms && (driverOrder.OrderNotifyPreferredFormat == OrderNotifyPreferredFormat.Sms || driverOrder.OrderNotifyPreferredFormat == OrderNotifyPreferredFormat.Both))
                {
                    try
                    {
                        if (driverOrder.CellPhoneNumber.IsNullOrEmpty())
                        {
                            await HandleMessageErrorAsync(driverOrder, "An SMS", " because the driver is missing a cell phone number", NotificationSeverity.Warn);
                        }
                        else
                        {
                            await _smsOrderSender.SendAsync(driverOrder);
                        }
                    }
                    catch(UserFriendlyException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        result = false;
                        await HandleMessageErrorAsync(driverOrder, "An SMS", ". An unknown error occurred.", NotificationSeverity.Error);
                        _logger.LogError(e, $"An error occurred trying to send an SMS to {driverOrder.CellPhoneNumber} for {driverOrder.DeliveryDate}");
                    }
                }
            }
            return result;
        }

        private async Task HandleMessageErrorAsync(DriverOrderDto driverOrder, string messageKind, string reason, NotificationSeverity notificationSeverity)
        {
            await _appNotifier.SendMessageAsync(
                Session.ToUserIdentifier(),
                $"{messageKind} to {driverOrder.DriverFullName} for {driverOrder.DeliveryDate.ToShortDateString()} wasn't sent{reason}",
                notificationSeverity);
        }

        private async Task CheckOrdersExistForDate(DateTime deliveryDate)
        {
            bool ordersExistForDate = await _orderRepository.GetAll().Where(o => o.DeliveryDate == deliveryDate).AnyAsync();
            if (!ordersExistForDate)
            {
                throw new UserFriendlyException(L("SendOrders_NoOrders", deliveryDate.ToString("d")));
            }
        }

        private async Task<List<DriverOrderDto>> GetDriverOrders(DateTime deliveryDate, Shift? shift)
        {
            string shiftName = await SettingManager.GetShiftName(shift) ?? "";
            var driverOrders = await _driverAssignmentRepository.GetAll().AsNoTracking()
                .Where(da =>
                    da.Date == deliveryDate &&
                    da.Driver.OrderNotifyPreferredFormat != OrderNotifyPreferredFormat.Neither &&
                    da.Truck.OrderLineTrucks.Any(olt => olt.OrderLine.Order.DeliveryDate == deliveryDate && olt.OrderLine.Order.LocationId == OfficeId)
                )
                .Select(da => new DriverOrderDto
                {
                    DeliveryDate = deliveryDate,
                    ShiftName = shiftName,

                    DriverFirstName = da.Driver.FirstName,
                    DriverLastName = da.Driver.LastName,
                    EmailAddress = da.Driver.EmailAddress,
                    CellPhoneNumber = da.Driver.CellPhoneNumber,
                    OrderNotifyPreferredFormat = da.Driver.OrderNotifyPreferredFormat,

                    TruckCode = da.Truck.TruckCode,
                    Orders = da.Truck.OrderLineTrucks
                        .Where(olt => olt.OrderLine.Order.DeliveryDate == deliveryDate)
                        .Select(olt => new DriverOrderDto.OrderDto
                        {
                            Id = olt.OrderLine.Order.Id,
                            Directions = olt.OrderLine.Order.Directions,
                            OrderTime = olt.TimeOnJob.HasValue ? olt.TimeOnJob : olt.OrderLine.TimeOnJob,

                            OrderLines = olt.OrderLine.Order.OrderLines.Select(ol => new DriverOrderDto.OrderDto.OrderLineDto
                            {
                                Item = ol.Service.Service1,
                                MaterialQuantity = ol.MaterialQuantity,
                                FreightQuantity = ol.FreightQuantity,
                                MaterialUomName = ol.MaterialUom.Name,
                                FreightUomName = ol.FreightUom.Name,
                                LoadAt = ol.LoadAt == null ? null : new LocationNameDto
                                {
                                    Name = ol.LoadAt.Name,
                                    StreetAddress = ol.LoadAt.StreetAddress,
                                    City = ol.LoadAt.City,
                                    State = ol.LoadAt.State
                                },
                                DeliverTo = ol.DeliverTo == null ? null : new LocationNameDto
                                {
                                    Name = ol.DeliverTo.Name,
                                    StreetAddress = ol.DeliverTo.StreetAddress,
                                    City = ol.DeliverTo.City,
                                    State = ol.DeliverTo.State
                                },
                                Note = ol.Note
                            }),
                        }).ToList(),
                })
                .ToListAsync();

            var timezone = await GetTimezone();
            driverOrders.ForEach(order => order.Orders.ForEach(o => o.OrderTime = o.OrderTime?.ConvertTimeZoneTo(timezone)));

            return driverOrders;
        }

    }
}
