namespace DispatcherWeb.Locations.Dto
{
    public class OrderSelectListInfoDto
    {
        public string CustomerName { get; set; }
        public string ServiceName { get; set; }
        public LocationSelectListInfoDto DeliverTo { get; set; }
    }
}
