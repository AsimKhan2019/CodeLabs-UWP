using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AppServicesClientApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                //Send data to the service 
                var token = SharedStorageAccessManager.AddFile(file);

                var message = new ValueSet();
                message.Add("Token", token);

                //var targetAppUri = new Uri("lumiaphotoediting:");
                var targetAppUri = new Uri("lumiaphotoeditingquick:");

                // We want a specific app to perform our photo editing operation, not just any that implements the protocol we're using for launch
                var options = new LauncherOptions();
                //options.TargetApplicationPackageFamilyName = "704d7940-cd63-48d5-a0dd-45b6171c41c8_tkv88av2beah0";
                options.TargetApplicationPackageFamilyName = "3ac26f24-3747-47ef-bfc5-b877b482f0f3_gd40dm16kn5w8";

                var response = await Launcher.LaunchUriForResultsAsync(targetAppUri, options, message);

                if (response.Status == LaunchUriStatus.Success)
                {
                    if (response.Result != null)
                    {
                        string alteredFileToken = response.Result["Token"].ToString();
                        var alteredFile = await SharedStorageAccessManager.RedeemTokenForFileAsync(alteredFileToken);
                        await new MessageDialog("Altered file: " + alteredFile.Path).ShowAsync();
                    }
                    else
                        await new MessageDialog("Launch success, but no data returned ").ShowAsync();

                }
            }
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                //Send data to the service 
                var token = SharedStorageAccessManager.AddFile(file);

                var message = new ValueSet();
                message.Add("DestinationToken", token);
                message.Add("Photo", "Photo");

                var targetAppUri = new Uri("editshowcase:");

                // We want a specific app to perform our photo editing operation, not just any that implements the protocol we're using for launch
                var options = new LauncherOptions();
                options.TargetApplicationPackageFamilyName = "704d7940-cd63-48d5-a0dd-45b6171c41c8_tkv88av2beah0";

                var response = await Launcher.LaunchUriForResultsAsync(targetAppUri, options, message);

                if (response.Status == LaunchUriStatus.Success)
                {
                    if (response.Result != null)
                    {
                        string alteredFileToken = response.Result["Token"].ToString();
                        var alteredFile = await SharedStorageAccessManager.RedeemTokenForFileAsync(alteredFileToken);
                        await new MessageDialog("Altered file: " + alteredFile.Path).ShowAsync();
                    }
                    else
                        await new MessageDialog("Launch success, but no data returned ").ShowAsync();

                }
            }

        }
    }
}
