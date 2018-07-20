using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.ServiceModel.Syndication;

namespace desktopmascot
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private string hello;
        private string baseurl, parameter, value, url, json, souryo;
        private string tenki, text, kyou_tenki;
        private string tosyokan, libkey;
        private string syubetu;
        private string line;
        private string talk, reply,appid,botid;
        private JObject jobj;
        private CollectionViewSource view = new CollectionViewSource();
        private ObservableCollection<ItemList> itemLists = new ObservableCollection<ItemList>();
        private ObservableCollection<NewsList> newsLists = new ObservableCollection<NewsList>();
        private BitmapImage bi1 = new BitmapImage(new Uri("cut1.png", UriKind.Relative));
        private BitmapImage bi2 = new BitmapImage(new Uri("cut2.png", UriKind.Relative));
        private BitmapImage emon1 = new BitmapImage(new Uri("\\Emotion\\id-1_sad.png", UriKind.Relative));
        private BitmapImage emop1 = new BitmapImage(new Uri("\\Emotion\\id1_happy.png", UriKind.Relative));
        private BitmapImage emon2 = new BitmapImage(new Uri("\\Emotion\\id-2_dislike.png", UriKind.Relative));
        private BitmapImage emop2 = new BitmapImage(new Uri("\\Emotion\\id2_like.png", UriKind.Relative));
        private BitmapImage emon3 = new BitmapImage(new Uri("\\Emotion\\id-3_anxiety.png", UriKind.Relative));
        private BitmapImage emon5 = new BitmapImage(new Uri("\\Emotion\\id-5_surprise.png", UriKind.Relative));
        private BitmapImage emon6 = new BitmapImage(new Uri("\\Emotion\\id-6_amazed.png", UriKind.Relative));
        private BitmapImage emop6 = new BitmapImage(new Uri("\\Emotion\\id6_angry.png", UriKind.Relative));
        private BitmapImage emon8 = new BitmapImage(new Uri("\\Emotion\\id-8_contempt.png", UriKind.Relative));
        private BitmapImage emop8 = new BitmapImage(new Uri("\\Emotion\\id8_respect.png", UriKind.Relative));
        private BitmapImage emop10 = new BitmapImage(new Uri("\\Emotion\\id10_yeah.png", UriKind.Relative));
        private MediaPlayer mp = new MediaPlayer();
        private Clock clock = new Clock();
        private DocomoSubWindow Dsub = new DocomoSubWindow();
        private Twitter twitterwindow = new Twitter();

        public MainWindow()
        {
            InitializeComponent();
            var desktop = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Top = desktop.Height - 700;
            this.Left = desktop.Width - 900;

            int dtn = int.Parse(DateTime.Now.Hour.ToString());
            if (dtn >= 4 && dtn <= 10)
            {
                hello = "おはようございます！";
            }
            else if (dtn > 10 && dtn <= 18)
            {
                hello = "こんにちは！";
            }
            else if(dtn >18 && dtn<=24)
            {
                hello = "こんばんわ！";
            }
            else if (dtn >=0 && dtn < 4)
            {
                hello = "こんばんわ！";
            }

            textblock.Text = hello + "\n今日はどうしますか？";
            VisiblHidden();
            talk = textblock.Text;
            ConvMain(talk);
            textblock.Text = talk;
            image1.Source = bi1;
            clock.Show();
        }

        private void DocomoAI_clicked(object sender, RoutedEventArgs e)
        {
            Dsub.Show();
        }

        private void Twitter_clicked(object sender,RoutedEventArgs e)
        {
            twitterwindow.Show();
        }

        private void tenki_clicked(object sender, RoutedEventArgs e)
        {
            VisiblHidden();
            button3.Visibility = Visibility.Visible;
            button4.Visibility = Visibility.Visible;

            image1.Source = bi1;
            baseurl = "http://weather.livedoor.com/forecast/webservice/json/v1?city=230010";
            json = new HttpClient().GetStringAsync(baseurl).Result;

            jobj = JObject.Parse(json);

            tenki = (string)jobj["forecasts"][0]["telop"];
            text = (string)jobj["description"]["text"];
            kyou_tenki = "今日の天気は" + tenki + "です。\n" + text;

            textblock.Text = kyou_tenki;
        }

        private void button3_click(object sender, RoutedEventArgs e)
        {
            talk = textblock.Text;
            ConvMain(talk);
            textblock.Text = talk;
        }

        private void button4_click(object sender,RoutedEventArgs e)
        {
            
            string[] arraystr = textblock.Text.Split('\n');
            textblock.Text = "";

            if (appid == null)
            {
                JObject aidjobj = Task.Run(() => Hakata_AppIDPost()).Result;
                appid = aidjobj.First.First.ToString();
            }

            foreach (var str in arraystr)
            {
                string reply = "";

                reply = Task.Run(() => Hakata_HttpPost(str, appid)).Result;
                if (reply == "undefined")
                {
                    reply = "";
                }
                textblock.Text += reply + "\n";
            }

            talk = textblock.Text;
            ConvMain(talk);
            textblock.Text = talk;
        }

        private static async Task<JObject> Hakata_AppIDPost()
        {
            string url = "https://api.apigw.smt.docomo.ne.jp/naturalCharaConv/v1/registration" +
                "?APIKEY=[APIキーを入力してください]";

            var jsonpost = "{ \"botId\" : \"CharaConv\" , \"appKind\" : \"PC\"  }";

            HttpClient hc = new HttpClient();
            var content = new StringContent(jsonpost, Encoding.UTF8, "application/json");
            var response = await hc.PostAsync(url, content);
            byte[] binary = await response.Content.ReadAsByteArrayAsync();
            var jsontext = Encoding.UTF8.GetString(binary, 0, binary.Length);

            JObject jobj = JObject.Parse(jsontext);

            return jobj;

        }
        private static async Task<string> Hakata_HttpPost(string talk, string aid)
        {
            string url = "https://api.apigw.smt.docomo.ne.jp/naturalCharaConv/v1/dialogue" +
                "?APIKEY=[APIキーを入力してください]";

            StringContent jsonpost;

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
                            "option", new Dictionary<string,string>(){ { "t","hakata"} }
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

        protected void Rakuten_clicked(object sender, RoutedEventArgs e)
        {
            VisiblHidden();

            image1.Source = bi1;
            syubetu = "rakuten";
            textblock.Text = "商品名を入力してください。";
            textbox.Visibility = Visibility.Visible;
            button1.Visibility = Visibility.Visible;
        }
        protected void Tokai_clicked(object sender, RoutedEventArgs e)
        {
            VisiblHidden();

            image1.Source = bi1;
            syubetu = "tosyo";
            tosyokan = "Aichi_Tokai";
            libkey = "中央";
            textblock.Text = "ISBNコードを入力してください。";
            textbox.Visibility = Visibility.Visible;
            button1.Visibility = Visibility.Visible;
        }
        protected void Obu_clicked(object sender, RoutedEventArgs e)
        {
            VisiblHidden();

            image1.Source = bi1;
            syubetu = "tosyo";
            tosyokan = "Aichi_Obu";
            libkey = "図書館";
            textblock.Text = "ISBNコードを入力してください。";
            textbox.Visibility = Visibility.Visible;
            button1.Visibility = Visibility.Visible;
        }

        protected void button1_clicked(object sender, RoutedEventArgs e)
        {
            image1.Source = bi2;


            if (syubetu == "rakuten")
            {
                textblock.Text = "一覧を出力しました。";
                listView1.Visibility = Visibility.Visible;
                listView1.DataContext = null;
                itemLists.Clear();
                baseurl = "https://app.rakuten.co.jp/services/api/IchibaItem/Search/20170706?" +
                    "applicationId=[アプリケーションキーを入力してください]&";
                parameter = "keyword";
                value = textbox.Text;

                if (textbox.Text == "")
                {
                    MessageBox.Show("キーワードが入力されていません", "エラー");
                    return;
                }

                url = $"{baseurl}{parameter}={value}&imageFlag=1&";
                json = new HttpClient().GetStringAsync(url).Result;

                jobj = JObject.Parse(json);

                if (jobj["Items"].Count() == 0)
                {
                    MessageBox.Show("商品が見つかりませんでした。", "エラー");
                    return;
                }

                for (int i = 0; i < (int)jobj["hits"]; i++)
                {

                    if ((int)jobj["Items"][i]["Item"]["postageFlag"] == 0)
                    {
                        souryo = "送料込";
                    }
                    else if ((int)jobj["Items"][i]["Item"]["postageFlag"] == 1)
                    {
                        souryo = "送料別";
                    }


                    itemLists.Add(new ItemList()
                    {
                        Itemname = (string)jobj["Items"][i]["Item"]["itemName"],
                        Catchcopy = (string)jobj["Items"][i]["Item"]["catchcopy"],
                        Price = (int)jobj["Items"][i]["Item"]["itemPrice"],
                        Souryo = souryo,
                        Url = (string)jobj["Items"][i]["Item"]["itemUrl"]
                    });

                }
                view.Source = itemLists;
                listView1.DataContext = view;          
            }
            else if (syubetu == "tosyo")
            {
                textblock.Text = "検索中です・・・";
                string baseurl = "http://api.calil.jp/check?appkey=[APPキーを入力してください]&isbn=";
                string parameter = tosyokan;
                string value = textbox.Text;

                if (textbox.Text == "")
                {
                    MessageBox.Show("ISBNが入力されていません", "エラー");
                    return;
                }

                string url = $"{baseurl}{value}&systemid={parameter}&format=json&callback=no";
                string json = new HttpClient().GetStringAsync(url).Result;

                JObject jobj = JObject.Parse(json);

                string cont = (string)jobj["continue"];

                while (cont == "1")
                {
                    Thread.Sleep(2000);
                    json = new HttpClient().GetStringAsync(url).Result;
                    jobj = JObject.Parse(json);
                    cont = (string)jobj["continue"];
                }

                var libk = jobj["books"][value][parameter]["libkey"];

                if (libk == null)
                {
                    textblock.Text = ("本はありませんでした。");
                }
                else
                {
                    
                    if (libk.HasValues)
                    {
                        textblock.Text = "現時点で" + (string)libk[libkey] + "です。";
                    }
                    else
                    {
                        textblock.Text = ("本はありませんでした。");
                    }
                }
            }
            else if(syubetu == "rss")
            {
                if (textbox.Text == "")
                {
                    MessageBox.Show("URLが入力されていません", "エラー");
                    return;
                }

                StreamWriter sw = new StreamWriter(new Uri("rss.text", UriKind.Relative).ToString(), true, Encoding.ASCII);
                sw.Write(textbox.Text + "\n");
                sw.Close();
                textblock.Text = "登録しました。";
            }

        }

        private void Rss_add(object sender,RoutedEventArgs e)
        {
            VisiblHidden();

            image1.Source = bi1;
            syubetu = "rss";
            textblock.Text = "RSSのURLを入力してください。";
            textbox.Visibility = Visibility.Visible;
            button1.Visibility = Visibility.Visible;

        }

        private void Rss_load(object sender, RoutedEventArgs e)
        {
            VisiblHidden();

            image1.Source = bi1;
            listBox.Items.Clear();
            newsLists.Clear();
            line = "";

            textblock.Text = "どれを読み込みますか？";
            yomi_button.Visibility = Visibility.Visible;
            listBox.Visibility = Visibility.Visible;
            Stream fl = File.OpenRead(new Uri("rss.text", UriKind.Relative).ToString());
            using (StreamReader sr = new StreamReader(fl, Encoding.ASCII))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    listBox.Items.Add(line);
                }
            }
            fl.Close();
        }

        private void Yomi_button(object sender,RoutedEventArgs e)
        {
            textblock.Text = "RSSを読み込みました！";
            image1.Source = bi2;
            listBox.Visibility = Visibility.Hidden;
            listView2.Visibility = Visibility.Visible;

            url = listBox.SelectedValue.ToString();

            using (XmlReader rdr = XmlReader.Create(url))
            {
                SyndicationFeed feed = SyndicationFeed.Load(rdr);

                foreach (SyndicationItem item in feed.Items)
                {
                    newsLists.Add(new NewsList
                    {
                        news = item.Title.Text,
                        nurl = item.Links[0].Uri.AbsoluteUri
                    });                    
                }
            }

            view.Source = newsLists;
            listView2.DataContext = view;
        }

        private void Communication(object sender,RoutedEventArgs e)
        {
            VisiblHidden();

            image1.Source = bi1;
            textblock.Text = "なんでしょう？";
            textbox.Visibility = Visibility.Visible;
            button2.Visibility = Visibility.Visible;
            clalischeck.Visibility = Visibility.Visible;
            syubetu = "Communicate";
        }

        private void Docomo_comm(object sender,RoutedEventArgs e)
        {
            VisiblHidden();

            image1.Source = bi1;
            textblock.Text = "なんでしょう？";
            textbox.Visibility = Visibility.Visible;
            button2.Visibility = Visibility.Visible;
            clalischeck.Visibility = Visibility.Visible;
            syubetu = "docomo_comm";
        }

        private void Conversation(object sender, RoutedEventArgs e)
        {
            VisiblHidden();

            image1.Source = bi1;
            textblock.Text = "なんでしょう？";
            textbox.Visibility = Visibility.Visible;
            button2.Visibility = Visibility.Visible;
            syubetu = "Conversation";
        }


        private void Button2_clicked(object sender,RoutedEventArgs e)
        {
            talk = textbox.Text;
            
            if (syubetu == "Communicate")
            {
                byte[] jsonb = Task.Run(() => HttpPost(talk)).Result;
                json = Encoding.UTF8.GetString(jsonb);

                jobj = JObject.Parse(json);

                if ((int)jobj["status"] != 0)
                {
                    textblock.Text = (string)jobj["message"];
                    return;
                }

                reply = (string)jobj["results"][0]["reply"];

                if (clalischeck.IsChecked == true)
                {
                    ClalisEmotion();
                    return;
                }
                
                textblock.Text = reply;
                talk = textblock.Text;
                ConvMain(talk);
                textblock.Text = talk;
            }
            else if(syubetu == "Textsuggest")
            {
                listbox2.Items.Clear();

                if (textbox1.Text != null)
                {
                    talk = textbox1.Text + talk;
                }
                if(textbox1.Text.Length >= 500)
                {
                    MessageBox.Show("500文字以上は送信できません。", "エラー");
                    return;
                }

                url = $"https://api.a3rt.recruit-tech.co.jp/text_suggest/v2/predict?apikey=[APIキーを入力してください]"
                    + $"&previous_description={talk}";
                json = new HttpClient().GetStringAsync(url).Result;

                jobj = JObject.Parse(json);

                if (jobj["suggestion"].Count() == 0)
                {
                    MessageBox.Show("続きを作れませんでした。", "エラー");
                }

                string[] reply = new string[3];

                for (int i = 0; i < jobj["suggestion"].Count(); i++)
                {
                    reply[i] = (string)jobj["suggestion"][i];
                    listbox2.Items.Add(reply[i]);
                }
                textblock.Text = "どれにしますか？";
                
            }
            else if (syubetu == "docomo_comm")
            {
                Doc_comm_Main(talk);
            }
            else if (syubetu == "Conversation")
            {
                ConvMain(talk);
            }

        }

        private static async Task<byte[]> HttpPost(string talk)
        {
            string url = "https://api.a3rt.recruit-tech.co.jp/talk/v1/smalltalk";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"apikey","[APIキーを入力してください]" },
                {"query",talk }
            });

            var hc = new HttpClient();

            var response = await hc.PostAsync(url, content);

            return await response.Content.ReadAsByteArrayAsync();
        }

        private void ClalisEmotion()
        {
            
            json = Task.Run(() => ClalisHttpPost(reply)).Result;
            string result = JsonConvert.DeserializeObject<string>(json);
            string[] splres = result.Split(';');

            ConvMain(reply);

            ReplyAsync(splres);
        }

        private static async Task<string> ClalisHttpPost(string talk)
        {
            string url = "https://liplis.mine.nu/Clalis/v41/Json/ClalisEmotional.aspx";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"sentence",talk }
            });

            var hc = new HttpClient();

            var response = await hc.PostAsync(url, content);

            return await response.Content.ReadAsStringAsync();
        }

        private async void ReplyAsync(string[] splres)
        {
            textblock.Text = "";
            foreach (string msg in splres)
            {
                string[] reslist = msg.Split(',');

                if (reslist.Length == 3)
                {
                    textblock.Text += reslist[0];
                    switch (int.Parse(reslist[1]))
                    {
                        case 0:
                            break;
                        case 1:
                            if (int.Parse(reslist[2]) < 0)
                            {
                                image1.Source = emon1;
                            }
                            else
                            {
                                image1.Source = emop1;
                            }
                            break;
                        case 2:
                            if (int.Parse(reslist[2]) < 0)
                            {
                                image1.Source = emon2;
                            }
                            else
                            {
                                image1.Source = emop2;
                            }
                            break;
                        case 3:
                            if (int.Parse(reslist[2]) < 0)
                            {
                                image1.Source = emon3;
                            }
                            break;
                        case 5:
                            if (int.Parse(reslist[2]) < 0)
                            {
                                image1.Source = emon5;
                            }
                            break;
                        case 6:
                            if (int.Parse(reslist[2]) < 0)
                            {
                                image1.Source = emon6;
                            }
                            else
                            {
                                image1.Source = emop6;
                            }
                            break;
                        case 8:
                            if (int.Parse(reslist[2]) < 0)
                            {
                                image1.Source = emon8;
                            }
                            else
                            {
                                image1.Source = emop8;
                            }
                            break;
                        case 10:
                            if (int.Parse(reslist[2]) < 0)
                            {
                                image1.Source = bi1;
                            }
                            else
                            {
                                image1.Source = emop10;
                            }
                            break;
                        default:
                            image1.Source = bi1;
                            break;
                    }
                }
                await Task.Delay(100);
            }
        }

        private void Textsuggest (object sender, RoutedEventArgs e)
        {
            VisiblHidden();

            image1.Source = bi1;
            textblock.Text = "なんでしょう？";
            textbox.Visibility = Visibility.Visible;
            button2.Visibility = Visibility.Visible;
            textbox1.Visibility = Visibility.Visible;
            listbox2.Visibility = Visibility.Visible;
            syubetu = "Textsuggest";
        }

        private void Textselect(object sender, SelectionChangedEventArgs e)
        {
            if (listbox2.SelectedItem == null)
            {
                return;
            }
            textbox1.Text = "";
            textbox1.Text += talk + listbox2.SelectedValue.ToString();
            textbox.Text = "";
            listbox2.UnselectAll();
            
        }

        private void Doc_comm_Main(string talk)
        {
            if (appid == null)
            {
                botid = "Chatting";
                JObject aidjobj = Task.Run(() => Doc_comm_AppIDPost(botid)).Result;
                appid = (string)aidjobj.First.First.ToString();
            }

            reply = Doc_comm_HttpPost(talk, appid);

            if (clalischeck.IsChecked == true)
            {
                ClalisEmotion();
                return;
            }

            textblock.Text = reply;
            talk = textblock.Text;
            ConvMain(talk);
            textblock.Text = talk;


        }
        
        private static async Task<JObject> Doc_comm_AppIDPost(string botid)
        {
            string url = "https://api.apigw.smt.docomo.ne.jp/naturalChatting/v1/registration" +
                "?APIKEY=[APIキーを入力してください]";

            var jsonpost = "{ \"botId\" : \""+ botid + "\" , \"appKind\" : \"PC\"  }";

            HttpClient hc = new HttpClient();
            var content = new StringContent(jsonpost, Encoding.UTF8, "application/json");
            var response = await hc.PostAsync(url, content);
            byte[] binary = await response.Content.ReadAsByteArrayAsync();
            var jsontext = Encoding.UTF8.GetString(binary, 0, binary.Length);

            JObject jobj = JObject.Parse(jsontext);

            return jobj;

        }

        private static string Doc_comm_HttpPost(string talk, string aid)
        {
            string url = "https://api.apigw.smt.docomo.ne.jp/naturalChatting/v1/dialogue" +
                "?APIKEY=[APIキーを入力してください]";



            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);

            wr.ContentType = "application/json;charset=UTF-8";
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();
            string responseStr = null;


            string formitem = "{\"language\":\"ja-JP\",\"botId\":\"Chatting\",\"appId\":\"" + aid + "\",\"voiceText\":\"" + talk + "\",\"clientData\"";
            formitem += ":{\"option\":{\"place\":\"名古屋\",\"mode\":\"dialog\"}},\"appRecvTime\":\""
                + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"appSendTime\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
            Console.WriteLine(formitem);
            byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
            rs.Write(formitembytes, 0, formitembytes.Length);
            rs.Close();


            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                responseStr = reader2.ReadToEnd();
            }
            catch (Exception ex)
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }

            Console.WriteLine(responseStr);
            var res = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(responseStr);
            var systemText = (Newtonsoft.Json.Linq.JObject)res.GetValue("systemText");
            string reply = (string)systemText.GetValue("expression");
            return reply;
        }

        static HttpMethod apiMethod = HttpMethod.Post;
        static string apiEndPointURL = "https://api.apigw.smt.docomo.ne.jp/aiTalk/v1/textToSpeech?APIKEY=";
        static string apiKey = "[APIキーを入力してください]";
        static string apiContentType = "application/ssml+xml"; 
        static string apiAccept = "audio/L16";
        
        private void ConvMain(string talk)
        {
            mp.Close();
            string textBySSML = @"<?xml version=""1.0"" encoding=""utf-8"" ?>

            <speak version=""1.1"">
              <voice name=""maki"">" +
                  talk +
              "</voice>" +
            "</speak>";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            byte[] bytesBySSML = Encoding.UTF8.GetBytes(textBySSML);

            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(apiMethod, apiEndPointURL + apiKey);

                request.Content = new ByteArrayContent(bytesBySSML);

                request.Content.Headers.ContentType = new MediaTypeHeaderValue(apiContentType);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(apiAccept));
               
                var sbResponseHeaders = new StringBuilder();

                var isSuccessResponse = false;

                try
                {
                    var asyncSendTask = client.SendAsync(request);
                    asyncSendTask.Wait();

                    sbResponseHeaders.AppendLine($"HTTP-Status-Code: {asyncSendTask.Result.StatusCode}");
                    foreach (var headerItem in asyncSendTask.Result.Headers)
                    {
                        var headkey = headerItem.Key;
                        var headValue = String.Join(",", headerItem.Value);
                        sbResponseHeaders.AppendLine($"{headkey}: {headValue}");
                    }

                    if (asyncSendTask.Result.StatusCode == HttpStatusCode.OK)
                    {
                        isSuccessResponse = true;

                        var msgResponseBody = asyncSendTask.Result.Content;
                        var stmResponseBody = msgResponseBody.ReadAsStreamAsync().GetAwaiter().GetResult();
                        var wavByte = ToWavStream(stmResponseBody);

                        using (FileStream fs = File.Create(new Uri("myFile.wav", UriKind.Relative).ToString()))
                        {
                            fs.Write(wavByte, 0, wavByte.Length);
                        }
                        
                        mp.Open(new Uri("myFile.wav", UriKind.Relative));
                        mp.Play();
                        

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("エラー発生！\n" + ex.Message);
                }
                finally
                {
                    if (isSuccessResponse)
                    {
                        image1.Source = bi2;
                        textblock.Text = textbox.Text;
                    }
                    else
                    {
                        image1.Source = emon2;
                        textblock.Text = "音声が取得できませんでした。";
                    }
                }
            }
        }

        /// <summary>
        /// 音声の再生
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="index"></param>
        public static byte[] ToWavStream(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Byte[] lnByte = reader.ReadBytes((int)stream.Length);
                Byte[] wavByte = convertBytesEndian(lnByte);
                return wavByte;
            }

        }

        private static byte[] convertBytesEndian(byte[] bytes)
        {
            byte[] newBytes = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i += 2)
            {
                newBytes[i] = bytes[i + 1];
                newBytes[i + 1] = bytes[i];
            }
            // 44byte付加したnewBytes
            newBytes = addWAVHeader(newBytes);
            return newBytes;
        }

        private static byte[] addWAVHeader(byte[] bytes)
        {
            byte[] header = new byte[44];
            // サンプリングレート
            long longSampleRate = 16000;
            // チャンネル数
            int channels = 1;
            int bits = 16;
            // データ速度
            long byteRate = longSampleRate * (bits / 8) * channels;
            long dataLength = bytes.Length;
            long totalDataLen = dataLength + 36;
            // 最終的なWAVファイルのバイナリ
            byte[] finalWAVBytes = new byte[bytes.Length + header.Length];
            int typeSize = System.Runtime.InteropServices.Marshal.SizeOf(bytes.GetType().GetElementType());

            header[0] = convertByte("R");
            header[1] = convertByte("I");
            header[2] = convertByte("F");
            header[3] = convertByte("F");
            header[4] = (byte)(totalDataLen & 0xff);
            header[5] = (byte)((totalDataLen >> 8) & 0xff);
            header[6] = (byte)((totalDataLen >> 16) & 0xff);
            header[7] = (byte)((totalDataLen >> 24) & 0xff);
            header[8] = convertByte("W");
            header[9] = convertByte("A");
            header[10] = convertByte("V");
            header[11] = convertByte("E");
            header[12] = convertByte("f");
            header[13] = convertByte("m");
            header[14] = convertByte("t");
            header[15] = convertByte(" ");
            header[16] = 16;
            header[17] = 0;
            header[18] = 0;
            header[19] = 0;
            header[20] = 1;
            header[21] = 0;
            header[22] = (byte)channels;
            header[23] = 0;
            header[24] = (byte)(longSampleRate & 0xff);
            header[25] = (byte)((longSampleRate >> 8) & 0xff);
            header[26] = (byte)((longSampleRate >> 16) & 0xff);
            header[27] = (byte)((longSampleRate >> 24) & 0xff);
            header[28] = (byte)(byteRate & 0xff);
            header[29] = (byte)((byteRate >> 8) & 0xff);
            header[30] = (byte)((byteRate >> 16) & 0xff);
            header[31] = (byte)((byteRate >> 24) & 0xff);
            header[32] = (byte)((bits / 8) * channels);
            header[33] = 0;
            header[34] = (byte)bits;
            header[35] = 0;
            header[36] = convertByte("d");
            header[37] = convertByte("a");
            header[38] = convertByte("t");
            header[39] = convertByte("a");
            header[40] = (byte)(dataLength & 0xff);
            header[41] = (byte)((dataLength >> 8) & 0xff);
            header[42] = (byte)((dataLength >> 16) & 0xff);
            header[43] = (byte)((dataLength >> 24) & 0xff);

            System.Buffer.BlockCopy(header, 0, finalWAVBytes, 0, header.Length * typeSize);
            System.Buffer.BlockCopy(bytes, 0, finalWAVBytes, header.Length * typeSize, bytes.Length * typeSize);

            return finalWAVBytes;
        }

        private static byte convertByte(string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str)[0];
        }

        private void Quit(object sender,RoutedEventArgs e)
        {
            image1.Source = bi2;
            textblock.Text = "お疲れさまでした！";
            talk = textblock.Text;
            ConvMain(talk);
            textblock.Text = talk;
            QuitAsync();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private async void QuitAsync()
        {
            await Task.Delay(2000);
            this.Close();
            clock.Close();
            Dsub.Close();
            twitterwindow.Close();
        }

        private void VisiblHidden()
        {
            textbox.Visibility = Visibility.Hidden;
            button1.Visibility = Visibility.Hidden;
            button2.Visibility = Visibility.Hidden;
            button3.Visibility = Visibility.Hidden;
            button4.Visibility = Visibility.Hidden;
            listView1.Visibility = Visibility.Hidden;
            listView2.Visibility = Visibility.Hidden;
            listBox.Visibility = Visibility.Hidden;
            yomi_button.Visibility = Visibility.Hidden;
            textbox1.Visibility = Visibility.Hidden;
            listbox2.Visibility = Visibility.Hidden;
            clalischeck.Visibility = Visibility.Hidden;      
        }

    }

    class ItemList
    { 
        public string Itemname { get; set; }
        public string Catchcopy { get; set; }
        public int Price { get; set; }
        public string Souryo { get; set; }
        public string Url { get; set; }
    }
    class NewsList
    {
        public string news { get; set; }
        public string nurl { get; set; }
    }
        
}
