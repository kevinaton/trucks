using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Users.Cache
{
    public class UserCache : IUserCache, ISingletonDependency
    {
        private readonly ICacheManager _cacheManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly UserStore _userStore;

        public UserCache(
                ICacheManager cacheManager,
                IUnitOfWorkManager unitOfWorkManager,
                UserStore userStore
            )
        {
            _cacheManager = cacheManager;
            _unitOfWorkManager = unitOfWorkManager;
            _userStore = userStore;
        }

        private ITypedCache<long, UserCacheItem> GetUserCacheInternal()
        {
            return _cacheManager
                .GetCache(UserCacheItem.CacheName)
                .AsTyped<long, UserCacheItem>();
        }

        private async Task<List<UserCacheItem>> GetUsersFromDbAsync(params long[] userIds)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    var users = await _userStore.Users
                        .Where(x => userIds.Contains(x.Id))
                        .Select(x => new UserCacheItem
                        {
                            Id = x.Id,
                            TenantId = x.TenantId,
                            UserName = x.UserName,
                            Email = x.EmailAddress,
                            FirstName = x.Name,
                            LastName = x.Surname,
                            ProfilePictureId = x.ProfilePictureId
                        })
                        .ToListAsync();

                    var userCache = GetUserCacheInternal();
                    foreach (var user in users)
                    {
                        await userCache.SetAsync(user.Id, user);
                    }

                    return users;
                }
            });
        }

        public async Task<List<UserCacheItem>> GetUsersAsync(List<UserIdentifier> userIdentifiers)
        {
            return await GetUsersAsync(userIdentifiers.Select(x => x.UserId).ToList());
        }

        public async Task<List<UserCacheItem>> GetUsersAsync(List<long> userIds)
        {
            var cachedUsers = new List<UserCacheItem>();
            foreach (var userId in userIds.ToList())
            {
                var cachedUser = await GetUserCacheInternal()
                    .GetOrDefaultAsync(userId);
                    
                if (cachedUser != null)
                {
                    userIds.Remove(userId);
                    cachedUsers.Add(cachedUser);
                }
            }

            if (userIds.Any())
            {
                var users = await GetUsersFromDbAsync(userIds.ToArray());
                cachedUsers.AddRange(users);
            }

            return cachedUsers;
        }

        public async Task<UserCacheItem> GetUserAsync(UserIdentifier userIdentifier)
        {
            return await GetUserAsync(userIdentifier.UserId);
        }

        public async Task<UserCacheItem> GetUserAsync(long userId)
        {
            var cachedUser = await GetUserCacheInternal()
                .GetOrDefaultAsync(userId);

            if (cachedUser != null)
            {
                return cachedUser;
            }

            var users = await GetUsersFromDbAsync(userId);

            return users.FirstOrDefault();
        }
    }
}
