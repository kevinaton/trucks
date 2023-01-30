using System;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Services;

namespace DispatcherWeb.Chat
{
    public interface IChatMessageManager : IDomainService
    {
        Task<ChatMessage> SendMessageAsync(long targetUserId, string message);
        Task<ChatMessage> SendMessageAsync(UserIdentifier senderIdentifier, long targetUserId, string message);
        Task<ChatMessage> SendMessageAsync(UserIdentifier sender, UserIdentifier receiver, string message, string senderTenancyName, string senderUserName, Guid? senderProfilePictureId);

        long Save(ChatMessage message);

        int GetUnreadMessageCount(UserIdentifier userIdentifier, UserIdentifier sender);

        Task<ChatMessage> FindMessageAsync(int id, long userId);
        Task MarkAsReadAsync(long targetUserId);
    }
}
