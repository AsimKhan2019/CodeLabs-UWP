using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Geolocation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Labs.SightsToSee.Common;
using Microsoft.Labs.SightsToSee.Library.Models;
using Microsoft.Labs.SightsToSee.Library.Services.DataModelService;
using Microsoft.Labs.SightsToSee.Library.Services.SightsService;
using Microsoft.Labs.SightsToSee.Models;
using Microsoft.Labs.SightsToSee.Mvvm;
using Microsoft.Labs.SightsToSee.Services.RestaurantDataService;
using Microsoft.Labs.SightsToSee.Views;

namespace Microsoft.Labs.SightsToSee.ViewModels
{
    public class TripDetailPageViewModel : ViewModelBase
    {
        private readonly IDataModelService _dataModelService = DataModelServiceFactory.CurrentDataModelService();
        private Sight _currentSight;
        private DateTimeOffset? _currentSightDate;
        private ObservableCollection<SightFile> _currentSightFiles;
        private TimeSpan _currentSightTime;
        private Trip _currentTrip;
        private bool _isDisplay3D;
        private SightFile _selectedSightFile;
        private ObservableCollection<SightGroup> _sightGroups;
        private BitmapImage _sightImage;
        private ObservableCollection<Sight> _sights;
        private EatsControlViewModel _eatsControlViewModel;


