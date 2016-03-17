using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Labs.SightsToSee.Library.Models;
using Microsoft.Labs.SightsToSee.Library.Services.DataModelService;
using Microsoft.Labs.SightsToSee.Mvvm;
using Microsoft.Labs.SightsToSee.Services.TilesNotificationsService;

namespace Microsoft.Labs.SightsToSee.ViewModels
{
    public class SightDetailPageViewModel : ViewModelBase
    {
        private const string SightFilesPath = "SightFiles";
        private readonly IDataModelService _dataModelService = DataModelServiceFactory.CurrentDataModelService();
        private Sight _currentSight;
        private DateTimeOffset? _currentSightDate;
        private ObservableCollection<SightFile> _currentSightFiles;
        private TimeSpan _currentSightTime;
        private SightFile _selectedSightFile;
        private BitmapImage _sightImage;
        private bool _isNotesInking;

        public bool IsNotesInking
        {
            get { return _isNotesInking; }
            set
            {
                if (_isNotesInking == value) return;
                Set(ref _isNotesInking, value);
            }
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

        public Sight CurrentSight
        {
            get { return _currentSight; }
            set { Set(ref _currentSight, value); }
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

        public SightFile SelectedSightFile
        {
            get { return _selectedSightFile; }
            set
            {
                if (value == _selectedSightFile) return;
                Set(ref _selectedSightFile, value);
            }
        }

        public BitmapImage SightImage
        {
            get { return _sightImage; }
            set { Set(ref _sightImage, value); }
        }


        public async Task LoadSightAsync(Guid sightId)
        {
            CurrentSight = await _dataModelService.LoadSightAsync(sightId);
            CurrentSightFiles = new ObservableCollection<SightFile>();
            SightImage = CurrentSight.ImageUri;
            foreach (var sightFile in CurrentSight.SightFiles)
            {
                // Only add Image files to the CurrentSightFiles list, not inking
                if (sightFile.FileType == SightFileType.Image)
                {
                    CurrentSightFiles.Add(sightFile);
                }
            }

            SelectedSightFile = CurrentSightFiles.FirstOrDefault();
        }

        public async void DeleteSightAsync()
        {
            CurrentSight.IsMySight = false;
            await UpdateSightAsync(CurrentSight);
        }

        public async Task UpdateSightAsync(Sight sight)
        {
            await _dataModelService.SaveSightAsync(sight);
            await LoadSightAsync(sight.Id);
        }

        public void SightFile_DragOver(object sender, DragEventArgs e)
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

        public async void SightFile_DropAsync(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();

                if (items.Any())
                {
                    foreach (var storageFile in items.Select(file => file as StorageFile))
                    {
                        // copy to the required folder and add a corresponding SightFile record
                        Guid newSightFileId = Guid.NewGuid();
                        StorageFolder sightFolder = await CreateStorageFolderForSightFile(newSightFileId);

                        var copiedFile = await storageFile.CopyAsync(
                            sightFolder,
                            storageFile.Name,
                            NameCollisionOption.ReplaceExisting);

                        await AddSightFileAsync(copiedFile, newSightFileId, SightFileType.Image);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new Image or Inking File to the Sight
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileType">0: Image, 1: Inking</param>
        /// <returns></returns>
        private async Task AddSightFileAsync(StorageFile file, Guid sightFileId, SightFileType fileType)
        {
            // add to record
            var id = sightFileId;
            var sightFile = new SightFile
            {
                Id = id,
                FileType = fileType,
                Sight = CurrentSight,
                SightId = CurrentSight.Id,
                FileName = file.Name,
                Uri = "ms-appdata:///local/" + SightFilesPath + "/" + id.ToString() + "/" + file.Name,
            };

            CurrentSight.SightFiles.Add(sightFile);
            CurrentSightFiles.Add(sightFile);
            await _dataModelService.SaveSightFileAsync(sightFile);
        }

        public void GalleryItemClicked(object sender, SelectionChangedEventArgs e)
        {
            var view = sender as GridView;
            if (view != null)
            {
                SelectedSightFile = view.SelectedItem as SightFile;
            }

            var listView = sender as ListView;
            if (listView != null)
            {
                SelectedSightFile = listView.SelectedItem as SightFile;
            }

            if (SelectedSightFile == null) return;

            SightImage = SelectedSightFile.ImageUri;
        }

        public async void AddSightAsync()
        {
            // Add CurrentSightTime and schedule toast notification
            if (CurrentSightDate.HasValue)
                CurrentSight.VisitDate = CurrentSightDate.Value.Date.Add(CurrentSightTime);

            ScheduledNotificationService.AddToastReminder(CurrentSight);

            CurrentSight.IsMySight = true;
            await UpdateSightAsync(CurrentSight);
        }


        public void ShareSight()
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;

            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            request.Data.Properties.Title = $"I'm visiting the {CurrentSight.Name}";
            request.Data.Properties.Description = $"{CurrentSight.Description}";
            request.Data.SetText($"{CurrentSight.Description}");

            var localImage = SightImage.UriSource.AbsoluteUri;
            string htmlPayload = $"<img src=\"{localImage}\" width=\"200\"/><p>{CurrentSight.Description}</p>";
            var htmlFormat = HtmlFormatHelper.CreateHtmlFormat(htmlPayload);
            request.Data.SetHtmlFormat(htmlFormat);

            // Because the HTML contains a local image, we need to add it to the ResourceMap.
            var streamRef = RandomAccessStreamReference.CreateFromUri(new Uri(localImage));
            request.Data.ResourceMap[localImage] = streamRef;
        }

        public async void GetDirectionsAsync()
        {
            var mapsUri =
                new Uri($@"bingmaps:?rtp=~pos.{CurrentSight.Latitude}_{CurrentSight.Longitude}_{CurrentSight.Name}");

            // Launch the Windows Maps app
            var launcherOptions = new LauncherOptions();
            launcherOptions.TargetApplicationPackageFamilyName = "Microsoft.WindowsMaps_8wekyb3d8bbwe";
            await Launcher.LaunchUriAsync(mapsUri, launcherOptions);
        }

        /// <summary>
        /// Create the file to store inked notes on the main Sight
        /// </summary>
        /// <returns></returns>
        public async Task<StorageFile> GenerateStorageFileForInk()
        {
            Guid newSightFileId = Guid.NewGuid();
            StorageFolder sightFolder = await CreateStorageFolderForSightFile(newSightFileId);
            var inkFile = await
                    sightFolder.CreateFileAsync($"{Guid.NewGuid().ToString("D")}.png",
                        CreationCollisionOption.GenerateUniqueName);
            await AddSightFileAsync(inkFile, newSightFileId, SightFileType.General);

            return inkFile;
        }

        private async Task<StorageFolder> CreateStorageFolderForSightFile(Guid sightFileId)
        {
            // Physical file needs to go to {localfolder}/SightFiles/{sightFileId}/
            var sightFilesFolder = await
                    ApplicationData.Current.LocalFolder.CreateFolderAsync(SightFilesPath,
                        CreationCollisionOption.OpenIfExists);
            var sightFolder = await sightFilesFolder.CreateFolderAsync(sightFileId.ToString("D"),
                CreationCollisionOption.OpenIfExists);
            return sightFolder;
        }

        public async Task UpdateSightFileImageUriAsync(Uri imageUri)
        {
            // make sure we don't overwrite the original uri
            if (string.IsNullOrWhiteSpace(SelectedSightFile.OriginalUri))
            {
                SelectedSightFile.OriginalUri = SelectedSightFile.Uri;
            }

            SelectedSightFile.Uri = imageUri.ToString();
            await UpdateSelectedSightFileAsync();
        }

        public async Task RevertSightFileToOriginalUriAsync()
        {
            SelectedSightFile.Uri = SelectedSightFile.OriginalUri;
            await UpdateSelectedSightFileAsync();
        }

        public async Task UpdateSelectedSightFileAsync()
        {
            await _dataModelService.UpdateSightFileAsync(SelectedSightFile);
            SightImage = SelectedSightFile.ImageUri;
        }

        public void EnableInk()
        {
            IsNotesInking = true;
        }
    }
}