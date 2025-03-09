using ReactiveUI;
using System;

namespace FlashbackMonitor.ViewModels
{
    public class NotificationViewModel : ViewModelBase
    {
        public string UserName {  get; set; }
        public bool VipUser { get; set; }
        public string TopicName { get; set; }
        public string TopicLastUpdated { get; set; }
        public DateTime TopicLastUpdatedDateTime { get; set; }
        public string TopicUrl { get; set; }
        public string TopicColor { get; set; }

        private bool _isFavoriteTopic;
        public bool IsFavoriteTopic
        {
            get => _isFavoriteTopic;
            set => _isFavoriteTopic = value; //this.RaiseAndSetIfChanged(ref _isFavoriteTopic, value);
        }

        private bool _isFavoriteUser;
        public bool IsFavoriteUser
        {
            get => _isFavoriteUser;
            set => this.RaiseAndSetIfChanged(ref _isFavoriteUser, value);
        }
        public string ForumName { get; set; }
        public int Index { get; set; }
    }
}
