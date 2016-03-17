using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Labs.SightsToSee.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Microsoft.Labs.SightsToSee.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewTripPage : Page
    {
        public NewTripPage()
        {
            InitializeComponent();
            Loaded += async (sender, args) =>
            {
                var dialog = new ContentDialog
                {
                    Title = "New Trip",
                    Content = "This feature has not yet been implemented",
                    PrimaryButtonText = "Go Back"
                };
                await dialog.ShowAsync();
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            };
        }
    }
}