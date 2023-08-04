namespace DispatcherWeb.UserSettings.Dto
{
    public class UserConfig
    {
        public UserPermission Permissions { get; set; }
        public UserFeatures Features { get; set; }
        public UserAppSettings Settings { get; set; }
    }
}