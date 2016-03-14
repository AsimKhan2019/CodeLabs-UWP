using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Labs.SightsToSee.Common;
using Microsoft.Labs.SightsToSee.Controls;
using Microsoft.Labs.SightsToSee.Models;
using Microsoft.Labs.SightsToSee.Mvvm;
using Microsoft.Labs.SightsToSee.Services.DataModelService;
using Microsoft.Labs.SightsToSee.Views;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Microsoft.Labs.SightsToSee.ViewModels
{
    public class TripDetailPageViewModel : ViewModelBase
    {
        private readonly IDataModelService _dataModelService = DataModelServiceFactory.CurrentDataModelService();
        private Sight _currentSight;
        private Trip _currentTrip;
        private bool _isDisplay3D;
        private SightFile _selectedSightFile;
        private SightDetailControl _sightDetailControl;
        private ObservableCollection<SightGroup> _sightGroups;
        private BitmapImage _sightImage;
        private ObservableCollection<Sight> _sights;


        public MapControl Map { get; set; }

        public string MapServiceToken
            =>
                "7H7lMjEkAfP3PeOrrPVO~IRTU1f4lP6GTdpBxi4gqoQ~AvvbAnSGbHtsowQ98zRfwvaw6PdCgo2vq3x75R3_SbvN2zb7-YcaM_UIPNtNWOWK"
            ;

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
            set { Set(ref _selectedSightFile, value); }
        }

        public async Task LoadTripAsync(Guid tripId)
        {
            CurrentTrip = await _dataModelService.LoadTripAsync(tripId);
            Sights =
                new ObservableCollection<Sight>(
                    CurrentTrip.Sights.OrderBy(s => s.RankInDestination));

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
            if (Map != null)
            {
                var boundingBox = GeoboundingBox.TryCompute(GetSightsPositions());
                await
                    Map.TrySetViewBoundsAsync(boundingBox,
                        new Thickness {Left = 48, Top = 48, Right = 48, Bottom = 48},
                        MapAnimationKind.Bow);
            }

            // Set interactive tiles on load
        }


        public void GalleryItemClicked(object sender, SelectionChangedEventArgs e)
        {
            SelectedSightFile = ((GridView) sender).SelectedItem as SightFile;
            if (SelectedSightFile == null) return;

            SightImage = SelectedSightFile.ImageUri;
            if (_sightDetailControl != null)
            {
                _sightDetailControl.BackgroundImage = SightImage;
            }
        }

        public async void ShowDetail(object sender, RoutedEventArgs e)
        {
            Flyout?.Hide();
            // sender is the button - and the data context is the Sight
            _currentSight = ((Button) sender).DataContext as Sight;

            if (_currentSight == null)
                return;

            await ShowDetailDialog();
        }

        private async Task ShowDetailDialog()
        {
            _sightDetailControl = new SightDetailControl
            {
                BackgroundImage = _currentSight.ImageUri,
                DataContext = this
            };
            PopupManager.ShowContent(_sightDetailControl);
        }

        private IEnumerable<BasicGeoposition> GetSightsPositions()
        {
            return CurrentTrip.Sights.Select(sight => sight.Location.Position);
        }

        public async void DeleteSight()
        {
            PopupManager.HideContent();
            _currentSight.IsMySight = false;
            await UpdateSight(_currentSight);
        }

        public async void AddSight()
        {
            PopupManager.HideContent();
            _currentSight.IsMySight = true;
            await UpdateSight(_currentSight);
        }


        public async void ShareSight()
        {
        }

        public async void SightClicked(object sender, ItemClickEventArgs args)
        {
            _currentSight = args.ClickedItem as Sight;
            await ShowSight();
        }

        private async Task ShowSight()
        {
            if (_currentSight == null)
                return;
            Flyout?.Hide();
            SightImage = _currentSight.ImageUri;
            await ShowDetailDialog();
        }

        public async Task UpdateSight(Sight sight)
        {
            await _dataModelService.SaveSightAsync(sight);
            await LoadTripAsync(_currentTrip.Id);
        }

        public void Photo_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;

            // Customize the look of the DragUI
            if (e.DragUIOverride != null)
            {
                e.DragUIOverride.Caption = "Attach photo";
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = true;
                e.DragUIOverride.IsGlyphVisible = true;
            }
        }

        public async void Photo_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();

                if (items.Any())
                {
                    foreach (var storageFile in items.Select(file => file as StorageFile))
                    {
                        //await AddPhotoComplete(storageFile);
                    }
                }
            }
        }

        //private async Task AddPhotoComplete(StorageFile file)
        //{
        //    // add to record
        //    var photo = new SightFile
        //    {
        //        Id = Guid.NewGuid(),
        //        Sight = CurrentSight,
        //        FileType = 0,
        //        FileName = file.Name,
        //        Uri = file.Path,
        //};

        //    await CopyAndAssignImageAsync(file);

        //}

        //public async Task CopyAndAssignImageAsync(StorageFile file)
        //{
        //    var copiedFile = await file.CopyAsync(
        //        ApplicationData.Current.LocalFolder,
        //        Id.ToString(),
        //        NameCollisionOption.ReplaceExisting);

        //    Path = copiedFile.Path;
        //    using (var fileStream = await copiedFile.OpenAsync(FileAccessMode.Read))
        //    {
        //        // Set the image source to the selected bitmap 
        //        var imageSource = new BitmapImage();
        //        await imageSource.SetSourceAsync(fileStream);
        //        ImageSource = imageSource;
        //    }
        //}
    }
}