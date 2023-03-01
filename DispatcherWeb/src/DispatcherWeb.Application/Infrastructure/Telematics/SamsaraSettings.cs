namespace DispatcherWeb.Infrastructure.Telematics
{
    public class SamsaraSettings
    {
        public string ApiToken { get; set; }
        public string BaseUrl { get; set; }

        public bool IsEmpty() => string.IsNullOrEmpty(ApiToken)
            || string.IsNullOrEmpty(BaseUrl);
    }
}
