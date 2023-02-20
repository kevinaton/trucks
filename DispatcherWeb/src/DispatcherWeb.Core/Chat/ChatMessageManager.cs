using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Cache;
using DispatcherWeb.Friendships;
using DispatcherWeb.Friendships.Cache;
using DispatcherWeb.SyncRequests;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Chat
{
    [AbpAuthorize]
    public class ChatMessageManager : DispatcherWebDomainServiceBase, IChatMessageManager
    {
        private readonly IFriendshipManager _friendshipManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IOnlineClientManager<ChatChannel> _onlineClientManager;
        private readonly UserManager _userManager;
        private readonly ITenantCache _tenantCache;
        private readonly IUserCache _userCache;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IUserEmailer _userEmailer;
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IChatFeatureChecker _chatFeatureChecker;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ChatMessageManager(
            IFriendshipManager friendshipManager,
            IChatCommunicator chatCommunicator,
            IOnlineClientManager<ChatChannel> onlineClientManager,
            UserManager userManager,
            ITenantCache tenantCache,
            IUserCache userCache,
            IUserFriendsCache userFriendsCache,
            IUserEmailer userEmailer,
            IRepository<ChatMessage, long> chatMessageRepository,
            IChatFeatureChecker chatFeatureChecker,
            ISyncRequestSender syncRequestSender,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _friendshipManager = friendshipManager;
            _chatCommunicator = chatCommunicator;
            _onlineClientManager = onlineClientManager;
            _userManager = userManager;
            _tenantCache = tenantCache;
            _userCache = userCache;
            _userFriendsCache = userFriendsCache;
            _userEmailer = userEmailer;
            _chatMessageRepository = chatMessageRepository;
            _chatFeatureChecker = chatFeatureChecker;
            _syncRequestSender = syncRequestSender;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<ChatMessage> SendMessageAsync(long targetUserId, string message)
        {
            return await SendMessageAsync(Session.ToUserIdentifier(), targetUserId, message);
        }

        public async Task<ChatMessage> SendMessageAsync(UserIdentifier senderIdentifier, long targetUserId, string message)
        {
            var sender = await _userCache.GetUserAsync(senderIdentifier.UserId);
            var receiver = await _userCache.GetUserAsync(targetUserId);
            var senderTenant = sender.TenantId.HasValue ? await _tenantCache.GetAsync(sender.TenantId.Value) : null;
            return await SendMessageAsync(sender.ToUserIdentifier(), receiver.ToUserIdentifier(), message, senderTenant?.TenancyName, sender.UserName, sender.ProfilePictureId);
        }

        public async Task<ChatMessage> SendMessageAsync(UserIdentifier sender, UserIdentifier receiver, string message, string senderTenancyName, string senderUserName, Guid? senderProfilePictureId)
        {
            CheckReceiverExists(receiver);

            _chatFeatureChecker.CheckChatFeatures(sender.TenantId, receiver.TenantId);

            var friendshipState = (await _friendshipManager.GetFriendshipOrNullAsync(sender, receiver))?.State;
            if (friendshipState == FriendshipState.Blocked)
            {
                throw new UserFriendlyException(L("UserIsBlocked"));
            }

            var sharedMessageId = Guid.NewGuid();

            var sentMessage = await HandleSenderToReceiverAsync(sender, receiver, message, sharedMessageId);
            await HandleReceiverToSenderAsync(sender, receiver, message, sharedMessageId);
            await HandleSenderUserInfoChangeAsync(sender, receiver, senderTenancyName, senderUserName, senderProfilePictureId);

            return sentMessage;
        }

        private void CheckReceiverExists(UserIdentifier receiver)
        {
            var receiverUser = _userManager.GetUserOrNull(receiver);
            if (receiverUser == null)
            {
                throw new UserFriendlyException(L("TargetUserNotFoundProbablyDeleted"));
            }
        }

        [UnitOfWork]
        public virtual long Save(ChatMessage message)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(message.TenantId))
                {
                    return _chatMessageRepository.InsertAndGetId(message);
                }
            });
        }
        
        public virtual int GetUnreadMessageCount(UserIdentifier sender, UserIdentifier receiver)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(receiver.TenantId))
                {
                    return _chatMessageRepository.Count(cm => cm.UserId == receiver.UserId &&
                                                              cm.TargetUserId == sender.UserId &&
                                                              cm.TargetTenantId == sender.TenantId &&
                                                              cm.ReadState == ChatMessageReadState.Unread);
                }
            });
        }

        public async Task<ChatMessage> FindMessageAsync(int id, long userId)
        {
            return await _chatMessageRepository.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        }

        private async Task<ChatMessage> HandleSenderToReceiverAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, string message, Guid sharedMessageId)
        {
            var friendshipState = (await _friendshipManager.GetFriendshipOrNullAsync(senderIdentifier, receiverIdentifier))?.State;
            if (friendshipState == null)
            {
                friendshipState = FriendshipState.Accepted;

                var receiverTenancyName = await GetTenancyNameOrNull(receiverIdentifier.TenantId);

                var receiverUser = await _userManager.GetUserAsync(receiverIdentifier);
                await _friendshipManager.CreateFriendshipAsync(
                    new Friendship(
                        senderIdentifier,
                        receiverIdentifier,
                        receiverTenancyName,
                        receiverUser.UserName,
                        receiverUser.ProfilePictureId,
                        friendshipState.Value)
                );
            }

            if (friendshipState.Value == FriendshipState.Blocked)
            {
                //Do not send message if receiver banned the sender
                return null;
            }

            var sentMessage = new ChatMessage(
                senderIdentifier,
                receiverIdentifier,
                ChatSide.Sender,
                message,
                ChatMessageReadState.Read,
                sharedMessageId,
                ChatMessageReadState.Unread
            );

            Save(sentMessage);

            await _chatCommunicator.SendMessageToClient(
                _onlineClientManager.GetAllByUserId(senderIdentifier),
                sentMessage
                );

            return sentMessage;
        }

        private async Task<ChatMessage> HandleReceiverToSenderAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, string message, Guid sharedMessageId)
        {
            var friendshipState = (await _friendshipManager.GetFriendshipOrNullAsync(receiverIdentifier, senderIdentifier))?.State;

            if (friendshipState == null)
            {
                var senderTenancyName = await GetTenancyNameOrNull(senderIdentifier.TenantId);

                var senderUser = await _userManager.GetUserAsync(senderIdentifier);
                await _friendshipManager.CreateFriendshipAsync(
                    new Friendship(
                        receiverIdentifier,
                        senderIdentifier,
                        senderTenancyName,
                        senderUser.UserName,
                        senderUser.ProfilePictureId,
                        FriendshipState.Accepted
                    )
                );
            }

            if (friendshipState == FriendshipState.Blocked)
            {
                //Do not send message if receiver banned the sender
                throw new UserFriendlyException(L("UserIsBlocked"));
            }

            var sentMessage = new ChatMessage(
                    receiverIdentifier,
                    senderIdentifier,
                    ChatSide.Receiver,
                    message,
                    ChatMessageReadState.Unread,
                    sharedMessageId,
                    ChatMessageReadState.Read
                );

            Save(sentMessage);

            var clients = _onlineClientManager.GetAllByUserId(receiverIdentifier);
            if (clients.Any())
            {
                await _chatCommunicator.SendMessageToClient(clients, sentMessage);
            }
            else
            {
                if (GetUnreadMessageCount(senderIdentifier, receiverIdentifier) == 1)
                {
                    var senderTenancyName = await GetTenancyNameOrNull(senderIdentifier.TenantId);

                    await _userEmailer.TryToSendChatMessageMail(
                          await _userManager.GetUserAsync(receiverIdentifier),
                          (await _userManager.GetUserAsync(senderIdentifier)).UserName,
                          senderTenancyName,
                          sentMessage
                      );
                }

                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (CurrentUnitOfWork.SetTenantId(sentMessage.TenantId))
                    {
                        await _syncRequestSender.SendSyncRequest(new SyncRequest()
                            .AddChange(EntityEnum.ChatMessage, sentMessage.ToChangedEntity()));
                    }
                });
            }

            return sentMessage;
        }

        private async Task HandleSenderUserInfoChangeAsync(UserIdentifier sender, UserIdentifier receiver, string senderTenancyName, string senderUserName, Guid? senderProfilePictureId)
        {
            var receiverCacheItem = await _userFriendsCache.GetCacheItemOrNullAsync(receiver);

            var senderAsFriend = receiverCacheItem?.Friends.FirstOrDefault(f => f.FriendTenantId == sender.TenantId && f.FriendUserId == sender.UserId);
            if (senderAsFriend == null)
            {
                return;
            }

            if (senderAsFriend.FriendTenancyName == senderTenancyName &&
                senderAsFriend.FriendUserName == senderUserName &&
                senderAsFriend.FriendProfilePictureId == senderProfilePictureId)
            {
                return;
            }

            var friendship = (await _friendshipManager.GetFriendshipOrNullAsync(receiver, sender));
            if (friendship == null)
            {
                return;
            }

            friendship.FriendTenancyName = senderTenancyName;
            friendship.FriendUserName = senderUserName;
            friendship.FriendProfilePictureId = senderProfilePictureId;

            await _friendshipManager.UpdateFriendshipAsync(friendship);
        }

        private async Task<string> GetTenancyNameOrNull(int? tenantId)
        {
            if (tenantId.HasValue)
            {
                var tenant = await _tenantCache.GetAsync(tenantId.Value);
                return tenant.TenancyName;
            }

            return null;
        }

        public async Task MarkAsReadAsync(long targetUserId)
        {
            var currentUserId = Session.GetUserId();
            var currentTenantId = Session.TenantId;

            var targetUser = await _userCache.GetUserAsync(targetUserId);

            // receiver messages
            var messages = await _chatMessageRepository
                 .GetAll()
                 .Where(m =>
                        m.UserId == currentUserId &&
                        m.TargetTenantId == targetUser.TenantId &&
                        m.TargetUserId == targetUser.Id &&
                        m.ReadState == ChatMessageReadState.Unread)
                 .ToListAsync();

            if (!messages.Any())
            {
                return;
            }

            foreach (var message in messages)
            {
                message.ChangeReadState(ChatMessageReadState.Read);
            }

            // sender messages
            using (CurrentUnitOfWork.SetTenantId(targetUser.TenantId))
            {
                var reverseMessages = await _chatMessageRepository.GetAll()
                    .Where(m => m.UserId == targetUserId && m.TargetTenantId == currentTenantId && m.TargetUserId == currentUserId)
                    .ToListAsync();

                if (!reverseMessages.Any())
                {
                    return;
                }

                foreach (var message in reverseMessages)
                {
                    message.ChangeReceiverReadState(ChatMessageReadState.Read);
                }
            }

            var userIdentifier = Session.ToUserIdentifier();
            var friendIdentifier = targetUser.ToUserIdentifier();

            await _userFriendsCache.ResetUnreadMessageCountAsync(userIdentifier, friendIdentifier);

            var onlineUserClients = _onlineClientManager.GetAllByUserId(userIdentifier);
            if (onlineUserClients.Any())
            {
                await _chatCommunicator.SendAllUnreadMessagesOfUserReadToClients(onlineUserClients, friendIdentifier);
            }

            var onlineFriendClients = _onlineClientManager.GetAllByUserId(friendIdentifier);
            if (onlineFriendClients.Any())
            {
                await _chatCommunicator.SendReadStateChangeToClients(onlineFriendClients, userIdentifier);
            }
        }
    }
}
