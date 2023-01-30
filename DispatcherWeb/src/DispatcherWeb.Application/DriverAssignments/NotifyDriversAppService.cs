using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Net.Mail;
using Abp.Notifications;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Notifications;
using Microsoft.EntityFrameworkCore;
using Twilio.Exceptions;

namespace DispatcherWeb.DriverAssignments
{
	[AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
    public class NotifyDriversAppService : DispatcherWebAppServiceBase, INotifyDriversAppService
	{
		private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
		private readonly IEmailSender _emailSender;
		private readonly ISmsSender _smsSender;
		private readonly IAppNotifier _appNotifier;

		public NotifyDriversAppService(
			IRepository<DriverAssignment> driverAssignmentRepository,
			IEmailSender emailSender,
			ISmsSender smsSender,
			IAppNotifier appNotifier
		)
		{
			_driverAssignmentRepository = driverAssignmentRepository;
			_emailSender = emailSender;
			_smsSender = smsSender;
			_appNotifier = appNotifier;
		}

		public async Task<bool> NotifyDrivers(NotifyDriversInput input)
		{
			var driversToNofify = await GetDriversToNotify();

			bool result = true;
			foreach(var driver in driversToNofify)
			{
				result = result && await SendEmailToDriver(driver);

				result = result && await SendSmsToDriver(driver);
			}
			return result;

			// *** Local functions ***
			async Task<List<NotifyDriverDto>> GetDriversToNotify()
			{
				var drivers = await GetDriverAssignmentQuery(input)
					.Select(da => new NotifyDriverDto
					{
						TruckCode = da.Truck.TruckCode,
						DriverFullName = da.Driver.FirstName + " " + da.Driver.LastName,
						StartTime = da.StartTime,
						OrderNotifyPreferredFormat = da.Driver.OrderNotifyPreferredFormat,
						EmailAddress = da.Driver.EmailAddress,
						CellPhoneNumber = da.Driver.CellPhoneNumber
					})
					.ToListAsync();

				var timezone = await GetTimezone();
				drivers.ForEach(x => x.StartTime = x.StartTime?.ConvertTimeZoneTo(timezone));

				return drivers;
			}

			async Task<bool> SendEmailToDriver(NotifyDriverDto driver)
			{
				bool success = true;
				if(driver.OrderNotifyPreferredFormat == OrderNotifyPreferredFormat.Email ||
					driver.OrderNotifyPreferredFormat == OrderNotifyPreferredFormat.Both)
				{
					try
					{
						if(driver.EmailAddress.IsNullOrEmpty())
						{
							await SendEmptyEmailNotification(driver.DriverFullName);
							success = false;
						}
						else
						{
							await SendEmail(driver);
						}
					}
					catch(Exception e)
					{
						success = false;
						await SendUnknownEmailErrorNotification(driver.DriverFullName);
						Logger.Error($"An error occurred trying to send an email to {driver.EmailAddress} for {input.Date}", e);
					}
				}
				return success;
			}

			async Task<bool> SendSmsToDriver(NotifyDriverDto driver)
			{
				bool success = true;
				if(driver.OrderNotifyPreferredFormat == OrderNotifyPreferredFormat.Sms ||
					driver.OrderNotifyPreferredFormat == OrderNotifyPreferredFormat.Both)
				{
					try
					{
						if(driver.CellPhoneNumber.IsNullOrEmpty())
						{
							await SendEmptySmsNotification(driver.DriverFullName);
							success = false;
						}
						else
						{
							await SendSms(driver);
						}
					}
					catch(ApiException e)
					{
						success = false;
						switch(e.Code)
						{
							case 21211:
								await SendSmsInvalidNumberErrorNotification(driver.DriverFullName);
								break;
							default:
								await SendSmsErrorNotification(driver.DriverFullName, $". {e.Message}");
								break;
						}

						Logger.Error($"An error occurred trying to send an SMS to {driver.CellPhoneNumber} for {input.Date}", e);
					}
					catch(Exception e)
					{
						success = false;
						await SendUnknownSmsErrorNotification(driver.DriverFullName);
						Logger.Error($"An error occurred trying to send an SMS to {driver.CellPhoneNumber} for {input.Date}", e);
					}
				}
				return success;
			}

			async Task<string> GetDriverStartTimeTemplate() => await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DriverStartTimeTemplate);

			async Task SendEmail(NotifyDriverDto notifyDriverDto)
			{
				MailMessage message = new MailMessage(
					await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress),
					notifyDriverDto.EmailAddress
				);
				message.Subject = "Start Time";
				message.Body = await ReplaceTokens(notifyDriverDto);
				Logger.Info($"Sending an email to '{message.To}' address with body: '{message.Body}'");
				await _emailSender.SendAsync(message);
			}

			async Task SendSms(NotifyDriverDto notifyDriverDto)
			{
				string message = await ReplaceTokens(notifyDriverDto);
				string phone = notifyDriverDto.CellPhoneNumber;
				Logger.Info($"Sending an SMS to '{phone}' number with text: '{message}'");
				var smsSendResult = await _smsSender.SendAsync(message, phone);
				if(smsSendResult.ErrorCode.HasValue)
				{
					Logger.Error(smsSendResult.ErrorMessage);
				}

			}

			async Task<string> ReplaceTokens(NotifyDriverDto notifyDriverDto) =>
				(await GetDriverStartTimeTemplate()).ReplaceTokens(
					new Dictionary<string, string>
					{
						{ TemplateTokens.Truck, notifyDriverDto.TruckCode },
						{ TemplateTokens.Driver, notifyDriverDto.DriverFullName },
						{ TemplateTokens.StartTime, notifyDriverDto.StartTime?.ToString("t") ?? "" },
						{ TemplateTokens.StartDate, input.Date.ToShortDateString() },
					}
				);

			async Task SendEmptyEmailNotification(string driverFullName) =>
				await SendAppNotification("An email", " because the driver is missing an email address.", driverFullName, NotificationSeverity.Warn);
			async Task SendUnknownEmailErrorNotification(string driverFullName) =>
				await SendAppNotification("An email", ". An unknown error occurred.", driverFullName, NotificationSeverity.Error);
			async Task SendEmptySmsNotification(string driverFullName) =>
				await SendAppNotification("An SMS", " because the driver is missing a cell phone number.", driverFullName, NotificationSeverity.Warn);
			async Task SendUnknownSmsErrorNotification(string driverFullName) =>
				await SendAppNotification("An SMS", ". An unknown error occurred.", driverFullName, NotificationSeverity.Error);
			async Task SendSmsErrorNotification(string driverFullName, string reason) =>
				await SendAppNotification("An SMS", reason, driverFullName, NotificationSeverity.Error);
			async Task SendSmsInvalidNumberErrorNotification(string driverFullName) =>
				await SendAppNotificationToUser($"An SMS wasn't sent to driver {driverFullName} because the driver has a bad phone number.", NotificationSeverity.Error);
			async Task SendAppNotification(string messageType, string reason, string driverFullName, NotificationSeverity notificationSeverity)
			{
				await SendAppNotificationToUser($"{messageType} to {driverFullName} for {input.Date.ToShortDateString()} wasn't sent{reason}", notificationSeverity);
			}
			async Task SendAppNotificationToUser(string message, NotificationSeverity notificationSeverity)
			{
				await _appNotifier.SendMessageAsync(
					Session.ToUserIdentifier(),
					message,
					notificationSeverity);
			}

		}

        private IQueryable<DriverAssignment> GetDriverAssignmentQuery(NotifyDriversInput input) =>
            _driverAssignmentRepository.GetAll()
                .Where(da => da.DriverId.HasValue &&
                             da.Date == input.Date &&
                             da.Shift == input.Shift &&
                             da.OfficeId == input.OfficeId &&
                             da.StartTime.HasValue &&
                             da.Driver.OrderNotifyPreferredFormat != OrderNotifyPreferredFormat.Neither
                );

        public async Task<bool> ThereAreDriversToNotify(NotifyDriversInput input) => 
            await GetDriverAssignmentQuery(input).AnyAsync();

        private static class TemplateTokens
		{
			public const string Truck = "{Truck}";
			public const string Driver = "{Driver}";
			public const string StartTime = "{StartTime}";
			public const string StartDate = "{StartDate}";
		}

		private class NotifyDriverDto
		{
			public string TruckCode { get; set; }
			public string DriverFullName { get; set; }
			public OrderNotifyPreferredFormat OrderNotifyPreferredFormat { get; set; }
			public string EmailAddress { get; set; }
			public string CellPhoneNumber { get; set; }
			public DateTime? StartTime { get; set; }
		}





	}
}
