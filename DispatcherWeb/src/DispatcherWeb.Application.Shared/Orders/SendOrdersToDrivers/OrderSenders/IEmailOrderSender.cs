using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Orders.SendOrdersToDrivers.Dto;

namespace DispatcherWeb.Orders.SendOrdersToDrivers.OrderSenders
{
    public interface IEmailOrderSender
	{
		Task SendAsync(DriverOrderDto driverOrder);
    }
}
