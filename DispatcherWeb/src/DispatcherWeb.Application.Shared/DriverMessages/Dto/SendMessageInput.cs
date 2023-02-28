namespace DispatcherWeb.DriverMessages.Dto
{
    public class SendMessageInput
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public int[] DriverIds { get; set; }
        public int[] OfficeIds { get; set; }
    }
}
