using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Labs.SightsToSee.ViewModels;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Labs.SightsToSee.Controls
{
    public sealed partial class SightDetailControl : UserControl
    {
        public static readonly DependencyProperty BackgroundImageProperty = DependencyProperty.Register(
            "BackgroundImage", typeof (BitmapImage), typeof (SightDetailControl),
            new PropertyMetadata(default(BitmapImage), BackgroundImagePropertyChanged));

        public SightDetailControl()
        {
            InitializeComponent();
            Loaded += TripDetailPopup_Loaded;
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
    }
}