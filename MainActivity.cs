using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Support.V7.App;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using OxyPlot;
using OxyPlot.Xamarin.Android;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace ArduinoMonitor
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    class ChartActivity : AppCompatActivity
    {
        string hostURL = "http://example.net/api"   //change to wherever you retrieve the sensor values

        List<Record> dataList = new List<Record>();
        Records recordList;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_chart);

            RetrieveRecords();
            SetUpChart();

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }

        
        private void SetUpChart()
        {
            PlotView viewTemp = FindViewById<PlotView>(Resource.Id.plot_viewTemp);
            PlotModel plotModelTemp = new PlotModel { Title = "Temperature/Humidity" };

            plotModelTemp.Axes.Add(
                new LinearAxis { 
                    Position = AxisPosition.Bottom, 
                    AxislineStyle = LineStyle.Solid, 
                    LabelFormatter = _formatter
                }
            );
            plotModelTemp.Axes.Add(
                new LinearAxis { 
                    Position = AxisPosition.Left, 
                    Maximum = 40, 
                    Minimum = 0,
                    AbsoluteMaximum = 60,
                    AbsoluteMinimum = -20,
                }
            );

            LineSeries seriesTemp = new LineSeries
            {
                LabelFormatString = "{1}°C",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White,
                Smooth = true
            };
            LineSeries seriesHumidity = new LineSeries
            {
                LabelFormatString = "{1}%",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White,
                Smooth = true
            };
            int tmp = 0;
            foreach (var item in dataList)
            {
                seriesTemp.Points.Add(new DataPoint(tmp, item.Temp));
                seriesHumidity.Points.Add(new DataPoint(tmp, item.Humidity));
                tmp++;
            }
            seriesTemp.TrackerFormatString = "{4}°C , {2}";

            plotModelTemp.Series.Add(seriesTemp);
            plotModelTemp.Series.Add(seriesHumidity);

            viewTemp.Model = plotModelTemp;
        }

        public string _formatter(double d)
        {
            return dataList[Convert.ToInt32(d)].Time;
        }

        public void RetrieveRecords()
        {
            HttpWebRequest webRequest = WebRequest.Create(hostURL) as HttpWebRequest;
            if (webRequest == null)
            {
                return;
            }

            webRequest.ContentType = "application/json";
            webRequest.UserAgent = "Nothing";

            using (Stream s = webRequest.GetResponse().GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    string dataAsJson = sr.ReadToEnd();
                    recordList = JsonConvert.DeserializeObject<Records>(dataAsJson);
                    foreach (var item in recordList.RecordList)
                    {
                        dataList.Add(item);
                    }
                }
            }
        }
    }
}
