using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.ObjectMapping;
using DispatcherWeb.Chat;

namespace DispatcherWeb.Friendships.Cache
{
    public class UserFriendCacheSyncronizer :
        IAsyncEventHandler<EntityCreatedEventData<Friendship>>,
        IAsyncEventHandler<EntityDeletedEventData<Friendship>>,
        IAsyncEventHandler<EntityUpdatedEventData<Friendship>>,
        IAsyncEventHandler<EntityCreatedEventData<ChatMessage>>,
        ITransientDependency
    {
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IObjectMapper _objectMapper;

        public UserFriendCacheSyncronizer(
            IUserFriendsCache userFriendsCache,
            IObjectMapper objectMapper)
        {
            _userFriendsCache = userFriendsCache;
            _objectMapper = objectMapper;
        }

        public async Task HandleEventAsync(EntityCreatedEventData<Friendship> eventData)
        {
            await _userFriendsCache.AddFriendAsync(
                eventData.Entity.ToUserIdentifier(),
                _objectMapper.Map<FriendCacheItem>(eventData.Entity)
                );
        }

        public async Task HandleEventAsync(EntityDeletedEventData<Friendship> eventData)
        {
            await _userFriendsCache.RemoveFriendAsync(
                eventData.Entity.ToUserIdentifier(),
                _objectMapper.Map<FriendCacheItem>(eventData.Entity)
            );
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<Friendship> eventData)
        {
            var friendCacheItem = _objectMapper.Map<FriendCacheItem>(eventData.Entity);
            await _userFriendsCache.UpdateFriendAsync(eventData.Entity.ToUserIdentifier(), friendCacheItem);
        }

        public async Task HandleEventAsync(EntityCreatedEventData<ChatMessage> eventData)
        {
            var message = eventData.Entity;
            if (message.ReadState == ChatMessageReadState.Unread)
            {
                await _userFriendsCache.IncreaseUnreadMessageCountAsync(
                    new UserIdentifier(message.TenantId, message.UserId),
                    new UserIdentifier(message.TargetTenantId, message.TargetUserId),
                    1
                );
            }
        }
    }
}
