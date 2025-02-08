using System;

namespace FlashbackMonitor.Services
{
    public class FlashbackDataItem
    {
        public int Index { get; set; }
        public string ForumName {  get; set; }
        public string ForumColor { get; set; }
        public string TopicName { get; set; }
        public string TopicUrl { get; set; }
        public string TopicLastUpdated { get; set; }
        public DateTime TopicLastUpdatedDateTime { get; set; }
        public string UserName { get; set; }
    }
}