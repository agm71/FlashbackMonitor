using ReactiveUI;

namespace FlashbackMonitor.ViewModels
{
    public class ForumViewModel(MainWindowViewModel mainWindowViewModel) : ViewModelBase
    {
        public string Name { get; set; }
        public string ForumColor { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set {
                this.RaiseAndSetIfChanged(ref _isChecked, value);
                mainWindowViewModel.RaiseNotificationForAllChecked();
            }
        }
    }
}