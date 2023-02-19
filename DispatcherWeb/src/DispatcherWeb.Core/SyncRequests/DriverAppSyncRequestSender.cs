using System;
using System.Collections.Generic;
using System.Linq;
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
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.SyncRequests.DriverApp;
using DispatcherWeb.SyncRequests.FcmPushMessages;
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
            var userIds = GetUserIds(syncRequest);
            if (!driverIds.Any() && !userIds.Any())
            {
                return;
            }

            var drivers = await _driverRepository.GetAll()
                .Where(x => x.UserId.HasValue && (driverIds.Contains(x.Id) || userIds.Contains(x.UserId.Value)))
                .Select(x => new UserDriverId
                {
                    DriverId = x.Id,
                    UserId = x.UserId.Value,
                    IsDriverActive = !x.IsInactive
                }).ToListAsync();

            userIds = userIds.Union(drivers.Select(x => x.UserId)).Distinct().ToList();
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

            foreach (var entityTypeGroup in syncRequestChanges.GroupBy(x => x.EntityType))
            {
                var entityType = entityTypeGroup.Key;
                if (EntityTypesToIgnore.Contains(entityType))
                {
                    continue;
                }

                var changeDetailsConverter = GetChangeDetailsConverter(entityType);
                await changeDetailsConverter.CacheDataIfNeeded(entityTypeGroup);

                var changeDetails = entityTypeGroup
                    .OfType<ISyncRequestChangeDetail>()
                    .Where(x => x.Entity is IChangedDriverAppEntity)
                    .SelectMany(x => GetUserDriverIds((IChangedDriverAppEntity)x.Entity, drivers).Select(user => new
                    {
                        User = user,
                        Entity = (IChangedDriverAppEntity)x.Entity,
                        x.ChangeType
                    }))
                    .ToList();

                foreach (var changeDetailsOfUser in changeDetails.GroupBy(x => x.User))
                {
                    var user = changeDetailsOfUser.Key;
                    foreach (var fcmToken in fcmTokens.Where(x => x.UserId == user.UserId))
                    {
                        var pushMessage = new ReloadSpecificEntitiesPushMessage
                        {
                            EntityType = changeDetailsConverter.GetEntityTypeForPushMessage(entityType)
                        };

                        foreach (var changeDetail in changeDetailsOfUser)
                        {
                            pushMessage.Changes.Add(changeDetailsConverter.GetChangeDetails(changeDetail.Entity, changeDetail.ChangeType));
                        }

                        var pushMessageJson = JsonConvert.SerializeObject(pushMessage);

                        newFcmPushMessages.Add((new FcmPushMessage
                        {
                            Id = pushMessage.Guid,
                            TenantId = Session.TenantId,
                            FcmRegistrationTokenId = fcmToken.Id,
                            ReceiverUserId = user.UserId,
                            ReceiverDriverId = user.DriverId,
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

        private GenericChangeDetailsConverter GetChangeDetailsConverter(EntityEnum entityType)
        {
            switch (entityType)
            {
                case EntityEnum.Dispatch:
                    return new DispatchChangeDetailsConverter(_dispatchRepository);
                case EntityEnum.EmployeeTimeClassification:
                    return new TimeClassificationChangeDetailsConverter();
                case EntityEnum.ChatMessage:
                    return new ChatMessageChangeDetailsConverter();
                default:
                    return new GenericChangeDetailsConverter();
            }
        }

        private static List<IChangedDriverAppEntity> GetDriverRelatedChanges(SyncRequest syncRequest)
        {
            return syncRequest.Changes
                            .OfType<ISyncRequestChangeDetail>()
                            .Where(x => x.Entity is IChangedDriverAppEntity)
                            .Select(x => (IChangedDriverAppEntity)x.Entity)
                            .ToList();
        }

        private static List<int> GetDriverIds(SyncRequest syncRequest)
        {
            var driverRelatedChanges = GetDriverRelatedChanges(syncRequest);

            var driverIds = driverRelatedChanges.Select(x => x.DriverId)
                .Union(driverRelatedChanges.Select(x => x.OldDriverIdToNotify))
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Union(driverRelatedChanges.SelectMany(x => x.DriverIds ?? new List<int>()))
                .Distinct()
                .ToList();
            
            return driverIds;
        }

        private static List<long> GetUserIds(SyncRequest syncRequest)
        {
            var driverRelatedChanges = GetDriverRelatedChanges(syncRequest);

            var userIds = driverRelatedChanges.Select(x => x.UserId)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

            return userIds;
        }

        private static List<UserDriverId> GetUserDriverIds(IChangedDriverAppEntity changedDriverAppEntity, List<UserDriverId> cachedDriverUserIds)
        {
            var result = new List<UserDriverId>();

            var driverIds = new[] { changedDriverAppEntity.DriverId, changedDriverAppEntity.OldDriverIdToNotify }
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Union(changedDriverAppEntity.DriverIds ?? new List<int>())
                .Distinct()
                .ToList();

            foreach (var driverId in driverIds)
            {
                var user = GetUserFromDriverId(driverId, cachedDriverUserIds);
                if (user != null)
                {
                    result.Add(user);
                }
            }

            var userIds = new[] { changedDriverAppEntity.UserId }
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();

            foreach (var userId in userIds)
            {
                var driver = GetDriverFromUserId(changedDriverAppEntity.UserId.Value, cachedDriverUserIds);
                if (driver != null && !result.Contains(driver))
                {
                    result.Add(driver);
                }
                else if (!result.Any(x => x.UserId == userId))
                {
                    result.Add(new UserDriverId()
                    {
                        UserId = userId,
                    });
                }
            }

            return result;
        }

        private static UserDriverId GetDriverFromUserId(long userId, List<UserDriverId> cachedDriverUserIds)
        {
            return cachedDriverUserIds
                .OrderByDescending(x => x.IsDriverActive)
                .FirstOrDefault(x => x.UserId == userId);
        }

        private static UserDriverId GetUserFromDriverId(int driverId, List<UserDriverId> cachedDriverUserIds)
        {
            return cachedDriverUserIds.FirstOrDefault(x => x.DriverId == driverId);
        }

        private class UserDriverId
        {
            public long UserId { get; set; }
            public int? DriverId { get; set; }
            public bool IsDriverActive { get; set; }
        }
    }
}
