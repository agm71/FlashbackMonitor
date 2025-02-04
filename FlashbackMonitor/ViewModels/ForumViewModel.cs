using ReactiveUI;

namespace FlashbackMonitor.ViewModels
{
    public class ForumViewModel : ViewModelBase
    {
        private bool _isChecked;

        public string Name { get; set; }
        public string ForumColor { get; set; }
        public bool IsChecked
        {
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }
    }
}
