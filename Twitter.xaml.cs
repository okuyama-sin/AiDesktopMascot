using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CoreTweet;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;

namespace desktopmascot
{
    /// <summary>
    /// Twitter.xaml の相互作用ロジック
    /// </summary>
    public partial class Twitter : Window
    {
        private string appid, talk;
        public Twitter()
        {
            InitializeComponent();
            tweetlist.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }

        private void button1_click(object sender,RoutedEventArgs e)
        {
            var tokens = Tokens.Create("[APIkey]",
                            "[APIsecret]",
                            "[Access Token]",
                            "[Access Token secret]");

            var home = tokens.Trends.Place(1117817);
            JArray ahome = JArray.Parse(home.Json);

            int tcount = ahome[0]["trends"].Count();
            string[] trend = new string[tcount];

            for (int i = 0; i < tcount; i++)
            {
                trend[i] = (string)ahome[0]["trends"][i]["name"];
            }

            foreach (var value in trend)
            {
                trendlist.Items.Add(value);
            }

        }

        private void Textselect(object sender, SelectionChangedEventArgs e)
        {
            if (trendlist.SelectedItem == null)
            {
                return;
            }
            searchtext.Text = "";
            searchtext.Text =trendlist.SelectedValue.ToString();
            trendlist.UnselectAll();

        }

        private void button3_click(object sender,RoutedEventArgs e)
        {
            tweetlist.Clear();

            var keyword = searchtext.Text;

            var tokens = Tokens.Create("[APIkey]",
                            "[APIsecret]",
                            "[Access Token]",
                            "[Access Token secret]");

            var result = Task.Run(() => SearchTweet(tokens, keyword)).Result;
            int hougen = combo1.SelectedIndex;

            if (appid == null)
            {
                JObject aidjobj = Task.Run(() => AppIDPost()).Result;
                appid = aidjobj.First.First.ToString();
            }

            foreach (var tweet in result)
            {
                string reply = "";
                talk = tweet.User.ScreenName + " : " + tweet.Text;

                reply = Task.Run(() => HttpPost(talk, appid, hougen)).Result;
                tweetlist.Text += reply + "\n\n";
            }


        }

        private async Task<SearchResult> SearchTweet(Tokens tokens,string keyword)
        {
            var result = await tokens.Search.TweetsAsync(count => 10,q => keyword);
            return result;
        }

        private static async Task<JObject> AppIDPost()
        {
            string url = "https://api.apigw.smt.docomo.ne.jp/naturalCharaConv/v1/registration" +
                "?APIKEY=[APIキー]";

            var jsonpost = "{ \"botId\" : \"CharaConv\" , \"appKind\" : \"PC\"  }";

            HttpClient hc = new HttpClient();
            var content = new StringContent(jsonpost, Encoding.UTF8, "application/json");
            var response = await hc.PostAsync(url, content);
            byte[] binary = await response.Content.ReadAsByteArrayAsync();
            var jsontext = Encoding.UTF8.GetString(binary, 0, binary.Length);

            JObject jobj = JObject.Parse(jsontext);

            return jobj;

        }
        private static async Task<string> HttpPost(string talk, string aid, int hougen)
        {
            string url = "https://api.apigw.smt.docomo.ne.jp/naturalCharaConv/v1/dialogue" +
                "?APIKEY=[APIキー]";

            StringContent jsonpost;
                string hougenstr = "";
                switch (hougen)
                {
                    case 0:
                        hougenstr = "ehime1";
                        break;
                    case 1:
                        hougenstr = "ehime2";
                        break;
                    case 2:
                        hougenstr = "ehime3";
                        break;
                    case 3:
                        hougenstr = "kansai";
                        break;
                    case 4:
                        hougenstr = "hakata";
                        break;
                    case 5:
                        hougenstr = "fukushima";
                        break;
                    case 6:
                        hougenstr = "mie";
                        break;
                }

                var jsondic = new Dictionary<string, object>()
                {
                    {"language","ja-JP" },
                    { "botId", "CharaConv" },
                    { "appId", aid },
                    { "voiceText", talk }
                };

                jsondic.Add("clientData", new Dictionary<string, object>()
                    {
                        {
                            "option", new Dictionary<string,string>(){ { "t",hougenstr} }
                        }
                    });

                jsondic.Add("appRecvTime", DateTime.Now.ToString("yyyy-MM-dd HH:MM:ss"));
                jsondic.Add("appSendTime", DateTime.Now.ToString("yyyy-MM-dd HH:MM:ss"));

                jsonpost = new StringContent(JsonConvert.SerializeObject(jsondic));

            HttpClient hc = new HttpClient();

            var response = await hc.PostAsync(url, jsonpost);
            byte[] binary = await response.Content.ReadAsByteArrayAsync();
            var jsontext = Encoding.UTF8.GetString(binary, 0, binary.Length);

            JObject jobj = JObject.Parse(jsontext);
            string reply = (string)jobj["systemText"]["expression"];

            return reply;
        }
    }
}
