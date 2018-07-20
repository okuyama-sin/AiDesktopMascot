using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace desktopmascot
{
    /// <summary>
    /// SubWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DocomoSubWindow : Window
    {
        private string botId,appid;
        private JObject aidjobj;
        private string talk, reply;
        private int hougenindex = 0;
                   
        private MediaPlayer mp = new MediaPlayer();


        public DocomoSubWindow()
        {
            InitializeComponent();
            this.Top = 200;
            this.Left = 500;
            textbox1.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            textbox2.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }

        private void Button1_click(object sender, RoutedEventArgs e)
        {
            int index = combo1.SelectedIndex;
            switch (index)
            {
                case 0:
                    Itokaisyaku();
                    break;

                case 1:
                    Tisiki();
                    break;
                case 2:
                    Character();
                    break;
                case 3:
                    TrendNews();
                    break;
                case 4:
                    Keitaiso();
                    break;
            }
        }


        private static async Task<JObject> AppIDPost(string botid)
        {
            string url = "https://api.apigw.smt.docomo.ne.jp/naturalTaskRecog/v1/registration" +
                "?APIKEY=[APIキーを入力してください]";

            var jsonpost = "{ \"botId\" : \"" + botid + "\" , \"appKind\" : \"PC\"  }";

            HttpClient hc = new HttpClient();
            var content = new StringContent(jsonpost, Encoding.UTF8, "application/json");
            var response = await hc.PostAsync(url, content);
            byte[] binary = await response.Content.ReadAsByteArrayAsync();
            var jsontext = Encoding.UTF8.GetString(binary, 0, binary.Length);

            JObject jobj = JObject.Parse(jsontext);

            return jobj;

        }

        private static async Task<string> HttpPost(string botid,string talk, string aid,int hougen)
        {
            string url = "https://api.apigw.smt.docomo.ne.jp/naturalTaskRecog/v1/dialogue" +
                "?APIKEY=[APIキーを入力してください]";

            StringContent jsonpost;

            if (botid == "CharaConv")
            {
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
            }
            else
            {
                jsonpost = new StringContent(JsonConvert.SerializeObject(new
                {
                    language = "ja-JP",
                    botId = botid,
                    appId = aid,
                    voiceText = talk,
                    appRecvTime = DateTime.Now.ToString("yyyy-MM-DD HH:MM:ss"),
                    appSendTime = DateTime.Now.ToString("yyyy-MM-DD HH:MM:ss")
                }), Encoding.UTF8, "application/json");
            }


            HttpClient hc = new HttpClient();

            var response = await hc.PostAsync(url, jsonpost);
            byte[] binary = await response.Content.ReadAsByteArrayAsync();
            var jsontext = Encoding.UTF8.GetString(binary, 0, binary.Length);

            JObject jobj = JObject.Parse(jsontext);
            string reply = (string)jobj["systemText"]["expression"];

            return reply;
        }

        private void Itokaisyaku()
        {
            if (botId != "TaskRecog")
            {
                botId = "TaskRecog";
                aidjobj = Task.Run(() => AppIDPost(botId)).Result;
                appid = aidjobj.First.First.ToString();
            }
            talk = textbox1.Text;
            reply = Task.Run(() => HttpPost(botId, talk, appid,hougenindex)).Result;
            textbox2.Text = reply + "\n\n" + textbox2.Text;
            if (Voice_check.IsChecked == true)
            {
                talk = textbox2.Text;
                ConvMain(reply);
                textbox2.Text = talk;
            }
        }

        private void Tisiki()
        {
            if (botId != "Knowledge")
            {
                botId = "Knowledge";
                aidjobj = Task.Run(() => AppIDPost(botId)).Result;
                appid = aidjobj.First.First.ToString();
            }
            talk = textbox1.Text;
            reply = Task.Run(() => HttpPost(botId, talk, appid,hougenindex)).Result;
            textbox2.Text = reply + "\n\n" + textbox2.Text;
            if (Voice_check.IsChecked == true)
            {
                talk = textbox2.Text;
                ConvMain(reply);
                textbox2.Text = talk;
            }
        }

        private void Character()
        {
            hougenindex = combo2.SelectedIndex;
            if (botId != "CharaConv")
            {
                botId = "CharaConv";
                aidjobj = Task.Run(() => AppIDPost(botId)).Result;
                appid = aidjobj.First.First.ToString();
            }
            talk = textbox1.Text;
            reply = Task.Run(() => HttpPost(botId, talk, appid,hougenindex)).Result;
            textbox2.Text = reply + "\n\n" + textbox2.Text;
            if (Voice_check.IsChecked == true)
            {
                talk = textbox2.Text;
                ConvMain(reply);
                textbox2.Text = talk;
            }
        }

        private void TrendNews()
        {
            string contenturl = "https://api.apigw.smt.docomo.ne.jp/webCuration/v3/contents?" +
                "genreId=1&" +
     "APIKEY=[APIキーを入力してください]";

            string contentjson = new HttpClient().GetStringAsync(contenturl).Result;
            Console.WriteLine(contentjson);

            JObject contentjobj = JObject.Parse(contentjson);

            for (int i = 0; i < contentjobj["articleContents"].Count(); i++)
            {
                string title = (string)contentjobj["articleContents"][i]["contentData"]["title"];
                string body = (string)contentjobj["articleContents"][i]["contentData"]["body"];
                string url = (string)contentjobj["articleContents"][i]["contentData"]["url"];
                textbox2.Text = title + "\n" + body + "\n" + url + "\n\n" + textbox2.Text;
            }
        }

        private void Keitaiso()
        {
            talk = textbox1.Text;
            reply = Task.Run(() => Keitaiso_HttpPost(talk)).Result;
            textbox2.Text = reply + "\n\n" + textbox2.Text;
            if (Voice_check.IsChecked == true)
            {
                talk = textbox2.Text;
                ConvMain(reply);
                textbox2.Text = talk;
            }
        }

        private static async Task<string> Keitaiso_HttpPost(string talk)
        {
            string url = "https://api.apigw.smt.docomo.ne.jp/gooLanguageAnalysis/v1/morph" +
                "?APIKEY=[APIキーを入力してください]";

            StringContent jsonpost = new StringContent(JsonConvert.SerializeObject(new
            {
                sentence = talk
            }), Encoding.UTF8, "application/json");

            HttpClient hc = new HttpClient();

            var response = await hc.PostAsync(url, jsonpost);
            byte[] binary = await response.Content.ReadAsByteArrayAsync();
            var jsontext = Encoding.UTF8.GetString(binary, 0, binary.Length);

            JObject jobj = JObject.Parse(jsontext);
            string reply = jobj["word_list"].ToString();

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
                        textbox2.Text = textbox1.Text;
                    }
                    else
                    {
                        textbox2.Text = "音声が取得できませんでした。";
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

    }
}
