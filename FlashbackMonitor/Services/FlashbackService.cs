using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace FlashbackMonitor.Services
{
    public class FlashbackService : IFlashbackService
    {
        private const string FlashbackBaseUrl = "https://www.flashback.org/";
        private const int ForumsCount = 140;

        public async Task<List<FlashbackDataItem>> GetFlashbackDataAsync()
        {
            var html = string.Empty;
        makeRequest:
            using (var client = new HttpClient())
            {

                try
                {
                    var response = await client.GetAsync($"{FlashbackBaseUrl}");
                    html = await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    await Task.Delay(3000);
                    goto makeRequest;
                }
            }

            var matches = Regex.Matches(html, "(?:(?:<span class=\"fa fa-circle\" style=\"color:)(?<Color>.*?)\">(?:</span>)(?:.|\\s)*?)?^(?:forumslist.|\\s*?<a href=\")(?<ForumUrl>.+?)\"(?:\\s.*?<strong>)(?<ForumName>.*?)</strong>(?:.|\\s)*?(?:<strong>\\s*?<a href=\")(?<TopicUrl>.*?)\"\\s.*?title.*?>(?<TopicName>.*?)</a>(?:.|\\s)*?av\\s<a href=.*?>(?<UserName>.*?)</a>\\s(?<Time>.*?)\\s*?</div>", RegexOptions.Multiline).ToList();

            if (matches.Count != ForumsCount)
            {
                await Task.Delay(3000);
                goto makeRequest;
            }

            List<FlashbackDataItem> items = [];
            for (int i = 0; i < matches.Count; i++)
            {
                items.Add(new FlashbackDataItem
                {
                    Index = i,
                    ForumName = matches[i].Groups["ForumName"].Value,
                    TopicName = HttpUtility.HtmlDecode(matches[i].Groups["TopicName"].Value),
                    TopicUrl = $"{FlashbackBaseUrl}{matches[i].Groups["TopicUrl"].Value}",
                    UserName = HttpUtility.HtmlDecode(matches[i].Groups["UserName"].Value),
                    TopicLastUpdated = HttpUtility.HtmlDecode(matches[i].Groups["Time"].Value),
                    TopicLastUpdatedDateTime = ToDateTime(HttpUtility.HtmlDecode(matches[i].Groups["Time"].Value)),
                    ForumColor = string.IsNullOrWhiteSpace(matches[i].Groups["Color"].Value) ? items[^1].ForumColor : matches[i].Groups["Color"].Value
                });
            }

            return items;
        }

        private static DateTime ToDateTime(string d)
        {
            if (d.Contains("Idag "))
            {
                var timeParts = d[(d.IndexOf("Idag ") + 5)..].Split(":").Select(d => int.Parse(d)).ToList();
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, timeParts[0], timeParts[1], 0);
            }
            else if (d.Contains("Igår "))
            {
                var timeParts = d[(d.IndexOf("Igår ") + 5)..].Split(":").Select(d => int.Parse(d)).ToList();
                DateTime yesterday = DateTime.Now.AddDays(-1);
                return new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, timeParts[0], timeParts[1], 0);
            }

            return DateTime.Parse(d);
        }
    }
}