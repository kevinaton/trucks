using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Extensions;
using Abp.UI;
using Castle.Core.Logging;
using DispatcherWeb.Configuration;
using DispatcherWeb.Exceptions;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Orders.SendOrdersToDrivers.Dto;

namespace DispatcherWeb.Orders.SendOrdersToDrivers.OrderSenders
{
    public class SmsOrderSender : ISmsOrderSender, ITransientDependency
	{
		private readonly ISmsSender _smsSender;
		private readonly ISettingManager _settingManager;

		public ILogger Logger { get; set; }

		public SmsOrderSender(
			ISmsSender smsSender,
			ISettingManager settingManager
		)
		{
			_smsSender = smsSender;
			_settingManager = settingManager;
		}

		[Obsolete("Use dispatcher functionality")]
		public async Task SendAsync(DriverOrderDto driverOrder)
		{
			if(String.IsNullOrEmpty(driverOrder.CellPhoneNumber))
			{
				throw new ApplicationException("Email Address is null or empty!");
			}

			string smsTemplate = await _settingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.Sms);
			if (smsTemplate.IsNullOrEmpty())
			{
				throw new UserFriendlyException("The \"Driver Order SMS\" template is empty!");
			}
			Regex orderRegex = new Regex(@"\[order](.+)\[/order]", RegexOptions.IgnoreCase);
			string driverTemplate = orderRegex.Replace(smsTemplate, String.Empty);

			var orderMatch = orderRegex.Match(smsTemplate);
			string orderTemplate = orderMatch.Length != 0 ? orderMatch.Groups[1].Value : "";
			Regex itemRegex = new Regex(@"\[item](.+)\[/item]", RegexOptions.IgnoreCase);
			orderTemplate = itemRegex.Replace(orderTemplate, String.Empty);

			var itemMatch = itemRegex.Match(smsTemplate);
			string itemTemplate = itemMatch.Length != 0 ? itemMatch.Groups[1].Value : "";

			StringBuilder smsBuilder = new StringBuilder();
			smsBuilder.Append(ReplaceTemplateVariables(driverTemplate, driverOrder));
			int orderNumber = 1;
			foreach (var order in driverOrder.Orders)
			{
				smsBuilder.Append(ReplaceTemplateVariables(orderTemplate, order, orderNumber));
				foreach (var orderLine in order.OrderLines)
				{
					smsBuilder.Append(ReplaceTemplateVariables(itemTemplate, orderLine));
				}
				orderNumber++;
			}

			string smsText = smsBuilder.ToString();

			var smsSendResult = await _smsSender.SendAsync(smsText, driverOrder.CellPhoneNumber);
			if (smsSendResult.ErrorCode.HasValue)
			{
				Logger.Error(smsSendResult.ErrorMessage);
			}
		}

		private string ReplaceTemplateVariables(string templateString, DriverOrderDto driverOrder)
		{
			var varReplacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{"DeliveryDate", driverOrder.DeliveryDate.ToString("d")},
			};
			return Utilities.ReplaceTemplateVariables(templateString, varReplacements);
		}

		private string ReplaceTemplateVariables(string templateString, DriverOrderDto.OrderDto order, int orderNumber)
		{
			var varReplacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{"OrderNumber", orderNumber.ToString("N0")},
                {"Directions", order.Directions},
                {"Comments", order.Directions},
				{"TimeOnJob", order.OrderTime?.ToString("F") ?? ""},
			};
			return Utilities.ReplaceTemplateVariables(templateString, varReplacements);
		}

		private string ReplaceTemplateVariables(string templateString, DriverOrderDto.OrderDto.OrderLineDto orderLine)
		{
			var varReplacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{"Item", orderLine.Item},
				{"UOM", orderLine.MaterialUomName},
                {"MaterialUom", orderLine.MaterialUomName},
                {"FreightUom", orderLine.FreightUomName},
                {"LoadAt", orderLine.LoadAtName},
                {"DeliverTo", orderLine.DeliverToName},
				{"Quantity", orderLine.MaterialQuantity?.ToString("F") ?? ""},
                {"MaterialQuantity", orderLine.MaterialQuantity?.ToString("F") ?? ""},
                {"FreightQuantity", orderLine.FreightQuantity?.ToString("F") ?? ""},
				{"Note", orderLine.Note },
            };
			return Utilities.ReplaceTemplateVariables(templateString, varReplacements);
		}

        
    }
}
