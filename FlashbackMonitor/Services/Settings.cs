using FlashbackMonitor.ViewModels;
using System.Collections.Generic;

namespace FlashbackMonitor.Services
{
    public class Settings
    {
        public List<string> Forums { get; set; } = [];
        public List<TopicViewModel> Topics { get; set; } = [];
        public List<UserViewModel> Users { get; set; } = [];
        public int Interval { get; set; } = 10;
        public string Theme { get; set; }
    }
}
