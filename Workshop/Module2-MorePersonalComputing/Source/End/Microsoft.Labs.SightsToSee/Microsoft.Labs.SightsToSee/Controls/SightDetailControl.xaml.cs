using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Labs.SightsToSee.ViewModels;
using Windows.UI.Input.Inking;
using System;
using System.Collections.Generic;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Labs.SightsToSee.Controls
{
    public sealed partial class SightDetailControl : UserControl
    {
        public static readonly DependencyProperty BackgroundImageProperty = DependencyProperty.Register(
            "BackgroundImage", typeof (BitmapImage), typeof (SightDetailControl),
            new PropertyMetadata(default(BitmapImage), BackgroundImagePropertyChanged));

        InkRecognizerContainer inkRecognizerContainer = null;
        private IReadOnlyList<InkRecognizer> recoView = null;


        public SightDetailControl()
        {
            InitializeComponent();
            Loaded += TripDetailPopup_Loaded;
            InkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                                                      Windows.UI.Core.CoreInputDeviceTypes.Pen |
                                                      Windows.UI.Core.CoreInputDeviceTypes.Touch;
            inkRecognizerContainer = new InkRecognizerContainer();
            recoView = inkRecognizerContainer.GetRecognizers();
            if (recoView.Count > 0)
            {
                foreach (InkRecognizer recognizer in recoView)
                {
                    RecoName.Items.Add(recognizer.Name);
                }
            }
            else
            {
                RecoName.IsEnabled = false;
                RecoName.Items.Add("No Recognizer Available");
            }
            RecoName.SelectedIndex = 0;
        }


        public BitmapImage BackgroundImage
        {
            get { return (BitmapImage) GetValue(BackgroundImageProperty); }
            set { SetValue(BackgroundImageProperty, value); }
        }

        public TripDetailPageViewModel ViewModel => DataContext as TripDetailPageViewModel;

        private void TripDetailPopup_Loaded(object sender, RoutedEventArgs e)
        {
            // in this example we assume the parent of the UserControl is a Popup 
            var p = Parent as Popup;
            if (p == null)
            {
                return;
            }
            RePositionPopup(p);
        }

        private void RePositionPopup(Popup p)
        {
            var horizontalOffset = (p.ActualWidth - ActualWidth)/2;
            var verticalOffset = (p.ActualHeight - ActualHeight)/2;
            p.HorizontalOffset = horizontalOffset < 0 ? 0 : horizontalOffset;
            p.VerticalOffset = verticalOffset < 0 ? 0 : verticalOffset;
        }


        private static void BackgroundImagePropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var sightDetailControl = dependencyObject as SightDetailControl;
            if (sightDetailControl == null) return;
            var newImage = dependencyPropertyChangedEventArgs.NewValue as BitmapImage;

            if (sightDetailControl.BackgroundControl != null && newImage != null)
            {
                sightDetailControl.BackgroundControl.BlurFactor = 30;
                sightDetailControl.BackgroundControl.BackgroundImageSource = newImage;
            }
        }

        private void GeneralDialogClose_OnClick(object sender, RoutedEventArgs e)
        {
            // in this example we assume the parent of the UserControl is a Popup 
            var p = Parent as Popup;

            // close the Popup
            if (p != null)
            {
                p.IsOpen = false;
            }
        }

        private void EnableInk(object sender, RoutedEventArgs e)
        {
            //ViewModel.CurrentSight.IsInkEnabled = true;
            Notes.Visibility = Visibility.Collapsed;
            InkButton.Visibility = Visibility.Collapsed;

            InkGrid.Visibility = Visibility.Visible;
            OCRButton.Visibility = Visibility.Visible;
            ClearButton.Visibility = Visibility.Visible;
        }

        private void DisableInk(string notes)
        {
            //ViewModel.CurrentSight.IsInkEnabled = false;
            Notes.Visibility = Visibility.Visible;
            InkButton.Visibility = Visibility.Visible;

            InkGrid.Visibility = Visibility.Collapsed;
            OCRButton.Visibility = Visibility.Collapsed;
            ClearButton.Visibility = Visibility.Collapsed;

            Notes.Text = notes;
        }

        private async void TryOCR(object sender, RoutedEventArgs e)
        {
            //var dialog = new ContentDialog()
            //{
            //    Title = "Speech to Text",
            //    MaxWidth = this.ActualWidth,
            //    Content = "asdf"
            //};
            //dialog.PrimaryButtonText = "Accept";
            //dialog.IsPrimaryButtonEnabled = true;

            //var result = await dialog.ShowAsync();

            ContentDialogResult result = await OCRDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                DisableInk(Status.Text);

            }
            else
            {
                //User pressed Cancel or the back arrow
            }
        }
        private void ClearInkCanvas(object sender, RoutedEventArgs e)
        {
            InkCanvas.InkPresenter.StrokeContainer.Clear();
        }

        private void OnRecognizerChanged(object sender, RoutedEventArgs e)
        {
            string selectedValue = (string)RecoName.SelectedValue;
            Status.Text = "";
            SetRecognizerByName(selectedValue);
        }

        private bool SetRecognizerByName(string recognizerName)
        {
            bool recognizerFound = false;

            foreach (InkRecognizer reco in recoView)
            {
                if (recognizerName == reco.Name)
                {
                    inkRecognizerContainer.SetDefaultRecognizer(reco);
                    recognizerFound = true;
                    break;
                }

                if (!recognizerFound)
                {
                    Status.Text = "Could not find target recognizer.";
                }

            }
            return recognizerFound;

        }
        async void OnRecognizeAsync(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<InkStroke> currentStrokes =
                InkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            if (currentStrokes.Count > 0)
            {
                //RecognizeBtn.IsEnabled = false;
                //ClearBtn.IsEnabled = false;
                RecoName.IsEnabled = false;

                var recognitionResults = await inkRecognizerContainer.RecognizeAsync(
                    InkCanvas.InkPresenter.StrokeContainer,
                    InkRecognitionTarget.All);

                if (recognitionResults.Count > 0)
                {
                    // Display recognition result
                    string str = "";
                    foreach (var r in recognitionResults)
                    {
                        str += r.GetTextCandidates()[0];
                    }
                    Status.Text = str;
                    OCRDialog.IsPrimaryButtonEnabled = true;
                }
                else
                {
                    Status.Text = "No text recognized.";
                }

                //RecognizeBtn.IsEnabled = true;
                //ClearBtn.IsEnabled = true;
                RecoName.IsEnabled = true;
            }
            else
            {
                Status.Text = "Must first write something.";
            }
        }

    }
}