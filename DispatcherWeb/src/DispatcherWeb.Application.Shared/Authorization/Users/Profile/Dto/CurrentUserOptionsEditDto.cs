namespace DispatcherWeb.Authorization.Users.Profile.Dto
{
    public class CurrentUserOptionsEditDto
    {
        public bool DontShowZeroQuantityWarning { get; set; }
        public bool PlaySoundForNotifications { get; set; }
        public HostEmailPreference HostEmailPreference { get; set; }
    }
}
