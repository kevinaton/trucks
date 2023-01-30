using DispatcherWeb.DriverApplication.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.DriverApplication
{
    public interface IDriverApplicationPushSender
    {
        Task SendPushMessageToDrivers(SendPushMessageToDriversInput input);
        Task<bool> SendPushMessageToDriversImmediately(SendPushMessageToDriversInput input);
    }
}
