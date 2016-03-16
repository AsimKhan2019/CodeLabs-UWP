using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Media.Imaging;
using SQLite.Net.Attributes;


namespace Microsoft.Labs.SightsToSee.Library.Models
{
    public class Sight : IGuidTable, INotifyPropertyChanged
    {
        private Guid _id;

        [PrimaryKey]
        public Guid Id
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _name;

        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        private bool _notesAreInk;

        public bool NotesAreInk
        {
            get { return _notesAreInk; }
            set
            {
                if (_notesAreInk == value) return;
                _notesAreInk = value;
                OnPropertyChanged();
            }
        }

        private double _latitude;

        public double Latitude
        {
            get { return _latitude; }
            set
            {
                if (_latitude == value) return;
                _latitude = value;
                OnPropertyChanged();
            }
        }

        private double _longitude;

        public double Longitude
        {
            get { return _longitude; }
            set
            {
                if (_longitude == value) return;
                _longitude = value;
                OnPropertyChanged();
            }
        }

        [Ignore]
        public Geopoint Location => new Geopoint(new BasicGeoposition {Latitude = Latitude, Longitude = Longitude});

        private string _address;

        public string Address
        {
            get { return _address; }
            set
            {
                if (_address == value) return;
                _address = value;
                OnPropertyChanged();
            }
        }

        private string _imagePath;

        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                if (_imagePath == value) return;
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        private string _inkFilePath;

        public string InkFilePath
        {
            get { return _inkFilePath; }
            set
            {
                if (_inkFilePath == value) return;
                _inkFilePath = value;
                OnPropertyChanged();
            }
        }

        [Ignore]
        public BitmapImage ImageUri => new BitmapImage(new Uri(ImagePath, UriKind.Absolute));

        private string _description;

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
                OnPropertyChanged();
            }
        }


        private string _longDescription;

        [System.ComponentModel.DataAnnotations.MaxLength(1000)]
        public string LongDescription
        {
            get { return _longDescription; }
            set
            {
                if (_longDescription == value) return;
                _longDescription = value;
                OnPropertyChanged();
            }
        }

        private string _notes;

        [System.ComponentModel.DataAnnotations.MaxLength(2000)]
        public string Notes
        {
            get { return _notes; }
            set
            {
                if (_notes == value) return;
                _notes = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _visitDate;

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "Date")]
        public DateTime? VisitDate
        {
            get { return _visitDate; }
            set
            {
                if (_visitDate == value) return;
                _visitDate = value;
                OnPropertyChanged();
            }
        }

        [Ignore]
        public Trip Trip { get; set; }

        public Guid TripId { get; set; }

        [Ignore]
        public List<SightFile> SightFiles { get; set; }

        private int _rankInDestination;

        public int RankInDestination
        {
            get { return _rankInDestination; }
            set
            {
                if (_rankInDestination == value) return;
                _rankInDestination = value;
                OnPropertyChanged();
            }
        }

        private bool _isMySight;

        public bool IsMySight
        {
            get { return _isMySight; }
            set
            {
                if (_isMySight == value) return;
                _isMySight = value;
                OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}