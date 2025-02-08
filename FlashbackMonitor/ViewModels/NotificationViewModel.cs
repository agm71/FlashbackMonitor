namespace FlashbackMonitor.ViewModels
{
    public class NotificationViewModel
    {
        public string UserName {  get; set; }
        public bool VipUser { get; set; }
        public string TopicName { get; set; }
        public string TopicLastUpdated { get; set; }
        public string TopicUrl { get; set; }
        public string TopicColor { get; set; }
        public bool IsFavoriteTopic { get; set; }
        public bool IsFavoriteUser { get; set; }
    }
}