        public TripDetailPageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                CurrentTrip = SeedDataFactory.CreateDesignTrip();
                CurrentSight = CurrentTrip.Sights[0];
                Sights =
                    new ObservableCollection<Sight>(
                        CurrentTrip.Sights.OrderBy(s => s.RankInDestination));
                BuildSightGroups();
                Debug.WriteLine("Debug mode for the model");
            }
        }

        public MapControl Map { get; set; }

        public string MapServiceToken => AppSettings.MapServiceToken;

        public Trip CurrentTrip
        {
            get { return _currentTrip; }
            set { Set(ref _currentTrip, value); }
        }

        public Sight CurrentSight
        {
            get { return _currentSight; }
            set { Set(ref _currentSight, value); }
        }

        public DateTimeOffset? CurrentSightDate
        {
            get { return _currentSightDate ?? (_currentSightDate = DateTime.Now); }
            set { Set(ref _currentSightDate, value); }
        }

        public TimeSpan CurrentSightTime
        {
            get { return _currentSightTime; }
            set { Set(ref _currentSightTime, value); }
        }

        public bool IsDisplay3D
        {
            get { return _isDisplay3D; }
            set { Set(ref _isDisplay3D, value); }
        }

        public FlyoutBase Flyout { get; set; }

        public ObservableCollection<SightGroup> SightGroups
        {
            get { return _sightGroups; }
            set { Set(ref _sightGroups, value); }
        }

        public ObservableCollection<Sight> Sights
        {
            get { return _sights; }
            set { Set(ref _sights, value); }
        }

        public BitmapImage SightImage
        {
            get { return _sightImage; }
            set { Set(ref _sightImage, value); }
        }

        public SightFile SelectedSightFile
        {
            get { return _selectedSightFile; }
            set
            {
                if (value == _selectedSightFile) return;
                Set(ref _selectedSightFile, value);
            }
        }

        public ObservableCollection<SightFile> CurrentSightFiles
        {
            get { return _currentSightFiles; }

            set
            {
                if (value == _currentSightFiles)
                    return;
                Set(ref _currentSightFiles, value);
            }
        }

        public EatsControlViewModel EatsControlViewModel
        {
            get { return _eatsControlViewModel; }
            set { Set(ref _eatsControlViewModel, value); }
        }

        public async Task ConfirmDeleteSightAsync(Guid sightId)
        {
            var sight = CurrentTrip.Sights.SingleOrDefault(s => s.Id == sightId);
            var cd = new ContentDialog
            {
                Title = "Sights2See",
                Content = $"\nAre you sure you wish to remove {sight.Name} from this trip?",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            };

            var result = await cd.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                sight.IsMySight = false;
                await UpdateSightAsync(sight);
            }
        }

        public async Task LoadTripAsync(Guid tripId, bool animateMap = true)
        {
            // As we need to have requested this permission for our background task, ask here
            await Geolocator.RequestAccessAsync();
            CurrentTrip = await _dataModelService.LoadTripAsync(tripId);
            Sights =
                new ObservableCollection<Sight>(
                    CurrentTrip.Sights.OrderBy(s => s.RankInDestination));

            BuildSightGroups();

            if (Map != null)
            {
                var boundingBox = GeoboundingBox.TryCompute(GetSightsPositions());
                if (animateMap)
                {
                    // We actually don't want to wait for the map to stop animating
                    // so we assign the task to a variable to remove the warning about await
                    var task = Map.TrySetViewBoundsAsync(boundingBox,
                        new Thickness { Left = 48, Top = 48, Right = 48, Bottom = 48 },
                        MapAnimationKind.Bow);
                }
                else
                {
                    // We actually don't want to wait for the map to stop animating
                    // so we assign the task to a variable to remove the warning about await
                    var task = Map.TrySetViewBoundsAsync(boundingBox,
                        new Thickness { Left = 48, Top = 48, Right = 48, Bottom = 48 },
                        MapAnimationKind.None);
                }
            }

            EatsControlViewModel = new EatsControlViewModel {CenterLocation = CurrentTrip.Location, Trip = CurrentTrip};
            try
            {
                EatsControlViewModel.IsLoadingEats = true;
                EatsControlViewModel.Eats =
                    new ObservableCollection<Restaurant>(await RestaurantDataService.Current.GetRestaurantsForTripAsync(CurrentTrip));
            }
            finally 
            {
                EatsControlViewModel.IsLoadingEats = false;
            }
            
            // Insert the M1_CreateTiles snippet here


        }

        private void BuildSightGroups()
        {
            var grouped = from sight in Sights
                group sight by sight.IsMySight
                into grp
                orderby grp.Key descending
                select new SightGroup
                {
                    GroupName = grp.Key ? "My sights" : "Suggested sights",
                    Sights = grp.ToList()
                };

            SightGroups = new ObservableCollection<SightGroup>(grouped.ToList());
        }

        public void ShowDetail(object sender, RoutedEventArgs e)
        {
            CurrentSight = ((Button) sender).DataContext as Sight;
            AppShell.Current.NavigateToPage(typeof (SightDetailPage), CurrentSight.Id.ToString("D"));
        }

        private IEnumerable<BasicGeoposition> GetSightsPositions()
        {
            return CurrentTrip.Sights.Select(sight => sight.Location.Position);
        }

        public async void DeleteSightAsync()
        {
            PopupManager.HideContent();
            CurrentSight.IsMySight = false;
            await UpdateSightAsync(CurrentSight);
        }

        public async void AddSightAsync()
        {
            PopupManager.HideContent();
            CurrentSight.IsMySight = true;
            await UpdateSightAsync(CurrentSight);
        }

        public async void GetDirectionsFromFlyoutAsync(object sender, RoutedEventArgs e)
        {
            var sight = ((Button) sender).DataContext as Sight;
            var mapsUri = new Uri($@"bingmaps:?rtp=~pos.{sight.Latitude}_{sight.Longitude}_{sight.Name}");

            // Launch the Windows Maps app
            var launcherOptions = new LauncherOptions();
            launcherOptions.TargetApplicationPackageFamilyName = "Microsoft.WindowsMaps_8wekyb3d8bbwe";
            await Launcher.LaunchUriAsync(mapsUri, launcherOptions);
        }


        // Insert the M1_Show3D snippet here

        

        public async void Close3D()
        {
            IsDisplay3D = false;
            Map.Style = MapStyle.Road;
            await Map.TrySetViewAsync(_currentSight.Location, 13, 0, 0);
        }


        public void SightClicked(object sender, ItemClickEventArgs args)
        {
            CurrentSight = args.ClickedItem as Sight;
            AppShell.Current.NavigateToPage(typeof (SightDetailPage), CurrentSight.Id.ToString("D"));
        }


        public async Task UpdateSightAsync(Sight sight)
        {
            await _dataModelService.SaveSightAsync(sight);
            await LoadTripAsync(_currentTrip.Id);
        }

        public async Task DisplayClosestSightAsync()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            if (accessStatus == GeolocationAccessStatus.Allowed)
            {
                // using default accuracy
                var geolocator = new Geolocator();
                var pos = await geolocator.GetGeopositionAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(5));
                if (pos != null)
                {
                    CurrentSight = await SightsHelper.FindClosestSightAsync(pos.Coordinate.Point, CurrentTrip, false);
                }
                else
                {
                    // If we can't use the location, we'll just use the first Sight
                    CurrentSight = CurrentTrip.Sights.FirstOrDefault(s => s.IsMySight);
                }
            }
            else
            {
                // If we can't use the location, we'll just use the first Sight
                CurrentSight = CurrentTrip.Sights.FirstOrDefault(s => s.IsMySight);
            }

            if (CurrentSight != null)
            {
                AppShell.Current.NavigateToPage(typeof (SightDetailPage), CurrentSight.Id.ToString("D"));
            }
        }

        public void ShowSight(Guid sightId)
        {
            CurrentSight = CurrentTrip.Sights.SingleOrDefault(s => s.Id == sightId);
            AppShell.Current.NavigateToPage(typeof (SightDetailPage), CurrentSight.Id.ToString("D"));
        }
    }
}