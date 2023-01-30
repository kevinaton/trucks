using System.Threading.Tasks;
using Abp;

namespace DispatcherWeb.Friendships.Cache
{
    public interface IUserFriendsCache
    {
        Task<UserWithFriendsCacheItem> GetCacheItemAsync(UserIdentifier userIdentifier);

        Task<UserWithFriendsCacheItem> GetCacheItemOrNullAsync(UserIdentifier userIdentifier);

        Task ResetUnreadMessageCountAsync(UserIdentifier userIdentifier, UserIdentifier friendIdentifier);

        Task IncreaseUnreadMessageCountAsync(UserIdentifier userIdentifier, UserIdentifier friendIdentifier, int change);

        Task AddFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend);

        Task RemoveFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend);

        Task UpdateFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend);
    }
}
