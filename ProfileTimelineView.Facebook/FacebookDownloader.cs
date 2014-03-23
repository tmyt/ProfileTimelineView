using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Authentication.Web;

namespace ProfileTimelineView.Facebook
{
    public class FacebookDownloader : DataDownloader
    {
        // エンドポイントの定義
        private const string DialogOAuth = "https://www.facebook.com/dialog/oauth";
        private const string OAuthAccessToken = "https://graph.facebook.com/oauth/access_token";
        private const string GraphMe = "https://graph.facebook.com/me";

        // クライアントIDの定義
        private const string ClientId = "721012307939618";
        private const string ClientSecret = "1cdce97a521de236686be9c8131e864a";
        private const string RedirectUri = "https://redirect.facebook.profiletimelineview.example.com/";

        // フィールドの定義
        private static string _accessToken;

        private async Task<string> GrantAsync()
        {
            // 認証ダイアログを表示
            var url = BuildUri(DialogOAuth, new
            {
                client_id = ClientId,
                redirect_uri = RedirectUri,
                scope = "read_stream"
            });
            var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None,
                new Uri(url), new Uri(RedirectUri));
            if (result.ResponseStatus != WebAuthenticationStatus.Success) return null;
            // レスポンスを解析
            var query = result.ResponseData.Split('?').Skip(1).FirstOrDefault().Split('#').FirstOrDefault();
            var param = query.ParseQueryString();
            // トークンを要求
            url = BuildUri(OAuthAccessToken, new
            {
                client_id = ClientId,
                client_secret = ClientSecret,
                redirect_uri = RedirectUri,
                code = param["code"]
            });
            try
            {
                var client = new HttpClient();
                var token = await client.GetStringAsync(url);
                // トークンを取り出し
                var tokenParams = token.ParseQueryString();
                return tokenParams["access_token"];
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine(e.Message);
            }
            return null;
        }

        private async Task<string> GetRedirectUri(string url)
        {
            var handler = new HttpClientHandler { AllowAutoRedirect = false };
            var client = new HttpClient(handler);
            var response = await client.GetAsync(url);
            return response.Headers.Location.AbsoluteUri;
        }

        public override async Task<List<TimelineData>> GetTimelineAsync()
        {
            // 認証トークンの取得
            if (string.IsNullOrEmpty(_accessToken))
            {
                _accessToken = await GrantAsync();
            }
            if (_accessToken == null) return new List<TimelineData>();
            // ユーザアイコンを取得
            var pictureUrl = BuildUri(GraphMe + "/picture", new { access_token = _accessToken, type = "normal" });
            var userPicture = await GetRedirectUri(pictureUrl);
            // feedを取得
            var url = BuildUri(GraphMe + "/feed", new { access_token = _accessToken });
            var client = new HttpClient { MaxResponseContentBufferSize = 1024 * 1024 };
            var json = JsonObject.Parse(await client.GetStringAsync(url));

            // データを解析
            var data = json.GetNamedArray("data");
            return data.Select(d => d.GetObject())
                .Select(d =>
                {
                    var from = d.GetNamedObject("from");
                    var name = from.GetNamedString("name");
                    var picture = d.GetNamedString("picture") ?? userPicture;
                    var message = d.GetNamedString("message");
                    return new WallEntryData
                    {
                        Body = message,
                        ImageUri = new Uri(picture),
                        Title = name
                    };
                })
                .Cast<TimelineData>()
                .ToList();
        }

        public static DataDownloader Create()
        {
            return new FacebookDownloader();
        }

        public static void Revoke()
        {
            _accessToken = null;
        }
    }
}
