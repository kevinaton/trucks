using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Runtime.Session;
using Castle.Core.Logging;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.WebPush;
using Microsoft.EntityFrameworkCore;
using WebPushLib = WebPush;

namespace DispatcherWeb.DriverApplication
{
    public class DriverApplicationPushSender : IDriverApplicationPushSender, ITransientDependency
    {
        private readonly IRepository<DriverPushSubscription> _driverPushSubscriptionRepository;
        private readonly IRepository<PushSubscription> _pushSubscriptionRepository;
        private readonly IWebPushSender _webPushSender;
        private readonly IDriverApplicationLogger _driverApplicationLogger;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IAbpSession _abpSession;
        private readonly ILogger _logger;

        public DriverApplicationPushSender(
            IRepository<DriverPushSubscription> driverPushSubscriptionRepository,
            IRepository<PushSubscription> pushSubscriptionRepository,
            IWebPushSender webPushSender,
            IDriverApplicationLogger driverApplicationLogger,
            IBackgroundJobManager backgroundJobManager,
            IAbpSession abpSession,
            ILogger logger
            )
        {
            _driverPushSubscriptionRepository = driverPushSubscriptionRepository;
            _pushSubscriptionRepository = pushSubscriptionRepository;
            _webPushSender = webPushSender;
            _driverApplicationLogger = driverApplicationLogger;
            _backgroundJobManager = backgroundJobManager;
            _abpSession = abpSession;
            _logger = logger;
        }

        public async Task SendPushMessageToDrivers(SendPushMessageToDriversInput input)
        {
            var jobArgs = new DriverApplicationPushSenderBackgroundJobArgs(input)
            {
                TenantId = _abpSession.GetTenantId(),
                RequestorUser = _abpSession.ToUserIdentifier()
            };
            await _backgroundJobManager.EnqueueAsync<DriverApplicationPushSenderBackgroundJob, DriverApplicationPushSenderBackgroundJobArgs>(jobArgs);
        }

        public async Task<bool> SendPushMessageToDriversImmediately(SendPushMessageToDriversInput input)
        {
            var pushSubs = await _driverPushSubscriptionRepository.GetAll()
                .Where(x => input.DriverIds.Contains(x.DriverId))
                .Select(x => new
                {
                    x.DriverId,
                    x.PushSubscription
                })
                .AsNoTracking()
                .ToListAsync();

            var result = true;
            var processedDrivers = new List<int>();

            foreach (var pushSub in pushSubs)
            {
                var guid = Guid.NewGuid();
                try
                {
                    await _webPushSender.SendAsync(pushSub.PushSubscription.ToDto(), new DriverApplication.PwaPushMessage //TODO update to send a SyncRequestPushMessage with an additional Changes field
                    {
                        Message = input.Message,
                        Action = DriverApplicationPushAction.SilentSync,
                        Guid = guid
                    });
                    if (!input.LogMessage.IsNullOrEmpty())
                    {
                        await _driverApplicationLogger.LogInfo(pushSub.DriverId, $"[Dispatcher] Sending push {guid};{pushSub.PushSubscription.Id:D7}; {input.LogMessage}");
                    }
                    else
                    {
                        await _driverApplicationLogger.LogInfo(pushSub.DriverId, $"[Dispatcher] Sending push {guid};{pushSub.PushSubscription.Id:D7};");
                    }
                    processedDrivers.Add(pushSub.DriverId); //for these drivers we won't show a "no push" log message
                }
                catch (WebPushLib.WebPushException exception)
                {
                    switch (exception.StatusCode)
                    {
                        case HttpStatusCode.Gone:
                        case HttpStatusCode.NotFound:
                            _logger.Warn($"DriverApplicationPushSender: subscription gone, http status code {exception.StatusCode}, message {exception.Message}", exception);
                            _logger.Warn($"DriverApplicationPushSender: removing subscription {pushSub.PushSubscription.Id}");
                            await _driverApplicationLogger.LogWarn(pushSub.DriverId, $"[Dispatcher] Sending push {guid};{pushSub.PushSubscription.Id:D7} failed, subscription gone");
                            await _driverPushSubscriptionRepository.DeleteAsync(x => x.PushSubscriptionId == pushSub.PushSubscription.Id);
                            await _pushSubscriptionRepository.DeleteAsync(pushSub.PushSubscription.Id);
                            break;
                        default:
                            _logger.Error($"DriverApplicationPushSender: error, http status code {exception.StatusCode}, message {exception.Message}", exception);
                            await _driverApplicationLogger.LogError(pushSub.DriverId, $"[Dispatcher] Sending push {guid};{pushSub.PushSubscription.Id:D7} failed, error: {exception.Message}; status code {exception.StatusCode}");
                            break;
                    }
                    result = false;
                }
            }

            foreach (var driverId in input.DriverIds)
            {
                if (!processedDrivers.Contains(driverId))
                {
                    await _driverApplicationLogger.LogWarn(driverId, $"[Dispatcher] No push subscription found. {input.LogMessage}");
                    result = false;
                }
            }

            return result;
        }
    }
}
