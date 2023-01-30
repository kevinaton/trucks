using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Net.Mail;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Templates;
using DispatcherWeb.Orders.SendOrdersToDrivers.Dto;

namespace DispatcherWeb.Orders.SendOrdersToDrivers.OrderSenders
{
	public class EmailOrderSender : IEmailOrderSender, ITransientDependency
	{
		private readonly IEmailSender _emailSender;
		private readonly ISettingManager _settingManager;
		private readonly IOrderAppService _orderAppService;

		public EmailOrderSender(
			IEmailSender emailSender,
			ISettingManager settingManager,
			IOrderAppService orderAppService
		)
		{
			_emailSender = emailSender;
			_settingManager = settingManager;
			_orderAppService = orderAppService;
		}

		public async Task SendAsync(DriverOrderDto driverOrder)
		{
			if (String.IsNullOrEmpty(driverOrder.EmailAddress))
			{
				throw new ApplicationException("Email Address is null or empty!");
			}

			using (var pdfStream = new MemoryStream())
			{
				//OrderPdfCreator.CreatePdf(driverOrder, pdfStream);
				var report = await _orderAppService.GetWorkOrderReport(new Orders.Dto.GetWorkOrderReportInput
				{
					HidePrices = true,
					Ids = driverOrder.Orders.Select(x => x.Id).ToList()
				});
				report.SaveToMemoryStream(pdfStream);

				MailMessage message = new MailMessage(
					await _settingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress),
					driverOrder.EmailAddress
				);
				message.Subject = ReplaceTemplateVariables(
					await _settingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.EmailTitle),
					driverOrder
				);
				message.Body = ReplaceTemplateVariables(
					await _settingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.EmailBody),
					driverOrder
				);

				Attachment pdfAttachment = new Attachment(pdfStream, new ContentType(MediaTypeNames.Application.Pdf));
				pdfAttachment.Name = $"Orders_{driverOrder.DeliveryDate:yyyy-MM-dd}.pdf";
				message.Attachments.Add(pdfAttachment);

				await _emailSender.SendAsync(message);
			}
		}

		private string ReplaceTemplateVariables(string templateString, DriverOrderDto driverOrder) =>
			templateString.ReplaceTokensInTemplate(new TemplateTokenDto()
			{
				DeliveryDate = driverOrder.DeliveryDate.ToString("d"),
				Shift = driverOrder.ShiftName,
			});
	}
}
