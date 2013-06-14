using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using LocalDatabaseSample.Model;
using Windows.Devices.Geolocation;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FoursquarePlaces
{
    public partial class PlacesToVisitPage : PhoneApplicationPage
    {

        public double lon;
        public double ltd;

        public PlacesToVisitPage()
        {
            InitializeComponent();
            this.DataContext = App.ViewModel;
            setMeOnMap();
            setPushpins();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            setMeOnMap();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Save changes to the database.
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
            this.toVisitMap.Center = new GeoCoordinate(myGeoPosition.Coordinate.Latitude, myGeoPosition.Coordinate.Longitude);
            this.toVisitMap.ZoomLevel = 15;
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
            this.toVisitMap.Center = p1;
            this.toVisitMap.ZoomLevel = 17;
            DrawPushpin(p1, Colors.DarkGray);

            foreach (VenueItem v in App.ViewModel.PlacesItems)
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
            this.toVisitMap.Layers.Add(layer);
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

        private void deleteVenueButton_Click(object sender, RoutedEventArgs e)
        {
            // Cast the parameter as a button.
            var button = sender as Button;

            if (button != null)
            {
                // Get a handle for the to-do item bound to the button.
                VenueItem venueForDelete = button.DataContext as VenueItem;

                App.ViewModel.DeleteVenueItem(venueForDelete);
            }

            // Put the focus back to the main page.
            this.Focus();
        }

        private void btnWishLst_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}