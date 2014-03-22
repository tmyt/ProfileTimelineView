using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace ProfileTimelineView.Twitter
{
    public class TwitterDownloader : DataDownloader
    {
        // アプリを識別するトークン
        private const string ConsumerKey = "HLdvteSMuoZ3m8YxzvLqA";
        private const string ConsumerSecret = "XOOU2Z5fAXI9Pn35M7iLLgzIQ4ERNs0FWnRiHy3Ko";

        // エンドポイントの定義
        private const string OAuth2Token = "https://api.twitter.com/oauth2/token";
        private const string StatusesUserTimeline = "https://api.twitter.com/1.1/statuses/user_timeline.json";

        // そのほかの定数
        private const string DateTimeFormat = "ddd MMM dd HH:mm:ss +0000 yyyy";

        // プロパティの定義
        public string ScreenName { get; set; }

        private async Task<string> GetAccessTokenAsync()
        {
            // 認証情報を設定
            var credentials = BuildCredentials();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            // リクエストを送信
            var content =
                new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("grant_type", "client_credentials")});
            var response = await client.PostAsync(OAuth2Token, content);
            // レスポンスを解析
            var json = JsonObject.Parse(await response.Content.ReadAsStringAsync());
            return json.GetNamedString("access_token");
        }

        private string BuildCredentials()
        {
            var original = string.Format("{0}:{1}", WebUtility.UrlEncode(ConsumerKey),
                WebUtility.UrlEncode(ConsumerSecret));
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(original));
        }

        private DateTime ParseCreatedString(string created)
        {
            var dst = TimeSpan.FromHours(TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? 1 : 0);
            return DateTime.ParseExact(created.Trim(), DateTimeFormat, new CultureInfo("en-us"))
                .Add(TimeZoneInfo.Local.BaseUtcOffset.Add(dst));
        }

        public override async Task<List<TimelineData>> GetTimelineAsync()
        {
            // 認証情報を取得
            var token = await GetAccessTokenAsync();
            var client = new HttpClient {MaxResponseContentBufferSize = 1024*1024};
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // リクエストを送信
            var response = await client.GetAsync(string.Format("{0}?screen_name={1}",
                StatusesUserTimeline, WebUtility.UrlEncode(ScreenName)));
            // レスポンスを解析
            var json = JsonValue.Parse(await response.Content.ReadAsStringAsync());
            var statuses = json.GetArray();
            return statuses.Select(status => status.GetObject())
                .Select(status =>
                {
                    var text = status.GetNamedString("text");
                    var user = status.GetNamedObject("user");
                    var created = status.GetNamedString("created_at");
                    var name = user.GetNamedString("name");
                    var screenname = user.GetNamedString("screen_name");
                    var profileImageUrl = user.GetNamedString("profile_image_url_https");
                    return new TweetData
                    {
                        Title = string.Format("{0} / @{1}", name, screenname),
                        Body = text,
                        ImageUri = new Uri(profileImageUrl),
                        Name = name,
                        ScreenName = screenname,
                        CreatedAt = ParseCreatedString(created)
                    };
                })
                .Cast<TimelineData>()
                .ToList();
        }

        public static DataDownloader Create(string screenName)
        {
            var downloader = new TwitterDownloader {ScreenName = screenName};
            return downloader;
        }
    }
}