﻿using Abp.Events.Bus.Handlers;

namespace DispatcherWeb.MultiTenancy.Payments
{
    public interface ISupportsRecurringPayments :
        IEventHandler<RecurringPaymentsDisabledEventData>,
        IEventHandler<RecurringPaymentsEnabledEventData>,
        IEventHandler<TenantEditionChangedEventData>
    {

    }
}
