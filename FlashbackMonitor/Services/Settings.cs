using System.Collections.Generic;

namespace FlashbackMonitor.Services
{
    public class Settings
    {
        public List<string> Forums { get; set; } = [];
        public List<string> Topics { get; set; } = [];
        public List<string> FavoriteTopics { get; set; } = [];
        public int Interval { get; set; } = 10;
    }
}
