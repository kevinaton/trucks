using System.Threading.Tasks;
using DispatcherWeb.Orders.SendOrdersToDrivers.Dto;

namespace DispatcherWeb.Orders.SendOrdersToDrivers.OrderSenders
{
    public interface ISmsOrderSender
    {
        Task SendAsync(DriverOrderDto driverOrder);
    }
}