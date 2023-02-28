using System.Threading.Tasks;
using DispatcherWeb.DriverApplication.Dto;

namespace DispatcherWeb.DriverApplication
{
    public interface IDriverApplicationPushSender
    {
        Task SendPushMessageToDrivers(SendPushMessageToDriversInput input);
        Task<bool> SendPushMessageToDriversImmediately(SendPushMessageToDriversInput input);
    }
}
