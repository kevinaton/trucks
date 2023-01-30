using Abp;
using DispatcherWeb.DriverApplication.Dto;

namespace DispatcherWeb.BackgroundJobs
{
    public class DriverApplicationPushSenderBackgroundJobArgs : SendPushMessageToDriversInput
    {
        public DriverApplicationPushSenderBackgroundJobArgs()
        {
        }

        public DriverApplicationPushSenderBackgroundJobArgs(SendPushMessageToDriversInput other)
        {
            DriverIds = other.DriverIds;
            LogMessage = other.LogMessage;
            Message = other.Message;
        }

        public int TenantId { get; set; }
        public UserIdentifier RequestorUser { get; set; }
    }
}
