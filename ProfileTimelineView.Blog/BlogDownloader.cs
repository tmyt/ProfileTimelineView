using System.Collections.Generic;
using System.Linq;
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

        public override async Task<List<TimelineData>> GetTimelineAsync()
        {
            var feed = new SyndicationFeed();
            feed.Load(Url);
            return feed.Items.Select(item => new BlogEntryData
                {
                    Title = item.Title.Text,
                    Body = item.Summary.Text,
                    EntryUrl = item.ItemUri.AbsoluteUri
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
