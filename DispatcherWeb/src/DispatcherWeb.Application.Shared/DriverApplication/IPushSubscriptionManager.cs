using DispatcherWeb.DriverApplication.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.DriverApplication
{
    public interface IPushSubscriptionManager
    {
        Task AddDriverPushSubscription(AddDriverPushSubscriptionInput input);
        Task RemoveDriverPushSubscription(RemoveDriverPushSubscriptionInput input);
        Task CleanupSubscriptions();
    }
}
