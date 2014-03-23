using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace ProfileTimelineView.Blog
{
    public class BlogDownloader : DataDownloader
    {
        // デフォルトのブログURL
        private const string DefaultBlogUrl = "http://blogs.msdn.com/b/microsoft_japan_academic/rss.aspx";

        // プロパティ定義
        public string Url { get; set; }

        private string DecodeHtml(string html)
        {
            var tags = new Regex("<.*?>", RegexOptions.Singleline);
            var refs = new Regex("&.*?;");
            var text = tags.Replace(html, "");
            return refs.Replace(text, "");
        }

        public override async Task<List<TimelineData>> GetTimelineAsync()
        {
            var client = new SyndicationClient();
            var feed = await client.RetrieveFeedAsync(new Uri(Url));
            return feed.Items.Select(item => new BlogEntryData
                {
                    Title = item.Title.Text,
                    Body = DecodeHtml(item.Summary.Text),
                    EntryUrl = (item.Links != null &&item.Links.Count > 0) ? item.Links.First().Uri.AbsoluteUri : ""
                })
                .Cast<TimelineData>()
                .ToList();
        }

        public static DataDownloader Create()
        {
            return Create(DefaultBlogUrl);
        }

        public static DataDownloader Create(string url)
        {
            var downloader = new BlogDownloader { Url = url };
            return downloader;
        }
    }
}
