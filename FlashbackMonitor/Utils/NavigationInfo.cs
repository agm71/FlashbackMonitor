namespace FlashbackMonitor.Utils
{
    public class NavigationInfo
    {
        public string RequestedUrl { get; set; }
        public UserControlType RequestedUserControl { get; set; }
        public string PreviousUrl { get; set; }
        public UserControlType PreviousUserControl { get; set; }
    }

    public enum UserControlType
    {
        MainUserControl,
        TopicUserControl,
        ThreadListUserControl,
        SettingsUserControl
    }
}