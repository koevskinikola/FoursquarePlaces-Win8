using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FoursquarePlaces.Resources;
using Windows.Devices.Geolocation;
using Newtonsoft.Json;
// Directive for the ViewModel.
using LocalDatabaseSample.Model;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Phone.Scheduler;

namespace FoursquarePlaces
{
    public partial class MainPage : PhoneApplicationPage
    {
        public double lon;
        public double ltd;
        WebClient client;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.DataContext = App.ViewModel;

            client = new WebClient();
            client.DownloadStringCompleted += client_DownloadStringCompleted;
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            StartTracking();
            setMeOnMap();                  
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            App.ViewModel.SaveChangesToDB();   
        }

        private async void setMeOnMap()
        {
            Geolocator MyGeolocator = new Geolocator();
            MyGeolocator.DesiredAccuracyInMeters = 5;
            Geoposition myGeoPosition = null;
            try
            {
                myGeoPosition = await MyGeolocator.GetGeopositionAsync(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10));
            }
            catch (Exception)
            {
                MessageBox.Show("Location is disabled in phone settings");
            }

            this.lon = myGeoPosition.Coordinate.Longitude;
            this.ltd = myGeoPosition.Coordinate.Latitude;

            // Set the center on the map
            this.placesMap.Center = new GeoCoordinate(myGeoPosition.Coordinate.Latitude, myGeoPosition.Coordinate.Longitude);
            this.placesMap.ZoomLevel = 15;

            getResult();
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                RootObject rtObj = JsonConvert.DeserializeObject<RootObject>(e.Result);
                List<Venue> venues = rtObj.response.venues;

                foreach (Venue v in venues)
                {
                    App.ViewModel.AddVenueToCollection(new VenueItem { VenueName = v.name, Latitude = v.location.lat, Longitude = v.location.lng });
                }

                setPushpins();
            }
        }

        public void getResult()
        {
            string strRequestUri = null;
            if (lon == 0.0 || ltd == 0.0)
                strRequestUri = "https://api.foursquare.com/v2/venues/search?ll=42.0031,21.4054&radius=250&oauth_token=EIRAL4CH3IFGKYZQVCO5HDIW5JNB3YJSLNDHX4II2HCAWTMT&v=20130324";
            else
                strRequestUri = "https://api.foursquare.com/v2/venues/search?ll=" + ltd.ToString() + "," + lon.ToString() + "&radius=250&oauth_token=EIRAL4CH3IFGKYZQVCO5HDIW5JNB3YJSLNDHX4II2HCAWTMT&v=20130324";
            string strResult = string.Empty;

            client.DownloadStringAsync(new Uri(strRequestUri));

            
        }

        public void setPushpins()
        {
            if (lon == 0.0 && ltd == 0.0)
            {
                this.ltd = 42.0031;
                this.lon = 21.4054;
            }
            GeoCoordinate p1 = new GeoCoordinate(this.ltd, this.lon);

            // Show center map
            this.placesMap.Center = p1;
            this.placesMap.ZoomLevel = 17;
            DrawPushpin(p1, Colors.DarkGray);

            foreach (VenueItem v in App.ViewModel.VenueItems)
            {
                DrawPushpin(new GeoCoordinate(v.Latitude, v.Longitude), Colors.Black);
            }
        }

        private void DrawPushpin(GeoCoordinate coord, Color color)
        {
            var aPushpin = CreatePushpinObject(color);

            //Creating a MapOverlay and adding the Pushpin to it.
            MapOverlay MyOverlay = new MapOverlay();
            MyOverlay.Content = aPushpin;
            MyOverlay.GeoCoordinate = coord;
            MyOverlay.PositionOrigin = new Point(0, 0.5);
            MapLayer layer = new MapLayer();
            layer.Add(MyOverlay);

            // Add the MapOverlay containing the pushpin to the MapLayer
            this.placesMap.Layers.Add(layer);
        }

        private Grid CreatePushpinObject(Color color)
        {
            //Creating a Grid element.
            Grid MyGrid = new Grid();
            MyGrid.RowDefinitions.Add(new RowDefinition());
            MyGrid.RowDefinitions.Add(new RowDefinition());
            MyGrid.Background = new SolidColorBrush(Colors.Transparent);

            //Creating a Rectangle
            Rectangle MyRectangle = new Rectangle();
            MyRectangle.Fill = new SolidColorBrush(color);
            MyRectangle.Height = 20;
            MyRectangle.Width = 20;
            MyRectangle.SetValue(Grid.RowProperty, 0);
            MyRectangle.SetValue(Grid.ColumnProperty, 0);

            //Adding the Rectangle to the Grid
            MyGrid.Children.Add(MyRectangle);

            //Creating a Polygon
            Polygon MyPolygon = new Polygon();
            MyPolygon.Points.Add(new Point(2, 0));
            MyPolygon.Points.Add(new Point(22, 0));
            MyPolygon.Points.Add(new Point(2, 40));
            MyPolygon.Stroke = new SolidColorBrush(color);
            MyPolygon.Fill = new SolidColorBrush(color);
            MyPolygon.SetValue(Grid.RowProperty, 1);
            MyPolygon.SetValue(Grid.ColumnProperty, 0);

            //Adding the Polygon to the Grid
            MyGrid.Children.Add(MyPolygon);
            return MyGrid;
        }

        private void addToVisitButton_Click(object sender, RoutedEventArgs e)
        {
            // Cast the parameter as a button.
            var button = sender as Button;

            if (button != null)
            {
                // Get a handle for the venue item bound to the button.
                VenueItem venueForAdd = button.DataContext as VenueItem;

                App.ViewModel.AddVenueItem(venueForAdd);
                App.ViewModel.DeleteVenueFromCollection(venueForAdd);
            };

            // Put the focus back to the main page.
            this.Focus();
        }

        private void btnWishLst_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/PlacesToVisitPage.xaml", UriKind.Relative));
        }

        #region Background location tracking

        PeriodicTask periodicTask = null;
        string periodicTaskName = "Destination";

        private void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
            }
            catch (Exception)
            {
            }
        }

        private void StartTracking()
        {
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

            if (periodicTask != null && !periodicTask.IsEnabled)
            {
                MessageBox.Show("Background agents for this application have been disabled by the user.");
                return;
            }

            if (periodicTask != null && periodicTask.IsEnabled)
            {
                RemoveAgent(periodicTaskName);
            }

            periodicTask = new PeriodicTask(periodicTaskName);

            periodicTask.Description = "New GeoCoordinates";
            ScheduledActionService.Add(periodicTask);

            #if(DEBUG_AGENT)
                    ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(60));
            #endif
        }

        #endregion
    }
}