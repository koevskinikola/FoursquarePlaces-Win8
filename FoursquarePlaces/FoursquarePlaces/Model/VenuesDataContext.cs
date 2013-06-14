using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace LocalDatabaseSample.Model
{

    [Table]
    public class VenueItem : INotifyPropertyChanged, INotifyPropertyChanging
    {

        // Define ID: private field, public property, and database column.
        private int _venueItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int VenueItemId
        {
            get { return _venueItemId; }
            set
            {
                if (_venueItemId != value)
                {
                    NotifyPropertyChanging("VenueItemId");
                    _venueItemId = value;
                    NotifyPropertyChanged("VenueItemId");
                }
            }
        }

        // Define item name: private field, public property, and database column.
        private string _venueName{ get; set; }

        [Column]
        public string VenueName
        {
            get { return _venueName; }
            set
            {
                if (_venueName != value)
                {
                    NotifyPropertyChanging("VenueName");
                    _venueName = value;
                    NotifyPropertyChanged("VenueName");
                }
            }
        }

        // Define completion value: private field, public property, and database column.
        private double _latitude { get; set; }

        [Column]
        public double Latitude
        {
            get { return _latitude; }
            set
            {
                if (_latitude != value)
                {
                    NotifyPropertyChanging("Latitude");
                    _latitude = value;
                    NotifyPropertyChanged("Latitude");
                }
            }
        }

        // Define completion value: private field, public property, and database column.
        private double _longitude { get; set; }

        [Column]
        public double Longitude
        {
            get { return _longitude; }
            set
            {
                if (_longitude != value)
                {
                    NotifyPropertyChanging("Longitude");
                    _longitude = value;
                    NotifyPropertyChanged("Longitude");
                }
            }
        }

        //
        // TODO: Add columns and associations, as applicable, here.
        //

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    public class VenueDataContext : DataContext
    {
        // Pass the connection string to the base class.
        public VenueDataContext(string connectionString)
            : base(connectionString)
        { }

        // Specify a table for the venue items.
        public Table<VenueItem> Items;
    }

}