using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace NetWork
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool isXml = false;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(isXml)
            {
                SearchIPXml();
            }
            else
            {
                SearchIPJson();
            }
        }

        private async void SearchIPXml()
        {
            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            StringBuilder message = new StringBuilder();
            Uri requestUri = new Uri(message.Append("http://ip-api.com/xml/").Append(Search.Text).ToString());
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            string httpResponseBody = "";
            try
            {
                //Send the GET request
                httpResponseMessage = await httpClient.GetAsync(requestUri);
                httpResponseMessage.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(httpResponseBody);
            XmlNodeList xmlNodes = xmlDocument.SelectNodes("query");
            message.Clear();
            foreach (var item in xmlNodes)
            {
                message.Append("IP : ").Append(item.SelectNodes("query")[0].InnerText).Append("\n").
                    Append("Country : ").Append(item.SelectNodes("country")[0].InnerText).Append("\n").
                    Append("Country code : ").Append(item.SelectNodes("countryCode")[0].InnerText).Append("\n").
                    Append("Region : ").Append(item.SelectNodes("regionName")[0].InnerText).Append("\n").
                    Append("Region code : ").Append(item.SelectNodes("region")[0].InnerText).Append("\n").
                    Append("City : ").Append(item.SelectNodes("city")[0].InnerText).Append("\n").
                    Append("Method : ").Append("xml").Append("\n");
                break;
            }
            show.Text = message.ToString();
        }

        private async void SearchIPJson()
        {
            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            StringBuilder message = new StringBuilder();
            Uri requestUri = new Uri(message.Append("https://www.sojson.com/open/api/weather/json.shtml?city=").Append(Search.Text).ToString());
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            string httpResponseBody = "";
            try
            {
                //Send the GET request
                httpResponseMessage = await httpClient.GetAsync(requestUri);
                httpResponseMessage.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            JObject jObject = JObject.Parse(httpResponseBody);
            message.Clear();
            message.Append("城市 : ").Append(jObject["city"]).Append("\n");
            foreach (var items in (jObject["data"] as JObject))
            {
                if (items.Key.Equals("yesterday"))
                {
                    message.Append("\n\n昨天 : ");
                    foreach(var item in (items.Value as JObject))
                    {
                        message.Append(item.Value).Append(" ");
                    }
                }
                else if(items.Key.Equals("forecast"))
                {
                    message.Append("\n\n");
                    foreach (var item in items.Value)
                    {
                        foreach (var it in (item as JObject))
                        {
                            message.Append(it.Value).Append(" ");
                        }
                        message.Append("\n\n");
                    }
                }
                else
                {
                    message.Append(items.Value).Append(" ");
                }
            }
            show.Text = message.ToString();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if ((string)radioButton.Content == "查询天气")
            {
                isXml = false;
            }
            else
            {
                isXml = true;
            }
        }
    }
}
