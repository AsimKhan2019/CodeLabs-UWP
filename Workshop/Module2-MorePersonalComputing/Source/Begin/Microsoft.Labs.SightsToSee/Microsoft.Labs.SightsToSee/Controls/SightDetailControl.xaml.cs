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

        // Insert the M2_Recognizers snippet here


        public SightDetailControl()
        {
            InitializeComponent();
            // Insert the M2_NotesInputs snippet here

            #region Setup Recognizers
            // Insert the M2_SetupRecognizers snippet here

            #endregion

            #region ImageInkToolbarControl

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
            // Insert the M2_SetupNotes snippet here

            
        }

        #region OCR
        
        // Insert the M2_RecognizerMethods snippet here

        private void OnRecognizerChanged(object sender, RoutedEventArgs e)
        {
            //Insert the M2_RecognizerChanged snippet here
        }
        private async void OnRecognizeAsync(object sender, RoutedEventArgs e)
        {
            // Insert the M2_OnRecognize snippet here

        }

        #endregion

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

        #region NotesInkToolbar
        // Insert the M2_SaveUndo snippet here

        #endregion


        #region ImageToolbar

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

    }
}