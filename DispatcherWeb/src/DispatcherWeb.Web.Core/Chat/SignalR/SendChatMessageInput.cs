namespace DispatcherWeb.Web.Chat.SignalR
{
    public class SendChatMessageInput
    {
        public long TargetUserId { get; set; }

        public string Message { get; set; }
    }
}