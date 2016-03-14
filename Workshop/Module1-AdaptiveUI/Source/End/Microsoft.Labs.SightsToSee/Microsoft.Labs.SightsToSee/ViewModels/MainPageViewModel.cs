using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Labs.SightsToSee.Models;
using Microsoft.Labs.SightsToSee.Mvvm;
using Microsoft.Labs.SightsToSee.Services.DataModelService;

namespace Microsoft.Labs.SightsToSee.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly IDataModelService _dataModelService;
        private Command<string> _createTripCommand;
        private bool _isBusy;
        private ObservableCollection<Trip> _trips;

        public MainPageViewModel()
        {
            _dataModelService = DataModelServiceFactory.CurrentDataModelService();
        }

        public ObservableCollection<Trip> Trips
        {
            get { return _trips; }
            set
            {
                _trips = value;
                RaisePropertyChanged();
            }
        }

        public Command<string> CreateTripCommand
        {
            get
            {
                if (_createTripCommand == null)
                {
                    _createTripCommand = new Command<string>(async p => await CreateTripAsync(p));
                }
                return _createTripCommand;
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        private async Task CreateTripAsync(string tripName)
        {
            try
            {
                IsBusy = true;
                await
                    _dataModelService.InsertTripAsync(new Trip
                    {
                        Name = tripName,
                        StartDate = DateTime.Today + TimeSpan.FromDays(10),
                        // Add some dummy attractions for the time being
                        Sights = BuildTestAttractions()
                    });
                await LoadTripsWithAttractionsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static List<Sight> BuildTestAttractions()
        {
            return new List<Sight>
            {
                new Sight
                {
                    Name = "Glenwood Canyon",
                    ImagePath =
                        "https://upload.wikimedia.org/wikipedia/commons/thumb/f/ff/Glencan.JPG/800px-Glencan.JPG",
                    Latitude = 39.575,
                    Longitude = -107.223,
                    Description =
                        "Glenwood Canyon is a rugged scenic 12.5 mi (20 km) canyon on the Colorado River in western Colorado in the United States. Its walls climb as high as 1,300 ft (396 m) above the Colorado River"
                },
                new Sight
                {
                    Name = "Hanging Lake",
                    ImagePath = "http://internetbrothers.org/images/hanging_lake_falls.jpg",
                    Latitude = 39.6016,
                    Longitude = -107.192,
                    Description =
                        "Hanging Lake is a lake in the U.S. State of Colorado. It is located in Glenwood Canyon, about 7 miles (11 km) east of Glenwood Springs, Colorado and is a very popular tourist destination."
                },
                new Sight
                {
                    Name = "Glenwood Caverns Adventure Park",
                    ImagePath =
                        "http://media-cdn.tripadvisor.com/media/photo-o/0a/14/a2/da/20160117-131839-largejpg.jpg",
                    Latitude = 39.5605,
                    Longitude = -107.3202,
                    Description =
                        "In Glenwood Springs you can explore stunning caverns and formations... in Colorado's largest showcave."
                }
            };
        }

        public async Task LoadTripsAsync()
        {
            try
            {
                IsBusy = true;
                Trips = new ObservableCollection<Trip>(await _dataModelService.LoadTripsAsync());
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadTripsWithAttractionsAsync()
        {
            try
            {
                IsBusy = true;
                Trips = new ObservableCollection<Trip>(await _dataModelService.LoadTripsWithAttractionsAsync());
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}