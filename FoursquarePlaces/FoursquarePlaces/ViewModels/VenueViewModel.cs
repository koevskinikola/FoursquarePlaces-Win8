using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

// Directive for the data model.
using LocalDatabaseSample.Model;
using Windows.Devices.Geolocation;
using FoursquarePlaces;
using System;
using Newtonsoft.Json;


namespace LocalDatabaseSample.ViewModel
{
    public class VenueViewModel : INotifyPropertyChanged
    {
        private VenueDataContext venueDB;

        // Class constructor, create the data context object.
        public VenueViewModel(string venueDBConnectionString)
        {
            venueDB = new VenueDataContext(venueDBConnectionString);
        }

        //
        // TODO: Add collections, list, and methods here.
        //

        // All venue items.
        private ObservableCollection<VenueItem> _placesItems;
        public ObservableCollection<VenueItem> PlacesItems
        {
            get { return _placesItems; }
            set
            {
                _placesItems = value;
                NotifyPropertyChanged("PlacesItems");
            }
        }

        private ObservableCollection<VenueItem> _venueItems;
        public ObservableCollection<VenueItem> VenueItems
        {
            get { return _venueItems; }
            set
            {
                _venueItems = value;
                NotifyPropertyChanged("VenueItems");
            }
        }

        public void AddVenueToCollection(VenueItem newVenueItem)
        {
            if (VenueItems == null) VenueItems = new ObservableCollection<VenueItem>();
            VenueItems.Add(newVenueItem);
        }

        public void DeleteVenueFromCollection(VenueItem venueForDelete)
        {
            VenueItems.Remove(venueForDelete);
        }

        public void LoadCollectionsFromDatabase()
        {
            var venueItemsInDB = from VenueItem venue in venueDB.Items
                                select venue;

            PlacesItems = new ObservableCollection<VenueItem>(venueItemsInDB);
        }

        // Add a venue item to the database and collections.
        public void AddVenueItem(VenueItem newVenueItem)
        {
            venueDB.Items.InsertOnSubmit(newVenueItem);
            venueDB.SubmitChanges();

            PlacesItems.Add(newVenueItem);
        }

        // Remove a venue task item from the database and collections.
        public void DeleteVenueItem(VenueItem venueForDelete)
        {
            PlacesItems.Remove(venueForDelete);
            venueDB.Items.DeleteOnSubmit(venueForDelete);
            venueDB.SubmitChanges();
        }

        // Write changes in the data context to the database.
        public void SaveChangesToDB()
        {
            venueDB.SubmitChanges();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the app that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}