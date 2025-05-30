﻿using System.Text.RegularExpressions;

namespace FlashbackMonitor
{
    public static partial class FlashbackRegexes
    {
        [GeneratedRegex("(?:(?:<span class=\"fa fa-circle\" style=\"color:)(?<Color>.*?)\">(?:</span>(?:.|\\s*?)<a class=\"forum-title\"(?:.|\\s)*?)\">(?<Category>.*?)</a>(?:.|\\s)*?)?^(?:forumslist.|\\s*?<a href=\")(?<ForumUrl>.+?)\"(?:\\s.*?<strong>)(?<ForumName>.*?)</strong>(?:.|\\s)*?(?:<strong>\\s*?<a href=\")(?<TopicUrl>.*?)\"\\s.*?title.*?>(?<TopicName>.*?)</a>(?:.|\\s)*?av\\s<a href=.*?>(?<UserName>.*?)</a>\\s(?<Time>.*?)\\s*?</div>", RegexOptions.Multiline)]
        public static partial Regex StartPageRegex();

        [GeneratedRegex("^[^\r\n\t]*", RegexOptions.Multiline)]
        public static partial Regex PostDateRegex();
    }
}