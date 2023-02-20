﻿using Abp.RealTime;
using DispatcherWeb.SyncRequests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.SignalR
{
    public interface ISignalRCommunicator
    {
        Task SendSyncRequest(IReadOnlyList<IOnlineClient> clients, SyncRequest syncRequest);
    }
}
