using ReactiveUI;

namespace FlashbackMonitor.ViewModels
{
    public class TopicViewModel : ViewModelBase
    {
        private string _topicName;
        
        public string TopicName
        {
            get => _topicName;
            set => this.RaiseAndSetIfChanged(ref _topicName, value);
        }

        private bool _isFavoriteTopic;

        public bool IsFavoriteTopic
        {
            get => _isFavoriteTopic;
            set => this.RaiseAndSetIfChanged(ref _isFavoriteTopic, value);
        }
    }
}