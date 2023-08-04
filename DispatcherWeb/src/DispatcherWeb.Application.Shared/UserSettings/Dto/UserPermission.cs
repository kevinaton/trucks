namespace DispatcherWeb.UserSettings.Dto
{
    public class UserPermission
    {
        public bool Edit { get; set; }
        public bool Print { get; set; }
        public bool EditTickets { get; set; }
        public bool EditQuotes { get; set; }
        public bool DriverMessages { get; set; }
        public bool Trucks { get; set; }
    }
}