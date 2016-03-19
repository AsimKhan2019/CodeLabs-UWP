using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Labs.SightsToSee.ViewModels;

namespace Microsoft.Labs.SightsToSee.Controls
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LandingControl : UserControl
    {
        public LandingControl()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                Destination.Focus(FocusState.Programmatic);
            };
        }

        public LandingPageViewModel ViewModel => DataContext as LandingPageViewModel;

        public bool ShowCreateFirstTrip
        {
            set
            {
                (DataContext as LandingPageViewModel).ShowCreateFirstTrip = value;
            }
        }
    }
}