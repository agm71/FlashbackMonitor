using ReactiveUI;
using System.Collections.Generic;

namespace FlashbackMonitor.ViewModels
{
    public class CategoryWithForumsViewModel : ViewModelBase
    {
        private string _category;
        public string Category
        {
            get => _category;
            set => this.RaiseAndSetIfChanged(ref _category, value);
        }

        public string ForumColor { get; set; }

        public List<NotificationViewModel> Notifications { get; set; }
    }
}