using Abp;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using DispatcherWeb.Chat;
using DispatcherWeb.DriverApp.Friends.Dto;
using DispatcherWeb.Friendships.Cache;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.Friends
{
    [AbpAuthorize]
    public class FriendAppService : DispatcherWebDriverAppAppServiceBase, IFriendAppService
    {
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IUserFriendsCache _userFriendsCache;
        //private readonly IOnlineClientManager<ChatChannel> _onlineClientManager;

        public FriendAppService(
            IRepository<ChatMessage, long> chatMessageRepository,
            IUserFriendsCache userFriendsCache
            )
        {
            _chatMessageRepository = chatMessageRepository;
            _userFriendsCache = userFriendsCache;
        }

        public async Task<IPagedResult<FriendDto>> Get(GetInput input)
        {
            var cacheItem = await _userFriendsCache.GetCacheItemAsync(Session.ToUserIdentifier());
            var allFriends = cacheItem.Friends.Select(x => new FriendDto
            {
                User = new Users.Dto.UserDto
                {
                    Id = x.FriendUserId,
                    FirstName = x.FriendFirstName,
                    LastName = x.FriendLastName,
                    ProfilePictureId = x.FriendProfilePictureId
                }
            }).ToList();

            var totalFriendCount = allFriends.Count;

            //foreach (var friend in friends)
            //{
            //    friend.IsOnline = _onlineClientManager.IsOnline(
            //        new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
            //    );
            //}

            var friendIds = allFriends.Select(x => x.User.Id).ToList();
            var currentUserId = Session.GetUserId();

            var messages = await _chatMessageRepository.GetAll()
                .Where(m => m.UserId == currentUserId && friendIds.Contains(m.TargetUserId))
                .GroupBy(x => new { x.TargetUserId })
                .Select(x => new
                {
                    FriendUserId = x.Key.TargetUserId,
                    LastMessage = x
                        .OrderByDescending(x => x.CreationTime)
                        .Select(m => new Messages.Dto.MessageDto
                        {
                            Id = m.Id,
                            Message = m.Message,
                            CreationTime = m.CreationTime,
                            Side = m.Side,
                            ReadState = m.ReadState,
                            ReceiverReadState = m.ReceiverReadState
                        })
                        .First(),
                   UnreadMessageCount = x.Count(m => m.ReadState == ChatMessageReadState.Unread)
                })
                .ToListAsync();

            foreach (var friend in allFriends)
            {
                var lastMessage = messages.FirstOrDefault(x => x.FriendUserId == friend.User.Id);
                friend.LastMessage = lastMessage?.LastMessage;
                friend.UnreadMessageCount = lastMessage?.UnreadMessageCount ?? 0;
            }

            allFriends = allFriends.OrderByDescending(x => x.LastMessage != null ? x.LastMessage.CreationTime : DateTime.MaxValue).ToList();

            var friends = allFriends
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            return new PagedResultDto<FriendDto>(
                totalFriendCount,
                friends);
        }
    }
}
