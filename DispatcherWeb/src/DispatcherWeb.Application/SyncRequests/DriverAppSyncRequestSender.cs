using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Castle.Core.Logging;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.SignalR;
using DispatcherWeb.SignalR.Entities;
using DispatcherWeb.SyncRequests.FcmPushMessages;
using DispatcherWeb.SyncRequests.FcmPushMessages.ChangeDetails;
using DispatcherWeb.WebPush;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DispatcherWeb.SyncRequests
{
    public class DriverAppSyncRequestSender : IDriverAppSyncRequestSender, ITransientDependency
    {
        public static EntityEnum[] EntityTypesToIgnore = new[]
        {
            EntityEnum.DriverAssignment,
        };

        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<FcmRegistrationToken> _fcmRegistrationTokenRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<FcmPushMessage, Guid> _fcmPushMessageRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IBackgroundJobManager _backgroundJobManager;

        public DriverAppSyncRequestSender(
            IRepository<Driver> driverRepository,
            IRepository<FcmRegistrationToken> fcmRegistrationTokenRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<FcmPushMessage, Guid> fcmPushMessageRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IBackgroundJobManager backgroundJobManager
            )
        {
            _driverRepository = driverRepository;
            _fcmRegistrationTokenRepository = fcmRegistrationTokenRepository;
            _dispatchRepository = dispatchRepository;
            _fcmPushMessageRepository = fcmPushMessageRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _backgroundJobManager = backgroundJobManager;
        }

        public AspNetZeroAbpSession Session { get; set; }

        public ILogger Logger { get; set; }

        protected IActiveUnitOfWork CurrentUnitOfWork => _unitOfWorkManager.Current;

        /// <summary>
        /// Schedules a new background job to send FCM push message to affected users/drivers
        /// </summary>
        /// <param name="syncRequest"></param>
        /// <returns></returns>
        public async Task SendSyncRequestAsync(SyncRequest syncRequest)
        {
            var newFcmPushMessages = new List<(FcmPushMessage pushMessage, EntityEnum entityType)>();

            var driverIds = GetDriverIds(syncRequest);
            if (!driverIds.Any())
            {
                return;
            }

            var drivers = await _driverRepository.GetAll()
                .Where(x => driverIds.Contains(x.Id) && x.UserId.HasValue)
                .Select(x => new
                {
                    DriverId = x.Id,
                    x.UserId
                }).ToListAsync();

            if (!drivers.Any())
            {
                return;
            }

            var userIds = drivers.Select(x => x.UserId).Distinct().ToList();
            var fcmTokens = await _fcmRegistrationTokenRepository.GetAll()
                .Where(x => userIds.Contains(x.UserId))
                .Select(x => new FcmRegistrationTokenDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Token = x.Token,
                    MobilePlatform = x.MobilePlatform
                })
                .ToListAsync();

            if (!fcmTokens.Any())
            {
                return;
            }

            var syncRequestChanges = syncRequest.Changes.ToList();

            //Dispatches need an additional DeliveryDate field in the change details
            var dispatches = syncRequestChanges
                .OfType<SyncRequestChangeDetail<ChangedDispatch>>()
                .SelectMany(x => GetDriverIds(x.Entity).Select(driverId => new
                {
                    DriverId = driverId,
                    UserId = drivers.FirstOrDefault(x => x.DriverId == driverId)?.UserId,
                    x.Entity.Id,
                    x.ChangeType
                }))
                .Where(x => x.UserId.HasValue)
                .ToList();

            syncRequestChanges.RemoveAll(x => x is SyncRequestChangeDetail<ChangedDispatch>);

            if (dispatches.Any())
            {
                var dispatchIds = dispatches.Select(x => x.Id).Distinct().ToList();
                var dispatchesDetails = await _dispatchRepository.GetAll()
                    .Where(x => dispatchIds.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.OrderLine.Order.DeliveryDate,
                    }).ToListAsync();

                foreach (var dispatchesOfUser in dispatches.Where(x => x.UserId.HasValue).GroupBy(x => x.UserId.Value))
                {
                    var userId = dispatchesOfUser.Key;
                    foreach (var fcmToken in fcmTokens.Where(x => x.UserId == userId))
                    {
                        var pushMessage = new ReloadSpecificEntitiesPushMessage
                        {
                            EntityType = EntityEnum.Dispatch
                        };

                        foreach (var dispatch in dispatchesOfUser)
                        {
                            pushMessage.Changes.Add(new FcmDispatchDetailsDto
                            {
                                Id = dispatch.Id,
                                ChangeType = dispatch.ChangeType,
                                DeliveryDate = dispatchesDetails.FirstOrDefault(x => x.Id == dispatch.Id)?.DeliveryDate?.ToString("yyyy-MM-dd")
                            });
                        }

                        var pushMessageJson = JsonConvert.SerializeObject(pushMessage);

                        newFcmPushMessages.Add((new FcmPushMessage
                        {
                            Id = pushMessage.Guid,
                            TenantId = Session.TenantId,
                            FcmRegistrationTokenId = fcmToken.Id,
                            ReceiverUserId = userId,
                            ReceiverDriverId = dispatchesOfUser.FirstOrDefault()?.DriverId,
                            JsonPayload = pushMessageJson,
                        }, pushMessage.EntityType));
                    }
                }
            }

            //Simple entity types that don't require additional fields
            foreach (var entityTypeGroup in syncRequestChanges.GroupBy(x => x.EntityType))
            {
                var entityType = entityTypeGroup.Key;
                if (EntityTypesToIgnore.Contains(entityType))
                {
                    continue;
                }
                var changeDetails = entityTypeGroup
                    .OfType<ISyncRequestChangeDetail>()
                    .Where(x => x.Entity is ChangedDriverAppEntity<int>)
                    .SelectMany(x => GetDriverIds((ChangedDriverAppEntity<int>)x.Entity).Select(driverId => new
                    {
                        DriverId = driverId,
                        UserId = drivers.FirstOrDefault(x => x.DriverId == driverId)?.UserId,
                        ((ChangedDriverAppEntity<int>)x.Entity).Id,
                        x.ChangeType
                    }))
                    .Where(x => x.UserId.HasValue)
                    .ToList();

                foreach (var changeDetailsOfUser in changeDetails.Where(x => x.UserId.HasValue).GroupBy(x => x.UserId.Value))
                {
                    var userId = changeDetailsOfUser.Key;
                    foreach (var fcmToken in fcmTokens.Where(x => x.UserId == userId))
                    {
                        var pushMessage = new ReloadSpecificEntitiesPushMessage
                        {
                            EntityType = entityType
                        };

                        foreach (var changeDetail in changeDetailsOfUser)
                        {
                            pushMessage.Changes.Add(new FcmEntityChangeDetailsDto<int>
                            {
                                Id = changeDetail.Id,
                                ChangeType = changeDetail.ChangeType,
                            });
                        }

                        var pushMessageJson = JsonConvert.SerializeObject(pushMessage);

                        newFcmPushMessages.Add((new FcmPushMessage
                        {
                            Id = pushMessage.Guid,
                            TenantId = Session.TenantId,
                            FcmRegistrationTokenId = fcmToken.Id,
                            ReceiverUserId = userId,
                            ReceiverDriverId = changeDetailsOfUser.FirstOrDefault()?.DriverId,
                            JsonPayload = pushMessageJson,
                        }, pushMessage.EntityType));
                    }
                }
            }


            //Replace ReloadSpecificEntities with ReloadAllEntities messages if the length of a specific message is too big
            foreach (var pushMessageTouple in newFcmPushMessages.ToList())
            {
                var (pushMessage, entityType) = pushMessageTouple;
                var fcmToken = fcmTokens.FirstOrDefault(x => x.Id == pushMessage.FcmRegistrationTokenId);
                if (fcmToken == null)
                {
                    newFcmPushMessages.Remove(pushMessageTouple);
                    continue;
                }
                if (pushMessage.JsonPayload.Length > EntityStringFieldLengths.FcmPushMessage.MaxAllowedJsonPayloadLength)
                {
                    Logger.Warn($"Falling back to 'ReloadAllEntitiesPushMessage' because FCM payload is too long ({pushMessage.JsonPayload.Length}) for user id {fcmToken.UserId}, token id {fcmToken.Id}: {pushMessage.JsonPayload}");
                    var fallbackPushMessage = new ReloadAllEntitiesPushMessage()
                    {
                        Guid = pushMessage.Id,
                        EntityType = entityType
                    };
                    var fallbackPushMessageJson = JsonConvert.SerializeObject(fallbackPushMessage);

                    var fallbackPushMessageEntity = new FcmPushMessage
                    {
                        Id = pushMessage.Id,
                        TenantId = pushMessage.TenantId,
                        FcmRegistrationTokenId = pushMessage.FcmRegistrationTokenId,
                        ReceiverUserId = pushMessage.ReceiverUserId,
                        ReceiverDriverId = pushMessage.ReceiverDriverId,
                        JsonPayload = fallbackPushMessageJson,
                    };
                    newFcmPushMessages.Remove(pushMessageTouple);
                    newFcmPushMessages.Add((fallbackPushMessageEntity, entityType));
                    _fcmPushMessageRepository.Insert(pushMessage);
                }
                else
                {
                    _fcmPushMessageRepository.Insert(pushMessage);
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            foreach (var (pushMessage, _) in newFcmPushMessages.ToList())
            {
                var fcmToken = fcmTokens.FirstOrDefault(x => x.Id == pushMessage.FcmRegistrationTokenId);
                if (fcmToken == null)
                {
                    continue;
                }
                await _backgroundJobManager.EnqueueAsync<FirebasePushSenderBackgroundJob, FirebasePushSenderBackgroundJobArgs>(new FirebasePushSenderBackgroundJobArgs
                {
                    JsonPayload = pushMessage.JsonPayload,
                    PushMessageGuid = pushMessage.Id,
                    RegistrationToken = fcmToken,
                    RequestorUser = Session.ToUserIdentifier(),
                });
            }
        }

        private List<int> GetDriverIds(SyncRequest syncRequest)
        {
            var driverRelatedChanges = syncRequest.Changes
                            .OfType<ISyncRequestChangeDetail>()
                            .Where(x => x.Entity is IChangedDriverAppEntity)
                            .Select(x => (IChangedDriverAppEntity)x.Entity)
                            .ToList();

            var driverIds = driverRelatedChanges.Select(x => x.DriverId)
                .Union(driverRelatedChanges.Select(x => x.OldDriverIdToNotify))
                .Distinct()
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();
            
            return driverIds;
        }

        private List<int> GetDriverIds(IChangedDriverAppEntity changedDriverAppEntity)
        {
            return new[] { changedDriverAppEntity.DriverId, changedDriverAppEntity.OldDriverIdToNotify }
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();
        }
    }
}
