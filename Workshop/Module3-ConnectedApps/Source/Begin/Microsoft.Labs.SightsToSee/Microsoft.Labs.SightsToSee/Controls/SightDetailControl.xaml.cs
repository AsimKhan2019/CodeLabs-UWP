using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using InkToolbarPreview;
using Microsoft.Labs.SightsToSee.Services.FileService;
using Microsoft.Labs.SightsToSee.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Microsoft.Labs.SightsToSee.Library.Models;
using Microsoft.Labs.SightsToSee.Library.Services.DataModelService;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Labs.SightsToSee.Controls
{
    public sealed partial class SightDetailControl : UserControl
    {
        public static readonly DependencyProperty BackgroundImageProperty = DependencyProperty.Register(
            "BackgroundImage", typeof(BitmapImage), typeof(SightDetailControl),
            new PropertyMetadata(default(BitmapImage), BackgroundImagePropertyChanged));


        private readonly InkPresenter _imageInkPresenter;

        #region InkToolBar

        private readonly InkBitmapRenderer _inkBitmapRenderer = new InkBitmapRenderer();

        #endregion

        private readonly InkRecognizerContainer _inkRecognizerContainer;
        private readonly IReadOnlyList<InkRecognizer> _recoView;

        public SightDetailControl()
        {
            InitializeComponent();
            NotesInkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse |
                                                           CoreInputDeviceTypes.Pen |
                                                           CoreInputDeviceTypes.Touch;
            _inkRecognizerContainer = new InkRecognizerContainer();
            _recoView = _inkRecognizerContainer.GetRecognizers();
            if (_recoView != null)
            {
                if (_recoView.Count > 0)
                {
                    foreach (var recognizer in _recoView)
                    {
                        RecoName?.Items?.Add(recognizer.Name);
                    }
                }
                else
                {
                    RecoName.IsEnabled = false;
                    RecoName?.Items?.Add("No Recognizer Available");
                }
            }
            RecoName.SelectedIndex = 0;

            #region InkToolbarControl

            _imageInkPresenter = ImageInkCanvas.InkPresenter;
            _imageInkPresenter.InputDeviceTypes =
                CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Pen;

            #endregion
        }


        public BitmapImage BackgroundImage
        {
            get { return (BitmapImage)GetValue(BackgroundImageProperty); }
            set { SetValue(BackgroundImageProperty, value); }
        }

        public SightDetailPageViewModel ViewModel => DataContext as SightDetailPageViewModel;

        public async Task SetupNotesInkAsync()
        {
            // Check to see if we are in ink mode
            if (ViewModel.CurrentSight.NotesAreInk)
            {
                ViewModel.IsNotesInking = true;

                if (!string.IsNullOrWhiteSpace(ViewModel.CurrentSight.InkFilePath))
                {
                    var file = await StorageFile.GetFileFromPathAsync(ViewModel.CurrentSight.InkFilePath);
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await NotesInkCanvas.InkPresenter.StrokeContainer.LoadAsync(stream);
                    }
                }
            }
        }

        private static void BackgroundImagePropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var sightDetailControl = dependencyObject as SightDetailControl;
            if (sightDetailControl == null) return;
            var newImage = dependencyPropertyChangedEventArgs.NewValue as BitmapImage;

            if (sightDetailControl.BackgroundControl != null && newImage != null)
            {
                sightDetailControl.BackgroundControl.BlurFactor = 20;
                sightDetailControl.BackgroundControl.BackgroundImageSource = newImage;
            }
        }


        private async void TryOCR(object sender, RoutedEventArgs e)
        {
            var result = await OCRDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.IsNotesInking = false;
                ViewModel.CurrentSight.NotesAreInk = false;
                // The last note is followed by a space.
                ViewModel.CurrentSight.Notes += $" {Status.Text}";
                await ViewModel.UpdateSightAsync(ViewModel.CurrentSight);
                Status.Text = string.Empty;
                NotesInkCanvas.InkPresenter.StrokeContainer.Clear();
            }
        }

        private void OnRecognizerChanged(object sender, RoutedEventArgs e)
        {
            var selectedValue = (string)RecoName.SelectedValue;
            Status.Text = string.Empty;
            SetRecognizerByName(selectedValue);
        }

        private bool SetRecognizerByName(string recognizerName)
        {
            var recognizerFound = false;

            foreach (var reco in _recoView)
            {
                if (recognizerName == reco.Name)
                {
                    _inkRecognizerContainer.SetDefaultRecognizer(reco);
                    recognizerFound = true;
                    break;
                }

                if (!recognizerFound)
                {
                    Status.Text = $"Could not find target recognizer: {recognizerName}.";
                }
            }
            return recognizerFound;
        }

        private async void OnRecognizeAsync(object sender, RoutedEventArgs e)
        {
            var currentStrokes =
                NotesInkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            if (currentStrokes.Count > 0)
            {
                RecoName.IsEnabled = false;

                var recognitionResults = await _inkRecognizerContainer.RecognizeAsync(
                    NotesInkCanvas.InkPresenter.StrokeContainer,
                    InkRecognitionTarget.All);

                if (recognitionResults.Count > 0)
                {
                    // Display recognition result
                    var str = string.Empty;
                    foreach (var r in recognitionResults)
                    {
                        str += $"{r.GetTextCandidates()[0]} ";
                    }
                    Status.Text = str;
                    OCRDialog.IsPrimaryButtonEnabled = true;
                }
                else
                {
                    Status.Text = "No text recognized.";
                }

                RecoName.IsEnabled = true;
            }
            else
            {
                Status.Text = "Must first write something.";
            }
        }


        private async void NotesSaveButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentSight.NotesAreInk = true;
            var file = await ViewModel.GenerateStorageFileForInk();
            ViewModel.CurrentSight.InkFilePath = file.Path;
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await NotesInkCanvas.InkPresenter.StrokeContainer.SaveAsync(stream);
            }
            await ViewModel.UpdateSightAsync(ViewModel.CurrentSight);
        }

        private async void NotesUndoButton_Click(object sender, RoutedEventArgs e)
        {
            NotesInkCanvas.InkPresenter.StrokeContainer.Clear();
            ViewModel.CurrentSight.NotesAreInk = false;
            ViewModel.IsNotesInking = false;
            await ViewModel.UpdateSightAsync(ViewModel.CurrentSight);
        }

        #region InkToolBarControl

        private async void ImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            var renderedImage = await _inkBitmapRenderer.RenderAsync(
                _imageInkPresenter.StrokeContainer.GetStrokes(),
                ImageInkCanvas.ActualWidth,
                ImageInkCanvas.ActualHeight);

            if (renderedImage != null)
            {
                // Convert to a format appropriate for SoftwareBitmapSource.
                var convertedImage = SoftwareBitmap.Convert(
                    renderedImage,
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied
                    );
                await InkImageSource.SetBitmapAsync(convertedImage);

                var renderTargetBitmap = new RenderTargetBitmap();
                var currentDpi = DisplayInformation.GetForCurrentView().LogicalDpi;

                // Prepare for RenderTargetBitmap by hiding the InkCanvas and displaying the
                // rasterized strokes instead.
                ImageInkCanvas.Visibility = Visibility.Collapsed;
                InkImage.Visibility = Visibility.Visible;

                await renderTargetBitmap.RenderAsync(InkingRoot);
                var pixelData = await renderTargetBitmap.GetPixelsAsync();

                // Restore the original layout now that we have created the RenderTargetBitmap image.
                ImageInkCanvas.Visibility = Visibility.Visible;
                InkImage.Visibility = Visibility.Collapsed;

                var file = await ViewModel.GenerateStorageFileForInk();
                if (file != null)
                {
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                        encoder.SetPixelData(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Ignore,
                            (uint)renderTargetBitmap.PixelWidth,
                            (uint)renderTargetBitmap.PixelHeight,
                            currentDpi,
                            currentDpi,
                            pixelData.ToArray()
                            );

                        await encoder.FlushAsync();
                    }

                    await ViewModel.UpdateSightFileImageUriAsync(file.GetUri());

                    // Erase all strokes.
                    ImageInkCanvas.InkPresenter.StrokeContainer.Clear();
                }
            }
        }

        private void EraserClearAll(InkToolbar sender, object args)
        {
            // Erase all strokes.
            sender.TargetInkCanvas.InkPresenter.StrokeContainer.Clear();
        }

        private async void ImageUndoButton_Click(object sender, RoutedEventArgs e)
        {
            // Erase all strokes.
            ImageInkCanvas.InkPresenter.StrokeContainer.Clear();

            await ViewModel.RevertSightFileToOriginalUriAsync();
        }

        #endregion

        private async void OnLaunchForResults(object sender, RoutedEventArgs e)
        {
            // M3: Insert the M3_OpenPicker snippet here


            // M3: Insert the M3_LaunchForResults snippet here

        }
    }
}