using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using DispatcherWeb.Chat;
using DispatcherWeb.DriverApp.Messages.Dto;
using DispatcherWeb.Friendships;
using DispatcherWeb.Friendships.Cache;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.Messages
{
    [AbpAuthorize]
    public class MessageAppService : DispatcherWebDriverAppAppServiceBase, IMessageAppService
    {
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IChatMessageManager _chatMessageManager;
        private readonly IFriendshipAppService _friendshipAppService;
        private readonly IUserFriendsCache _userFriendsCache;

        public MessageAppService(
            IRepository<ChatMessage, long> chatMessageRepository,
            IChatMessageManager chatMessageManager,
            IFriendshipAppService friendshipAppService,
            IUserFriendsCache userFriendsCache
            )
        {
            _chatMessageRepository = chatMessageRepository;
            _chatMessageManager = chatMessageManager;
            _friendshipAppService = friendshipAppService;
            _userFriendsCache = userFriendsCache;
        }

        public async Task<IPagedResult<MessageDto>> Get(GetInput input)
        {
            var currentUserId = Session.GetUserId();
            var query = _chatMessageRepository.GetAll()
                .WhereIf(input.AfterId.HasValue, m => m.Id > input.AfterId)
                .Where(m => m.UserId == currentUserId && m.TargetUserId == input.TargetUserId)
                .Select(x => new MessageDto
                {
                    Id = x.Id,
                    Message = x.Message,
                    CreationTime = x.CreationTime,
                    Side = x.Side,
                    ReadState = x.ReadState,
                    ReceiverReadState = x.ReceiverReadState
                })
                .OrderByDescending(x => x.Id);

            var totalCount = await query.CountAsync();
            var items = await query
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<MessageDto>(
                totalCount,
                items);
        }

        public async Task<MessageDto> Post(PostInput model)
        {
            var cacheFriendshipItem = await _userFriendsCache.GetCacheItemAsync(Session.ToUserIdentifier());
            if (!cacheFriendshipItem.Friends.Any(f => f.FriendUserId == model.TargetUserId))
            {
                await _friendshipAppService.CreateFriendshipRequest(new Friendships.Dto.CreateFriendshipRequestInput
                {
                    TenantId = Session.GetTenantId(),
                    UserId = model.TargetUserId
                });
            }

            var sentMessage = await _chatMessageManager.SendMessageAsync(model.TargetUserId, model.Message);
            if (sentMessage == null)
            {
                return null;
            }

            return new MessageDto
            {
                Id = sentMessage.Id,
                CreationTime = sentMessage.CreationTime,
                Message = sentMessage.Message,
                Side = sentMessage.Side,
                ReadState = sentMessage.ReadState,
                ReceiverReadState = sentMessage.ReceiverReadState,
            };
        }

        public async Task MarkAsRead(MarkAsReadInput input)
        {
            await _chatMessageManager.MarkAsReadAsync(input.TargetUserId.Value);
        }
    }
}
