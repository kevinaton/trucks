using System.Collections.Generic;
using Abp.Domain.Repositories;
using DispatcherWeb.Chat.Dto;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Linq.Extensions;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.Timing;
using Microsoft.EntityFrameworkCore;
using DispatcherWeb.Friendships.Cache;
using DispatcherWeb.Friendships.Dto;

namespace DispatcherWeb.Chat
{
    [AbpAuthorize]
    public class ChatAppService : DispatcherWebAppServiceBase, IChatAppService
    {
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IOnlineClientManager<ChatChannel> _onlineClientManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IChatMessageManager _chatMessageManager;

        public ChatAppService(
            IRepository<ChatMessage, long> chatMessageRepository,
            IUserFriendsCache userFriendsCache,
            IOnlineClientManager<ChatChannel> onlineClientManager,
            IChatCommunicator chatCommunicator,
            IChatMessageManager chatMessageManager)
        {
            _chatMessageRepository = chatMessageRepository;
            _userFriendsCache = userFriendsCache;
            _onlineClientManager = onlineClientManager;
            _chatCommunicator = chatCommunicator;
            _chatMessageManager = chatMessageManager;
        }

        [DisableAuditing]
        public async Task<GetUserChatFriendsWithSettingsOutput> GetUserChatFriendsWithSettings()
        {
            var userIdentifier = AbpSession.ToUserIdentifier();
            if (userIdentifier == null)
            {
                return new GetUserChatFriendsWithSettingsOutput();
            }

            var cacheItem = await _userFriendsCache.GetCacheItemAsync(userIdentifier);
            var friends = ObjectMapper.Map<List<FriendDto>>(cacheItem.Friends);

            foreach (var friend in friends)
            {
                friend.IsOnline = _onlineClientManager.IsOnline(
                    new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                );
            }

            return new GetUserChatFriendsWithSettingsOutput
            {
                Friends = friends,
                ServerTime = Clock.Now
            };
        }

        [DisableAuditing]
        public async Task<ListResultDto<ChatMessageDto>> GetUserChatMessages(GetUserChatMessagesInput input)
        {
            var userId = AbpSession.GetUserId();
            var messages = await _chatMessageRepository.GetAll()
                    .WhereIf(input.MinMessageId.HasValue, m => m.Id < input.MinMessageId.Value)
                    .Where(m => m.UserId == userId && m.TargetTenantId == input.TenantId && m.TargetUserId == input.UserId)
                    .OrderByDescending(m => m.CreationTime)
                    .Take(50)
                    .ToListAsync();

            messages.Reverse();

            return new ListResultDto<ChatMessageDto>(ObjectMapper.Map<List<ChatMessageDto>>(messages));
        }

        public async Task MarkAllUnreadMessagesOfUserAsRead(MarkAllUnreadMessagesOfUserAsReadInput input)
        {
            await _chatMessageManager.MarkAsReadAsync(input.UserId);
        }
    }
}
