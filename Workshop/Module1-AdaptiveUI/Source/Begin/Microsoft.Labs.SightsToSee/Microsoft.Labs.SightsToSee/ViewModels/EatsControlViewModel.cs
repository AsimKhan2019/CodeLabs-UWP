using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using System.Linq;
using Microsoft.Labs.SightsToSee.Library.Models;
using Microsoft.Labs.SightsToSee.Models;
using Microsoft.Labs.SightsToSee.Mvvm;
using System.Collections.Generic;

namespace Microsoft.Labs.SightsToSee.Views
{
    public class EatsControlViewModel : ViewModelBase
    {
        private Geopoint _centerLocation;
        private ObservableCollection<Restaurant> _eats;
        private ObservableCollection<EatsGroup> _eatsGroups;
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
            set
            {
                Set(ref _eats, value);
                BuildEatGroups();
            }
        }

        public ObservableCollection<EatsGroup> EatGroups
        {
            get { return _eatsGroups; }
            set { Set(ref _eatsGroups, value); }
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

        private void BuildEatGroups()
        {

            Restaurant r = new Restaurant();

            var grouped = from eat in Eats
                          group eat by eat.CulinaryStyle
                          into grp
                          orderby grp.Key ascending
                          select new EatsGroup
                          {
                              GroupName = grp.Key,
                              ListOfEats = grp.ToList()
                          };

            EatGroups = new ObservableCollection<EatsGroup>(grouped.ToList());
        }
    }
}

