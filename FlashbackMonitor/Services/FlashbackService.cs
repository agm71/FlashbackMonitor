using FlashbackMonitor.Utils;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace FlashbackMonitor.Services
{
    public class FlashbackService : IFlashbackService
    {
        private const string FlashbackBaseUrl = "https://www.flashback.org/";
        private const int ForumsCount = 140;

#pragma warning disable CS1998
        public async Task<IEnumerable<FlashbackDataItem>> GetFlashbackDataAsync()
#pragma warning restore CS1998
        {
            var html = string.Empty;

            using (var client = new HttpClient())
            {
                // För lokal testning
                //html = File.ReadAllText(@"c:\tmp\fb.html");

                // För prod
                var response = await client.GetAsync($"{FlashbackBaseUrl}");
                html = await response.Content.ReadAsStringAsync();
            }

            var matches = FlashbackRegexes.StartPageRegex().Matches(html).ToList();

            if (matches.Count != ForumsCount)
            {
                throw new Exception();
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
                    TopicLastUpdated = HttpUtility.HtmlDecode(matches[i].Groups["Time"].Value).Substring(0, 10),
                    TopicLastUpdatedDateTime = ToDateTime(HttpUtility.HtmlDecode(matches[i].Groups["Time"].Value)),
                    ForumColor = string.IsNullOrWhiteSpace(matches[i].Groups["Color"].Value) ? items[^1].ForumColor : matches[i].Groups["Color"].Value,
                    ForumCategory = string.IsNullOrWhiteSpace(matches[i].Groups["Category"].Value) ? items[^1].ForumCategory : matches[i].Groups["Category"].Value
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

#pragma warning disable CS1998
        public async Task<TopicPage> GetTopicPageAsync(string topicUrl)
#pragma warning restore CS1998
        {
            // Lokal testning
            //var doc = new HtmlDocument();
            //doc.Load(@"c:\tmp\riktiglista.html");

            // Prod
            HtmlWeb web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(topicUrl);

            var topicPage = FlashbackParser.ParseTopicsPage(doc);

            return topicPage;
        }
    }
}
