using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Geolocation;
using Newtonsoft.Json;
using SQLite.Net.Attributes;

namespace Microsoft.Labs.SightsToSee.Library.Models
{
    public class Restaurant : INotifyPropertyChanged
    {
        private string _address;
        private string _description;
        private string _id;
        private string _imagePath;
        private double _latitude;
        private string _longDescription;
        private double _longitude;
        private string _name;
        private string _notes;
        private int _rankInDestination;
        private int _restaurantType;
        private List<Review> _reviews;
        private string _telephone;
        private string _website;

        public int RestaurantType
        {
            get { return _restaurantType; }
            set
            {
                if (_restaurantType == value) return;
                _restaurantType = value;
                OnPropertyChanged();
            }
        }

        public string Telephone
        {
            get { return _telephone; }
            set
            {
                if (_telephone == value) return;
                _telephone = value;
                OnPropertyChanged();
            }
        }

        public string Website
        {
            get { return _website; }
            set
            {
                if (_website == value) return;
                _website = value;
                OnPropertyChanged();
            }
        }

        public List<Review> Reviews
        {
            get { return _reviews; }
            set
            {
                if (_reviews == value) return;
                _reviews = value;
                OnPropertyChanged();
            }
        }

        public string Id
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

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

        [JsonIgnore]
        [Ignore]
        public Geopoint Location => new Geopoint(new BasicGeoposition {Latitude = Latitude, Longitude = Longitude});

        [JsonIgnore]
        [Ignore]
        public string CulinaryStyle
        {
            get
            {
                string culinaryType;
                switch (RestaurantType)
                {
                    case 1:
                        culinaryType = "Chinese";
                        break;
                    case 2:
                        culinaryType = "Mexican";
                        break;
                    case 3:
                        culinaryType = "Indian";
                        break;
                    case 4:
                        culinaryType = "Seafood";
                        break;
                    case 5:
                        culinaryType = "International";
                        break;
                    case 6:
                        culinaryType = "Middle Eastern";
                        break;
                    case 7:
                        culinaryType = "Italian";
                        break;
                    case 8:
                        culinaryType = "French";
                        break;
                    case 9:
                        culinaryType = "Greek";
                        break;
                    case 10:
                        culinaryType = "Asian";
                        break;
                    case 11:
                        culinaryType = "Pizza";
                        break;
                    case 12:
                        culinaryType = "Japanese";
                        break;
                    case 13:
                        culinaryType = "Vegan and Vegetarian";
                        break;
                    case 14:
                        culinaryType = "Burgers";
                        break;
                    case 15:
                        culinaryType = "Steakhouse";
                        break;
                    case 16:
                        culinaryType = "Thai";
                        break;
                    default:
                        culinaryType = "American";
                        break;
                }

                return culinaryType;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
    public class EatsGroup
    {
        public string GroupName { get; set; }
        public List<Restaurant> ListOfEats { get; set; }
    }
}