using System;
using System.Threading.Tasks;
using DispatcherWeb.Orders.SendOrdersToDrivers.Dto;

namespace DispatcherWeb.Orders.SendOrdersToDrivers
{
	public interface ISendOrdersToDriversAppService
	{
		Task<bool> SendOrdersToDrivers(SendOrdersToDriversInput input);
	}
}