using ReactiveUI;

namespace FlashbackMonitor.ViewModels
{
    public class UserViewModel : ViewModelBase
    {
        private string _userName;
        public string UserName
        {
            get => _userName;
            set => this.RaiseAndSetIfChanged(ref _userName, value);
        }

        private bool _favorite;
        public bool Favorite
        {
            get => _favorite;
            set
            {
                this.RaiseAndSetIfChanged(ref _favorite, value);
            }
        }

        private bool _vipUser;
        public bool VipUser
        {
            get => _vipUser;
            set => this.RaiseAndSetIfChanged(ref _vipUser, value);
        }

        private bool _ignoredUser;
        public bool IgnoredUser
        {
            get => _ignoredUser;
            set => this.RaiseAndSetIfChanged(ref _ignoredUser, value);
        }
    }
}