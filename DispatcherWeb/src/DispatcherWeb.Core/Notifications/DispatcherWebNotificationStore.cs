using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Notifications
{
    public class DispatcherWebNotificationStore : NotificationStore
    {
        private readonly IRepository<TenantNotificationInfo, Guid> _tenantNotificationRepository;
        private readonly IRepository<UserNotificationInfo, Guid> _userNotificationRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public DispatcherWebNotificationStore(
            IRepository<NotificationInfo, Guid> notificationRepository,
            IRepository<TenantNotificationInfo, Guid> tenantNotificationRepository,
            IRepository<UserNotificationInfo, Guid> userNotificationRepository,
            IRepository<NotificationSubscriptionInfo, Guid> notificationSubscriptionRepository,
            IUnitOfWorkManager unitOfWorkManager
            ) : base(notificationRepository, tenantNotificationRepository, userNotificationRepository, notificationSubscriptionRepository, unitOfWorkManager)
        {
            _tenantNotificationRepository = tenantNotificationRepository;
            _userNotificationRepository = userNotificationRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public override async Task<List<UserNotificationInfoWithNotificationInfo>> GetUserNotificationsWithNotificationsAsync(
                UserIdentifier user,
                UserNotificationState? state = null,
                int skipCount = 0,
                int maxResultCount = int.MaxValue,
                DateTime? startDate = null,
                DateTime? endDate = null)
        {
            var result = await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(user.TenantId))
                {
                    var query = from userNotificationInfo in _userNotificationRepository.GetAll()
                                join tenantNotificationInfo in _tenantNotificationRepository.GetAll()
                                    on userNotificationInfo.TenantNotificationId equals tenantNotificationInfo.Id
                                where userNotificationInfo.UserId == user.UserId
                                orderby userNotificationInfo.CreationTime descending
                                select new
                                {
                                    userNotificationInfo,
                                    tenantNotificationInfo
                                };

                    if (state.HasValue)
                    {
                        query = query.Where(x => x.userNotificationInfo.State == state.Value);
                    }

                    if (startDate.HasValue)
                    {
                        query = query.Where(x => x.tenantNotificationInfo.CreationTime >= startDate);
                    }

                    if (endDate.HasValue)
                    {
                        query = query.Where(x => x.tenantNotificationInfo.CreationTime <= endDate);
                    }

                    query = query.PageBy(skipCount, maxResultCount);

                    var list = await query.ToListAsync();

                    return list.Select(
                        a => new UserNotificationInfoWithNotificationInfo(
                            a.userNotificationInfo,
                            a.tenantNotificationInfo
                        )
                    ).ToList();
                }
            });

            return await Task.FromResult(result);
        }

        public override List<UserNotificationInfoWithNotificationInfo> GetUserNotificationsWithNotifications(
            UserIdentifier user,
            UserNotificationState? state = null,
            int skipCount = 0,
            int maxResultCount = int.MaxValue,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(user.TenantId))
                {
                    var query = from userNotificationInfo in _userNotificationRepository.GetAll()
                                join tenantNotificationInfo in _tenantNotificationRepository.GetAll()
                                    on userNotificationInfo.TenantNotificationId equals tenantNotificationInfo.Id
                                where userNotificationInfo.UserId == user.UserId
                                orderby tenantNotificationInfo.CreationTime descending
                                select new
                                {
                                    userNotificationInfo,
                                    tenantNotificationInfo
                                };

                    if (state.HasValue)
                    {
                        query = query.Where(x => x.userNotificationInfo.State == state.Value);
                    }

                    if (startDate.HasValue)
                    {
                        query = query.Where(x => x.tenantNotificationInfo.CreationTime >= startDate);
                    }

                    if (endDate.HasValue)
                    {
                        query = query.Where(x => x.tenantNotificationInfo.CreationTime <= endDate);
                    }

                    query = query.PageBy(skipCount, maxResultCount);

                    var list = query.ToList();

                    return list.Select(
                        a => new UserNotificationInfoWithNotificationInfo(
                            a.userNotificationInfo,
                            a.tenantNotificationInfo
                        )
                    ).ToList();
                }
            });
        }
    }
}
