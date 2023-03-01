using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Runtime.Caching;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Cache;
using DispatcherWeb.Chat;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Friendships.Cache
{
    public class UserFriendsCache : IUserFriendsCache, ISingletonDependency
    {
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Friendship, long> _friendshipRepository;
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly ITenantCache _tenantCache;
        private readonly IUserCache _userCache;
        private readonly UserStore _userStore;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        private readonly object _syncObj = new object();

        public UserFriendsCache(
            ICacheManager cacheManager,
            IRepository<Friendship, long> friendshipRepository,
            IRepository<ChatMessage, long> chatMessageRepository,
            ITenantCache tenantCache,
            IUserCache userCache,
            IUnitOfWorkManager unitOfWorkManager,
            UserStore userStore)
        {
            _cacheManager = cacheManager;
            _friendshipRepository = friendshipRepository;
            _chatMessageRepository = chatMessageRepository;
            _tenantCache = tenantCache;
            _userCache = userCache;
            _unitOfWorkManager = unitOfWorkManager;
            _userStore = userStore;
        }

        //[UnitOfWork]
        public virtual async Task<UserWithFriendsCacheItem> GetCacheItemAsync(UserIdentifier userIdentifier)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var cacheItem = await _cacheManager
                    .GetCache(FriendCacheItem.CacheName)
                    .AsTyped<string, UserWithFriendsCacheItem>()
                    .GetAsync(userIdentifier.ToUserIdentifierString(), async f => await GetUserFriendsCacheItemInternalAsync(userIdentifier));

                await UpdateFriendNamesFromCache(cacheItem.Friends);

                return cacheItem;
            });
        }

        public virtual async Task<UserWithFriendsCacheItem> GetCacheItemOrNullAsync(UserIdentifier userIdentifier)
        {
            var cacheItem = await _cacheManager
                .GetCache(FriendCacheItem.CacheName)
                .AsTyped<string, UserWithFriendsCacheItem>()
                .GetOrDefaultAsync(userIdentifier.ToUserIdentifierString());

            if (cacheItem != null)
            {
                await UpdateFriendNamesFromCache(cacheItem.Friends);
            }

            return cacheItem;
        }

        //[UnitOfWork]
        public virtual async Task ResetUnreadMessageCountAsync(UserIdentifier userIdentifier, UserIdentifier friendIdentifier)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var user = await GetCacheItemOrNullAsync(userIdentifier);
                if (user == null)
                {
                    return;
                }

                lock (_syncObj)
                {
                    var friend = user.Friends.FirstOrDefault(
                        f => f.FriendUserId == friendIdentifier.UserId &&
                             f.FriendTenantId == friendIdentifier.TenantId
                    );

                    if (friend == null)
                    {
                        return;
                    }

                    friend.UnreadMessageCount = 0;
                    UpdateUserOnCache(userIdentifier, user);
                }
            });
        }

        public virtual async Task IncreaseUnreadMessageCountAsync(UserIdentifier userIdentifier, UserIdentifier friendIdentifier, int change)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var user = await GetCacheItemOrNullAsync(userIdentifier);
                if (user == null)
                {
                    return;
                }

                lock (_syncObj)
                {
                    var friend = user.Friends.FirstOrDefault(
                        f => f.FriendUserId == friendIdentifier.UserId &&
                             f.FriendTenantId == friendIdentifier.TenantId
                    );

                    if (friend == null)
                    {
                        return;
                    }

                    friend.UnreadMessageCount += change;
                    UpdateUserOnCache(userIdentifier, user);
                }
            });
        }

        public async Task AddFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend)
        {
            var user = await GetCacheItemOrNullAsync(userIdentifier);
            if (user == null)
            {
                return;
            }

            lock (_syncObj)
            {
                if (!user.Friends.ContainsFriend(friend))
                {
                    user.Friends.Add(friend);
                    UpdateUserOnCache(userIdentifier, user);
                }
            }
        }

        public async Task RemoveFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend)
        {
            var user = await GetCacheItemOrNullAsync(userIdentifier);
            if (user == null)
            {
                return;
            }

            lock (_syncObj)
            {
                if (user.Friends.ContainsFriend(friend))
                {
                    user.Friends.Remove(friend);
                    UpdateUserOnCache(userIdentifier, user);
                }
            }
        }

        public async Task UpdateFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend)
        {
            var user = await GetCacheItemOrNullAsync(userIdentifier);
            if (user == null)
            {
                return;
            }

            lock (_syncObj)
            {
                var existingFriendIndex = user.Friends.FindIndex(
                    f => f.FriendUserId == friend.FriendUserId &&
                         f.FriendTenantId == friend.FriendTenantId
                );

                if (existingFriendIndex >= 0)
                {
                    user.Friends[existingFriendIndex] = friend;
                    UpdateUserOnCache(userIdentifier, user);
                }
            }
        }

        protected virtual async Task<UserWithFriendsCacheItem> GetUserFriendsCacheItemInternalAsync(UserIdentifier userIdentifier)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var tenancyName = userIdentifier.TenantId.HasValue
                    ? (await _tenantCache.GetOrNullAsync(userIdentifier.TenantId.Value))?.TenancyName
                    : null;

                using (_unitOfWorkManager.Current.SetTenantId(userIdentifier.TenantId))
                {
                    var friendCacheItems = await _friendshipRepository.GetAll()
                        .Where(friendship => friendship.UserId == userIdentifier.UserId)
                        .Select(friendship => new FriendCacheItem
                        {
                            FriendUserId = friendship.FriendUserId,
                            FriendTenantId = friendship.FriendTenantId,
                            State = friendship.State,
                            FriendUserName = friendship.FriendUserName,
                            FriendTenancyName = friendship.FriendTenancyName,
                            FriendProfilePictureId = friendship.FriendProfilePictureId,
                            UnreadMessageCount = _chatMessageRepository.GetAll().Count(cm =>
                                cm.ReadState == ChatMessageReadState.Unread &&
                                cm.UserId == userIdentifier.UserId &&
                                cm.TenantId == userIdentifier.TenantId &&
                                cm.TargetUserId == friendship.FriendUserId &&
                                cm.TargetTenantId == friendship.FriendTenantId &&
                                cm.Side == ChatSide.Receiver)
                        }).ToListAsync();

                    await UpdateFriendNamesFromCache(friendCacheItems);

                    var user = await _userCache.GetUserAsync(userIdentifier);

                    return new UserWithFriendsCacheItem
                    {
                        TenantId = userIdentifier.TenantId,
                        UserId = userIdentifier.UserId,
                        TenancyName = tenancyName,
                        UserName = user.UserName,
                        ProfilePictureId = user.ProfilePictureId,
                        Friends = friendCacheItems
                    };
                }
            });
        }

        private void UpdateUserOnCache(UserIdentifier userIdentifier, UserWithFriendsCacheItem user)
        {
            _cacheManager.GetCache(FriendCacheItem.CacheName).Set(userIdentifier.ToUserIdentifierString(), user);
        }

        private async Task UpdateFriendNamesFromCache(IEnumerable<FriendCacheItem> friends)
        {
            var userIds = friends.Select(x => x.ToFriendIdentifier()).ToList();
            var users = await _userCache.GetUsersAsync(userIds);
            foreach (var friend in friends)
            {
                var friendUser = users.FirstOrDefault(x => x.ToUserIdentifier() == friend.ToFriendIdentifier());
                if (friendUser == null)
                {
                    friend.IsMissing = true;
                }
                else
                {
                    friend.FriendFirstName = friendUser.FirstName;
                    friend.FriendLastName = friendUser.LastName;
                }
            }
        }
    }
}
