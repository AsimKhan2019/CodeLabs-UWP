using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using Microsoft.Labs.SightsToSee.Library.Models;
using Microsoft.Labs.SightsToSee.Models;
using Microsoft.Labs.SightsToSee.Mvvm;

namespace Microsoft.Labs.SightsToSee.Views
{
    public class EatsControlViewModel : ViewModelBase
    {
        private Geopoint _centerLocation;
        private ObservableCollection<Restaurant> _eats;
        private bool _isDisplayingSightEats;
        private bool _isLoadingEats;

        public string MapServiceToken => AppSettings.MapServiceToken;

        public bool IsLoadingEats
        {
            get { return _isLoadingEats; }
            set { Set(ref _isLoadingEats, value); }
        }

        public ObservableCollection<Restaurant> Eats
        {
            get { return _eats; }
            set { Set(ref _eats, value); }
        }

        public Geopoint CenterLocation
        {
            get { return _centerLocation; }
            set { Set(ref _centerLocation, value); }
        }

        public bool IsDisplayingSightEats
        {
            get { return _isDisplayingSightEats; }
            set { Set(ref _isDisplayingSightEats, value); }
        }

        public Sight Sight { get; set; }
        public Trip Trip { get; set; }

        public string Title => IsDisplayingSightEats ? $"Here are the nearest restaurants to {Sight.Name}" : $"Here are restaurants in {Trip.Name}";
    }
}