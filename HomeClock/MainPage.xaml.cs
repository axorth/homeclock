using HomeClock.WeatherModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Syndication;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HomeClock
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer _dispatcherTimer;
        private DateTime _currentDay = DateTime.Now.AddDays(-2);
        private int? _temp = null;

        public MainPage()
        {
            this.InitializeComponent();
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            _dispatcherTimer.Start();

            
            
        }

        private async void _dispatcherTimer_Tick(object sender, object e)
        {
            var now = DateTime.Now;

            if (now.Second == 15 || _temp == null)
            {
                try
                {
                    var weather = await CallRestService("http://api.openweathermap.org/data/2.5/weather?q=Plano&appid=7a9f540de6321da219e71ebe98f805db");
                    _temp = (int)(9.0 / 5.0 * (weather.main.temp - 273) + 32);
                    txtTemp.Text = $"{_temp}°";
                    if (weather.weather.Any())
                    {
                        txtWeather.Text = weather.weather[0].description;
                    }
                    
                }
                catch (Exception)
                {
                    //do nothing i don't care
                }
                
            }

            
            txtClock.Text = $"{now.Hour % 12:00}:{now.Minute:00}";
            

            if (now.Date > _currentDay)
            {
                //XmlReader reader = XmlReader.Create("https://www.biblegateway.com/votd/get/?format=atom");
                SyndicationClient client = new SyndicationClient();
                var feed = await client.RetrieveFeedAsync(new Uri("https://www.biblegateway.com/votd/get/?format=atom")).AsTask();
                
                foreach (SyndicationItem item in feed.Items)
                {
                    // do what you want with the feed data
                    string verseText = WebUtility.HtmlDecode( item.Content.Text);
                    string verseTitle = item.Title.Text;

                    txtVerse.Text = verseTitle;
                    txtBible.Text = verseText;
   
                }
       
                _currentDay = now.Date;
            }


        }

        private async Task<WeatherModel> CallRestService(string uri)
        {
            dynamic result;

            var req = HttpWebRequest.Create(uri);
            req.ContentType = "application/json";
            
            using (var resp = await req.GetResponseAsync())
            {
                var results = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                result = JsonConvert.DeserializeObject<WeatherModel>(results);
            }

            return result;
        }


    }
}
