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
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
    class ChartActivity : AppCompatActivity
    {
        DrawerLayout drawerLayout;
        NavigationView navigationView;

        bool logged_in = false;
        int selectedNavItem = 0;

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
            //SupportActionBar.Title = dataList[0].Temp.ToString();
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.NavigationItemSelected += NavigationView_NavigationItemSelected;

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);
            drawerLayout.AddDrawerListener(drawerToggle);
            drawerToggle.SyncState();
            navigationView.SetCheckedItem(Resource.Id.nav_config);
            selectedNavItem = Resource.Id.nav_chart;
        }

        void NavigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.MenuItem.ItemId)
            {
                case (Resource.Id.nav_home):
                    navigationView.SetCheckedItem(Resource.Id.nav_home);
                    StartActivity(new Intent(this, typeof(MainActivity)).SetFlags(ActivityFlags.ReorderToFront));
                    break;
                case (Resource.Id.nav_chart):

                    break;
                case (Resource.Id.nav_config):
                    if (!logged_in)
                    {
                        Toast.MakeText(Application.Context, "please sign in first!", ToastLength.Short).Show();
                        navigationView.SetCheckedItem(selectedNavItem);
                    }
                    else
                    {
                        StartActivity(new Intent(this, typeof(ConfigActivity)).SetFlags(ActivityFlags.ReorderToFront));
                    }
                    break;
                case (Resource.Id.nav_devices):

                    break;
            }
            selectedNavItem = e.MenuItem.ItemId;
            // Close drawer
            drawerLayout.CloseDrawers();
        }

        private void SetUpChart()
        {
            //Temperature
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

            //Humidity
            //PlotView viewHumidity = FindViewById<PlotView>(Resource.Id.plot_viewHumidity);

            //PlotModel plotModelHumidity = new PlotModel { Title = "Humidity" };

            //plotModelTemp.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
            //plotModelTemp.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 10, Minimum = 0 });

            //LineSeries seriesHumidity = new LineSeries
            //{
            //    MarkerType = MarkerType.Circle,
            //    MarkerSize = 4,
            //    MarkerStroke = OxyColors.White
            //};

            //seriesHumidity.Points.Add(new DataPoint(0.0, 6.0));
            //seriesHumidity.Points.Add(new DataPoint(1.4, 2.1));
            //seriesHumidity.Points.Add(new DataPoint(2.0, 4.2));
            //seriesHumidity.Points.Add(new DataPoint(3.3, 2.3));
            //seriesHumidity.Points.Add(new DataPoint(4.7, 7.4));
            //seriesHumidity.Points.Add(new DataPoint(6.0, 6.2));
            //seriesHumidity.Points.Add(new DataPoint(8.9, 8.9));

            //plotModelTemp.Series.Add(seriesHumidity);

            //viewHumidity.Model = plotModelHumidity;
        }

        public string _formatter(double d)
        {
            return dataList[Convert.ToInt32(d)].Time;
        }

        public void RetrieveRecords()
        {
            HttpWebRequest webRequest = WebRequest.Create("http://bard.bplaced.net/api") as HttpWebRequest;
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